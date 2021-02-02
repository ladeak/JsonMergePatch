using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class TypeBuilder
    {
        public GeneratedWrapper BuildWrapperType(ITypeSymbol typeInfo, string sourceTypeName)
        {
            var name = GetName(typeInfo);
            var state = InitializeState(typeInfo, name, sourceTypeName);
            BuildFile(state);
            return new GeneratedWrapper() { 
                FileName = $"LaDeakJsonMergePatch{name}",
                SourceCode = state.Builder.ToString(),
                SourceTypeFullName = sourceTypeName,
                TargetTypeFullName = $"LaDeak.JsonMergePatch.Generated.{name}",
                ToProcessTypes = state.ToProcessTypeSymbols
            };
        }

        public string GetName(ITypeSymbol typeInfo) => $"{typeInfo.Name}Wrapped";

        public void BuildFile(BuilderState state) => BuildNameSpace(state, BuildClass);

        public void BuildClass(BuilderState state) => BuildClassDeclaration(state, s => BuildClassBody(s));

        public BuilderState InitializeState(ITypeSymbol typeInfo, string name, string sourceTypeName)
        {
            var typeInformation = new TypeInformation() { Name = name, SourceTypeName = sourceTypeName, TypeSymbol = typeInfo };
            typeInformation.Properties = typeInfo.GetMembers().OfType<IPropertySymbol>()
                .Where(x => !x.IsReadOnly && !x.IsWriteOnly && !x.IsIndexer && !x.IsStatic && !x.IsAbstract)
                .ToList();

            return new BuilderState(typeInformation);
        }

        public void BuildNameSpace(BuilderState state, Action<BuilderState> addBody = null)
        {
            state.AppendLine("namespace LaDeak.JsonMergePatch.Generated");
            state.AppendLine("{");
            addBody?.Invoke(state.IncrementIdentation());
            state.AppendLine("}");
        }

        public void BuildClassDeclaration(BuilderState state, Action<BuilderState> addBody = null)
        {
            BuildAttributes(state, state.TypeInfo.TypeSymbol.GetAttributes());
            state.AppendLine($"public class {state.TypeInfo.Name} : LaDeak.JsonMergePatch.Patch<{state.TypeInfo.SourceTypeName}>");
            state.AppendLine("{");
            addBody?.Invoke(state.IncrementIdentation());
            state.AppendLine("}");
        }

        public void BuildConstructor(BuilderState state, Action<BuilderState> addBody = null)
        {
            state.AppendLine($"public {state.TypeInfo.Name}()");
            state.AppendLine("{");
            var innerState = state.IncrementIdentation();
            innerState.AppendLine($"Properties = new bool[{state.TypeInfo.Properties.Count}];");
            addBody?.Invoke(innerState);
            state.AppendLine("}");
        }

        public void BuildPropery(BuilderState state, IPropertySymbol propertySymbol, int propertyId)
        {
            state.ToProcessTypeSymbols.Add(propertySymbol.Type);
            string fieldName = $"_{ Casing.ToCamelCase(propertySymbol.Name)}";
            var propertyTypeName = $"{propertySymbol.Type.ContainingNamespace}.{propertySymbol.Type.Name}";
            state.AppendLine($"private {propertyTypeName} {fieldName};");
            BuildAttributes(state, propertySymbol.GetAttributes());
            state.AppendLine($"public {propertyTypeName} {propertySymbol.Name}");
            state.AppendLine("{");
            var getterSetter = state.IncrementIdentation();
            getterSetter.AppendLine($"get {{ return {fieldName}; }}");
            getterSetter.AppendLine($"init");
            getterSetter.AppendLine("{");
            var setterBody = getterSetter.IncrementIdentation();
            setterBody.AppendLine($"Properties[{propertyId}] = true;");
            setterBody.AppendLine($"{fieldName} = value;");
            getterSetter.AppendLine("}");
            state.AppendLine("}");
        }

        public void BuildAttributes(BuilderState state, IEnumerable<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
                BuildAttribute(state, attribute);
        }

        public void BuildAttribute(BuilderState state, AttributeData attribute) => state.AppendLine($"[{attribute}]");

        public void BuildAllProperties(BuilderState state)
        {
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                BuildPropery(state, state.TypeInfo.Properties[i], i);
                state.AppendLine();
            }
        }

        public void BuildClassBody(BuilderState state)
        {
            BuildConstructor(state);
            state.AppendLine();
            BuildAllProperties(state);
            BuildApplyPath(state);
        }

        public void BuildApplyPath(BuilderState state)
        {
            state.AppendLine($"public override {state.TypeInfo.SourceTypeName} ApplyPatch({state.TypeInfo.SourceTypeName} input)");
            state.AppendLine("{");
            var bodyState = state.IncrementIdentation();
            for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
            {
                bodyState.AppendLine($"if (Properties[{i}])");
                bodyState.IncrementIdentation().AppendLine($"input.{state.TypeInfo.Properties[i].Name} = {state.TypeInfo.Properties[i].Name};");
            }
            bodyState.AppendLine("return input;");
            state.AppendLine("}");
        }
    }
}
