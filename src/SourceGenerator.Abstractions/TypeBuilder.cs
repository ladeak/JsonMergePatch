using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions
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
            if (propertyInfo.Property.Type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
                throw new ArgumentException("Parameter is not generic type parameter.", nameof(propertyInfo));

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
            bodyState.AppendLine($"input ??= new {state.TypeInfo.SourceTypeName}();");
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                bodyState.AppendLine($"if (Properties[{i}])");
                var currentProperty = state.TypeInfo.Properties[i].Property;
                if (GeneratedTypeFilter.IsGeneratableType(currentProperty.Type))
                    bodyState.IncrementIdentation().AppendLine($"input.{currentProperty.Name} = {currentProperty.Name}?.ApplyPatch(input.{currentProperty.Name});");
                else if (state.TypeInfo.Properties[i].IsGenericDictionary)
                    BuildDictionaryApplyPath(bodyState.IncrementIdentation(), state.TypeInfo.Properties[i]);
                else if (state.TypeInfo.Properties[i].IsConvertedToNullableType)
                    bodyState.IncrementIdentation().AppendLine($"input.{currentProperty.Name} = {currentProperty.Name}.HasValue ? {currentProperty.Name}.Value : default;");
                else
                    bodyState.IncrementIdentation().AppendLine($"input.{currentProperty.Name} = {currentProperty.Name};");
            }
            bodyState.AppendLine("return input;");
            state.AppendLine("}");
        }

        private void BuildDictionaryApplyPath(BuilderState state, PropertyInformation propertyInformation)
        {
            var propertyName = propertyInformation.Property.Name;
            state.AppendLine($"input.{propertyName} ??= new();");
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
