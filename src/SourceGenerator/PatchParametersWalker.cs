using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonMergePatch.SourceGenerator
{
    public class PatchParametersWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semantics;
        private readonly List<ITypeSymbol> _typeNames;

        public PatchParametersWalker(SemanticModel semantics)
        {
            _semantics = semantics;
            _typeNames = new List<ITypeSymbol>();
        }

        public List<ITypeSymbol> Process(SyntaxNode node)
        {
            base.Visit(node);
            return _typeNames;
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            if (node?.Type != null)
            {
                var typeInfo = _semantics.GetTypeInfo(node.Type).Type as INamedTypeSymbol;
                if (typeInfo?.Name == "Patch" && typeInfo.ContainingAssembly.Name == "JsonMergePatch" && typeInfo.TypeArguments.Count() == 1)
                {
                    _typeNames.Add(typeInfo.TypeArguments.First());
                }
            }

            base.VisitParameter(node);
        }

    }
}
