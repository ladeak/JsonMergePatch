using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public static class NameBuilder
    {
        internal const string Namespace = "LaDeak.JsonMergePatch.Generated";

        public static string GetName(ITypeSymbol typeInfo) => $"{typeInfo.Name}Wrapped";

        public static string GetNamespaceExtension(ITypeSymbol typeInfo) => $"Safe{typeInfo.ContainingNamespace.ToDisplayString()}";

        public static string GetFullTypeName(ITypeSymbol typeInfo) => $"{Namespace}.{GetNamespaceExtension(typeInfo)}.{GetName(typeInfo)}";

        public static string GetNamespace(ITypeSymbol typeInfo) => $"{Namespace}.{GetNamespaceExtension(typeInfo)}";

        public static string GetNamespaceExtension(string extension) => $"{Namespace}.Safe{extension}";
    }
}
