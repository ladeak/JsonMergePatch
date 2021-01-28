using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    public class TypeBuilderGenerator
    {
        private readonly IEnumerable<SyntaxTree> _syntaxTrees;
        private readonly Compilation _compilation;

        public TypeBuilderGenerator(IEnumerable<SyntaxTree> syntaxTrees, Compilation compilation)
        {
            _syntaxTrees = syntaxTrees ?? throw new ArgumentNullException(nameof(syntaxTrees));
            _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        }

        public IEnumerable<GeneratedWrapper> Generate()
        {
            var result = new List<GeneratedWrapper>();
            foreach (SyntaxTree tree in _syntaxTrees)
            {
                var walker = new PatchParametersWalker(_compilation.GetSemanticModel(tree));
                var typesToWrap = walker.Process(tree.GetRoot());
                foreach (var typeInfo in typesToWrap)
                {
                    var sourceTypeName = $"{typeInfo.ContainingNamespace}.{typeInfo.Name}";
                    if (!result.Any(x => x.SourceTypeFullName == sourceTypeName))
                    {
                        var name = GetName(typeInfo);
                        var typeSource = CreateWrapperType(typeInfo, name, sourceTypeName);
                        result.Add(new GeneratedWrapper() { FileName = $"LaDeakJsonMergePatch{name}", SourceCode = typeSource, SourceTypeFullName = sourceTypeName, TargetTypeFullName = $"LaDeak.JsonMergePatch.Generated.{name}" });
                    }
                }
            }
            //if (!Debugger.IsAttached) Debugger.Launch();
            return result;
        }

        private string GetName(ITypeSymbol typeInfo) => $"{typeInfo.Name}Wrapped";

        public string CreateWrapperType(ITypeSymbol typeInfo, string name, string sourceTypeName)
        {
            var typeInformation = new TypeInformation() { Name = name, SourceTypeName = sourceTypeName };
            typeInformation.Properties = typeInfo.GetMembers().OfType<IPropertySymbol>()
                .Where(x => !x.IsReadOnly && !x.IsWriteOnly && !x.IsIndexer && !x.IsStatic && !x.IsAbstract)
                .ToList();

            var state = new BuilderState(typeInformation);
            BuildNameSpace(state, BuildClass);

            return state.Builder.ToString();
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
            state.AppendLine($"public class {state.TypeInfo.Name} : LaDeak.JsonMergePatch.Shared.Patch<{state.TypeInfo.SourceTypeName}>");
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
            string fieldName = $"_{ Casing.ToCamelCase(propertySymbol.Name)}";
            var propertyTypeName = $"{propertySymbol.Type.ContainingNamespace}.{propertySymbol.Type.Name}";
            state.AppendLine($"private {propertyTypeName} {fieldName};");
            BuildAttributes(state, propertySymbol.GetAttributes());
            state.AppendLine($"public {propertyTypeName} {propertySymbol.Name}");
            state.AppendLine("{");
            var getterSetter = state.IncrementIdentation();
            getterSetter.AppendLine($"get {{ return {fieldName}; }}");
            getterSetter.AppendLine($"set");
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
            {
                BuildAttribute(state, attribute);
            }
        }

        public void BuildAttribute(BuilderState state, AttributeData attribute)
        {
            state.AppendLine($"[{attribute.ToString()}]");
        }

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

        public void BuildClass(BuilderState state) => BuildClassDeclaration(state, s => BuildClassBody(s));

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
