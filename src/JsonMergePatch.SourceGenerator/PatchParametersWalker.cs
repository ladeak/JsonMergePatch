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
        private const string PatchableAttributeFullName = "LaDeak.JsonMergePatch.Abstractions.PatchableAttribute";
        private SemanticModel? _semantics;
        private List<ITypeSymbol>? _typeNames;

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
                if (typeInfo?.Name == "Patch" && typeInfo.OriginalDefinition.ContainingNamespace.ToDisplayString() == "LaDeak.JsonMergePatch.Abstractions" && typeInfo.TypeArguments.Length == 1)
                {
                    _typeNames?.Add(typeInfo.TypeArguments.First());
                }
            }

            base.VisitParameter(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.ValueText == "ReadJsonPatchAsync")
            {
                if (_semantics?.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.OriginalDefinition.ContainingType.Name == "HttpContentExtensions"
                    && methodSymbol.OriginalDefinition.ContainingType.ContainingNamespace.ToDisplayString() == "LaDeak.JsonMergePatch.Http"
                    && !methodSymbol.TypeArguments.IsEmpty)
                    _typeNames?.Add(methodSymbol.TypeArguments.First());
            }
            base.VisitInvocationExpression(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (_semantics.GetDeclaredSymbol(node) is ITypeSymbol typeSymbol
                && typeSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == PatchableAttributeFullName))
            {
                _typeNames?.Add(typeSymbol);
            }
            base.VisitStructDeclaration(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            if (_semantics.GetDeclaredSymbol(node) is ITypeSymbol typeSymbol
                && typeSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == PatchableAttributeFullName))
            {
                _typeNames?.Add(typeSymbol);
            }
            base.VisitRecordDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (_semantics.GetDeclaredSymbol(node) is ITypeSymbol typeSymbol
                && typeSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == PatchableAttributeFullName))
            {
                _typeNames?.Add(typeSymbol);
            }
            base.VisitClassDeclaration(node);
        }
    }
}
