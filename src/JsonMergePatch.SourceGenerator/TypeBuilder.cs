﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class TypeBuilder : ITypeBuilder
    {
        private const string Namespace = "LaDeak.JsonMergePatch.Generated";

        public GeneratedWrapper BuildWrapperType(ITypeSymbol typeInfo, string sourceTypeName)
        {
            var name = GetName(typeInfo);
            var state = InitializeState(typeInfo, name, sourceTypeName);
            BuildFile(state);
            return new GeneratedWrapper()
            {
                FileName = $"LaDeakJsonMergePatch{GetNamespaceExtension(typeInfo)}{name}",
                SourceCode = state.Builder.ToString(),
                SourceTypeFullName = sourceTypeName,
                TargetTypeFullName = GetFullName(typeInfo),
                ToProcessTypes = state.ToProcessTypeSymbols
            };
        }

        private string GetName(ITypeSymbol typeInfo) => $"{typeInfo.Name}Wrapped";

        private string GetNamespaceExtension(ITypeSymbol typeInfo) => $"S{typeInfo.ContainingNamespace.ToDisplayString()}";

        private string GetFullName(ITypeSymbol typeInfo) => $"{Namespace}.{GetNamespaceExtension(typeInfo)}.{GetName(typeInfo)}";

        private void BuildFile(BuilderState state) => BuildNameSpace(state, BuildClass);

        private void BuildClass(BuilderState state) => BuildClassDeclaration(state, s => BuildClassBody(s));

        private BuilderState InitializeState(ITypeSymbol typeInfo, string name, string sourceTypeName)
        {
            var typeInformation = new TypeInformation() { Name = name, SourceTypeName = sourceTypeName, TypeSymbol = typeInfo };

            var currentType = typeInfo;
            while (currentType != null && currentType.Name != "Object")
            {
                typeInformation.Properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>()
                    .Where(x => !x.IsReadOnly && !x.IsWriteOnly && !x.IsIndexer && !x.IsStatic && !x.IsAbstract && !x.IsVirtual).Select(x => new PropertyInformation() { Property = x, IsConvertedToNullableType = false }));
                currentType = currentType.BaseType;
            }

            return new BuilderState(typeInformation);
        }

        private void BuildNameSpace(BuilderState state, Action<BuilderState>? addBody = null)
        {
            state.AppendLine($"namespace {Namespace}.{GetNamespaceExtension(state.TypeInfo.TypeSymbol)}");
            state.AppendLine("{");
            addBody?.Invoke(state.IncrementIdentation());
            state.AppendLine("}");
        }

        private void BuildClassDeclaration(BuilderState state, Action<BuilderState>? addBody = null)
        {
            BuildAttributes(state, state.TypeInfo.TypeSymbol.GetAttributes());
            state.AppendLine($"public class {state.TypeInfo.Name} : LaDeak.JsonMergePatch.Abstractions.Patch<{state.TypeInfo.SourceTypeName}>");
            state.AppendLine("{");
            addBody?.Invoke(state.IncrementIdentation());
            state.AppendLine("}");
        }

        private void BuildConstructor(BuilderState state, Action<BuilderState>? addBody = null)
        {
            state.AppendLine($"public {state.TypeInfo.Name}()");
            state.AppendLine("{");
            var innerState = state.IncrementIdentation();
            innerState.AppendLine($"Properties = new bool[{state.TypeInfo.Properties.Count}];");
            addBody?.Invoke(innerState);
            state.AppendLine("}");
        }

        private void BuildPropery(BuilderState state, PropertyInformation propertyInfo, int propertyId)
        {
            state.ToProcessTypeSymbols.Add(propertyInfo.Property.Type);
            string fieldName = Casing.PrefixUnderscoreCamelCase(propertyInfo.Property.Name);
            string propertyTypeName = GetPropertyTypeName(propertyInfo);
            state.AppendLine($"private {propertyTypeName} {fieldName};");
            BuildAttributes(state, propertyInfo.Property.GetAttributes());
            state.AppendLine($"public {propertyTypeName} {propertyInfo.Property.Name}");
            state.AppendLine("{");
            var getterSetter = state.IncrementIdentation();
            getterSetter.AppendLine($"get {{ return {fieldName}; }}");
            getterSetter.AppendLine("init");
            getterSetter.AppendLine("{");
            var setterBody = getterSetter.IncrementIdentation();
            setterBody.AppendLine($"Properties[{propertyId}] = true;");
            setterBody.AppendLine($"{fieldName} = value;");
            getterSetter.AppendLine("}");
            state.AppendLine("}");
        }

        private string GetPropertyTypeName(PropertyInformation propertyInfo)
        {
            var propertyTypeSymbol = propertyInfo.Property.Type;
            if (propertyTypeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
                return CreateGenericTypeWithParameters(propertyInfo);

            return ConvertToNullableIfRequired(propertyInfo, propertyTypeSymbol);
        }

        private string CreateGenericTypeWithParameters(PropertyInformation propertyInfo)
        {
            if (propertyInfo?.Property?.Type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
                throw new InvalidOperationException("Parameter is not generic type parameter.");

            var firstUnderlyingType = GetPropertyTypeName(namedType.TypeArguments.First()).TypeName;
            var withoutUnderlyingType = namedType.ToDisplayString(new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.None, SymbolDisplayMemberOptions.None, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeType, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.IncludeType, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.ExpandNullable));

            var genericResult = $"{withoutUnderlyingType}<{firstUnderlyingType}";
            foreach (var underlyingType in namedType.TypeArguments.Skip(1).OfType<INamedTypeSymbol>())
            {
                string genericTypeParam = ConvertToNullableIfRequired(propertyInfo, underlyingType);
                genericResult += $", {genericTypeParam}";
            }
            genericResult += ">";
            if (namedType.Name.Contains("Dictionary") && namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic")
                propertyInfo.IsGenericDictionary = true;

            return genericResult;
        }

        private string ConvertToNullableIfRequired(PropertyInformation propertyInfo, ITypeSymbol typeSymbol)
        {
            string genericTypeParam = GetPropertyTypeName(typeSymbol).TypeName;
            if (typeSymbol.IsValueType && typeSymbol.SpecialType != SpecialType.System_Nullable_T && typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                propertyInfo.IsConvertedToNullableType = true;
                genericTypeParam = $"System.Nullable<{genericTypeParam}>";
            }

            return genericTypeParam;
        }

        private (bool IsGeneratedType, string TypeName) GetPropertyTypeName(ITypeSymbol propertyTypeSymbol)
        {
            if (GeneratedTypeFilter.IsGeneratableType(propertyTypeSymbol))
            {
                return (true, GetFullName(propertyTypeSymbol));
            }
            return (false, propertyTypeSymbol.ToDisplayString(GeneratedTypeFilter.SymbolFormat));
        }

        private void BuildAttributes(BuilderState state, IEnumerable<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
                BuildAttribute(state, attribute);
        }

        private void BuildAttribute(BuilderState state, AttributeData attribute) => state.AppendLine($"[{attribute}]");

        private void BuildAllProperties(BuilderState state)
        {
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                BuildPropery(state, state.TypeInfo.Properties[i], i);
                state.AppendLine();
            }
        }

        private void BuildClassBody(BuilderState state)
        {
            BuildConstructor(state);
            state.AppendLine();
            BuildAllProperties(state);
            BuildApplyPath(state);
        }

        private void BuildApplyPath(BuilderState state)
        {
            state.AppendLine($"public override {state.TypeInfo.SourceTypeName} ApplyPatch({state.TypeInfo.SourceTypeName} input)");
            state.AppendLine("{");
            var bodyState = state.IncrementIdentation();
            CallConstructIfEmpty(bodyState, "input ??=", leaveOpen: false);
            SetInitOnlyProperties(bodyState);
            SetReadWriteProperties(bodyState);
            PopulateDictionaryProperties(bodyState);
            bodyState.AppendLine("return input;");
            state.AppendLine("}");
        }

        private void SetReadWriteProperties(BuilderState state)
        {
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                var currentProperty = state.TypeInfo.Properties[i].Property;
                if (!IsInitOnlyProperty(currentProperty))
                {
                    state.AppendLine($"if (Properties[{i}])");
                    if (GeneratedTypeFilter.IsGeneratableType(currentProperty.Type))
                        state.IncrementIdentation().AppendLine($"input.{currentProperty.Name} = {currentProperty.Name}?.ApplyPatch(input.{currentProperty.Name});");
                    else if (state.TypeInfo.Properties[i].IsGenericDictionary)
                        state.IncrementIdentation().AppendLine($"input.{currentProperty.Name} ??= new();");
                    else if (state.TypeInfo.Properties[i].IsConvertedToNullableType)
                        state.IncrementIdentation().AppendLine($"input.{currentProperty.Name} = {currentProperty.Name}.HasValue ? {currentProperty.Name}.Value : default;");
                    else
                        state.IncrementIdentation().AppendLine($"input.{currentProperty.Name} = {currentProperty.Name};");
                }
            }
        }

        private void SetInitOnlyProperties(BuilderState state)
        {
            if (!state.TypeInfo.Properties.Any(x => IsInitOnlyProperty(x.Property)))
                return;
            //state.AppendLine($"var tmp = new {state.TypeInfo.SourceTypeName}()");
            CallConstructIfEmpty(state, "var tmp =", leaveOpen: true);
            state.AppendLine("{");
            var initializerState = state.IncrementIdentation();
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                var currentProperty = state.TypeInfo.Properties[i].Property;
                if (IsInitOnlyProperty(currentProperty))
                {
                    if (GeneratedTypeFilter.IsGeneratableType(currentProperty.Type))
                        initializerState.AppendLine($"{currentProperty.Name} = Properties[{i}] ? this.{currentProperty.Name}?.ApplyPatch(input.{currentProperty.Name}) : input.{currentProperty.Name},");
                    else if (state.TypeInfo.Properties[i].IsGenericDictionary)
                        initializerState.AppendLine($"{currentProperty.Name} = Properties[{i}] && input.{currentProperty.Name} == null ? new() : input.Values,");
                    else if (state.TypeInfo.Properties[i].IsConvertedToNullableType)
                        initializerState.AppendLine($"{currentProperty.Name} = Properties[{i}] ? ({currentProperty.Name}.HasValue ? this.{currentProperty.Name}.Value : default) : input.{currentProperty.Name},");
                    else
                        initializerState.AppendLine($"{currentProperty.Name} = Properties[{i}] ? this.{currentProperty.Name} : input.{currentProperty.Name},");
                }
                else
                {
                    // Copy old property values onto the new object
                    initializerState.AppendLine($"{currentProperty.Name} = input.{currentProperty.Name},");
                }
            }
            state.AppendLine("};");
            state.AppendLine("input = tmp;");
        }

        private void PopulateDictionaryProperties(BuilderState state)
        {
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                var currentProperty = state.TypeInfo.Properties[i].Property;
                if (!GeneratedTypeFilter.IsGeneratableType(currentProperty.Type) && state.TypeInfo.Properties[i].IsGenericDictionary)
                    PopulateDictionary(state, state.TypeInfo.Properties[i]);
            }
        }

        private bool IsInitOnlyProperty(IPropertySymbol propertySymbol)
        {
            return propertySymbol.SetMethod?.OriginalDefinition.IsInitOnly ?? false;
        }

        private bool HasDefaultConstructor(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Constructor).AnyOrNull(x => x.Parameters.IsEmpty);
        }

        private void CallConstructIfEmpty(BuilderState state, string toInitialize, bool leaveOpen)
        {
            IEnumerable<string> parameters = Enumerable.Empty<string>();
            if (!HasDefaultConstructor(state.TypeInfo.TypeSymbol))
            {
                var typeSymbol = state.TypeInfo.TypeSymbol;
                var ctor = typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Constructor).OrderByDescending(x => x.Parameters.Length).First();
                var properties = state.TypeInfo.Properties.Select(x => x.Property).ToList();
                parameters = Enumerable.Repeat("default", ctor.Parameters.Length);
            }
            var ending = leaveOpen ? "" : ";";
            state.AppendLine($"{toInitialize} new {state.TypeInfo.SourceTypeName}({string.Join(", ", parameters)}){ending}");
            return;
        }

        private void PopulateDictionary(BuilderState state, PropertyInformation propertyInformation)
        {
            var propertyName = propertyInformation.Property.Name;
            state.AppendLine($"foreach(var item in {propertyName})");
            state.AppendLine("{");
            var foreachBody = state.IncrementIdentation();
            foreachBody.AppendLine("if(item.Value is null)");
            foreachBody.IncrementIdentation().AppendLine($"input.{propertyName}.Remove(item.Key);");
            foreachBody.AppendLine("else");
            if (propertyInformation.IsConvertedToNullableType)
                foreachBody.IncrementIdentation().AppendLine($"input.{propertyName}[item.Key] = item.Value.Value;");
            else
                foreachBody.IncrementIdentation().AppendLine($"input.{propertyName}[item.Key] = item.Value;");
            state.AppendLine("}");
        }
    }
}
