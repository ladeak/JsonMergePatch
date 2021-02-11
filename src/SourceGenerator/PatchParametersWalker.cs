using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class PatchParametersWalker : CSharpSyntaxWalker, IPatchParametersWalker
    {
        private SemanticModel _semantics;
        private List<ITypeSymbol> _typeNames;

        public IEnumerable<ITypeSymbol> Process(SyntaxNode node, SemanticModel semantics)
        {
            _semantics = semantics ?? throw new ArgumentNullException(nameof(semantics));
            _typeNames = new List<ITypeSymbol>();
            base.Visit(node);
            return _typeNames;
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            if (node?.Type != null)
            {
                var typeInfo = _semantics.GetTypeInfo(node.Type).Type as INamedTypeSymbol;
                if (typeInfo?.Name == "Patch" && typeInfo.OriginalDefinition.ContainingSymbol.Name == "JsonMergePatch" && typeInfo.TypeArguments.Count() == 1)
                {
                    _typeNames.Add(typeInfo.TypeArguments.First());
                }
            }

            base.VisitParameter(node);
        }

    }
}
