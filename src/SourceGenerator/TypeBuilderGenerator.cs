using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace JsonMergePatch.SourceGenerator
{
    public class TypeBuilderGenerator
    {
        private readonly IEnumerable<SyntaxTree> _syntaxTrees;
        private readonly Compilation _compilation;
        private readonly Stack<ITypeSymbol> _typesToWrap;

        public TypeBuilderGenerator(IEnumerable<SyntaxTree> syntaxTrees, Compilation compilation)
        {
            _typesToWrap = new Stack<ITypeSymbol>();
            _syntaxTrees = syntaxTrees ?? throw new ArgumentNullException(nameof(syntaxTrees));
            _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        }

        public IEnumerable<GeneratedWrapper> Generate()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            var result = new List<GeneratedWrapper>();
            foreach (SyntaxTree tree in _syntaxTrees)
                WalkTree(result, tree);

            
            return result;
        }

        private void WalkTree(List<GeneratedWrapper> result, SyntaxTree tree)
        {
            var walker = new PatchParametersWalker(_compilation.GetSemanticModel(tree));
            var typesToWrap = walker.Process(tree.GetRoot());
            PushToWrap(typesToWrap);
            while (_typesToWrap.Count > 0)
            {
                var typeInfo = _typesToWrap.Pop();
                var sourceTypeName = $"{typeInfo.ContainingNamespace}.{typeInfo.Name}";
                if (!result.Any(x => x.SourceTypeFullName == sourceTypeName) 
                    && typeInfo.SpecialType == SpecialType.None
                    && !typeInfo.IsAnonymousType && !typeInfo.IsAbstract)
                {
                    var typeBuilder = new TypeBuilder();
                    var generatedTypeResult = typeBuilder.BuildWrapperType(typeInfo, sourceTypeName);
                    PushToWrap(generatedTypeResult.ToProcessTypes);
                    result.Add(generatedTypeResult);
                }
            }
        }

        private void PushToWrap(List<ITypeSymbol> typesToWrap)
        {
            foreach (var typeToWrap in typesToWrap)
                _typesToWrap.Push(typeToWrap);
        }
    }
}
