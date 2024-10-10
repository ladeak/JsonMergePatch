using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public interface IPatchParametersWalker
    {
        /// <summary>
        /// Finds all generic type parameters used with Patch type used as a parameter
        /// in the children a syntax node.
        /// </summary>
        IEnumerable<ITypeSymbol> Process(SyntaxNode node, SemanticModel semanticModel);
    }
}