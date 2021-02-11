using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class MultiTypeBuilder
    {
        private readonly IEnumerable<SyntaxTree> _syntaxTrees;
        private readonly Compilation _compilation;
        private readonly ITypeBuilder _typeBuilder;
        private readonly IPatchParametersWalker _walker;
        private readonly Stack<ITypeSymbol> _typesToWrap;

        public MultiTypeBuilder(IEnumerable<SyntaxTree> syntaxTrees, Compilation compilation, ITypeBuilder typeBuilder, IPatchParametersWalker walker)
        {
            _typesToWrap = new Stack<ITypeSymbol>();
            _syntaxTrees = syntaxTrees ?? throw new ArgumentNullException(nameof(syntaxTrees));
            _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            _typeBuilder = typeBuilder ?? throw new ArgumentNullException(nameof(typeBuilder));
            _walker = walker ?? throw new ArgumentNullException(nameof(walker));
        }

        public IEnumerable<GeneratedWrapper> Generate()
        {
            var result = new List<GeneratedWrapper>();
            foreach (SyntaxTree tree in _syntaxTrees)
                WalkTree(result, tree);

            return result;
        }

        private void WalkTree(List<GeneratedWrapper> result, SyntaxTree tree)
        {
            var typesToWrap = _walker.Process(tree.GetRoot(), _compilation.GetSemanticModel(tree));
            PushToWrap(typesToWrap);
            while (_typesToWrap.Count > 0)
            {
                var typeInfo = _typesToWrap.Pop();
                var sourceTypeName = $"{typeInfo.ContainingNamespace}.{typeInfo.Name}";
                if (!result.Any(x => x.SourceTypeFullName == sourceTypeName) 
                    && typeInfo.SpecialType == SpecialType.None
                    && !typeInfo.IsAnonymousType && !typeInfo.IsAbstract)
                {
                    var generatedTypeResult = _typeBuilder.BuildWrapperType(typeInfo, sourceTypeName);
                    PushToWrap(generatedTypeResult.ToProcessTypes);
                    result.Add(generatedTypeResult);
                }
            }
        }

        private void PushToWrap(IEnumerable<ITypeSymbol> typesToWrap)
        {
            foreach (var typeToWrap in typesToWrap)
                _typesToWrap.Push(typeToWrap);
        }
    }
}
