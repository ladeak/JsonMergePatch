using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public static class GeneratedTypeFilter
    {
        public static bool IsGeneratableType(ITypeSymbol typeInfo)
        {
            return typeInfo.SpecialType == SpecialType.None && !typeInfo.IsAnonymousType && !typeInfo.IsAbstract;
            // TODO check for default constructor
        }

        public static string SourceTypeName(ITypeSymbol typeInfo)
        {
            return $"{typeInfo.ContainingNamespace.ToDisplayString()}.{typeInfo.Name}";
        }
    }
}
