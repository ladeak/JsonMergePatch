using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions
{
    public interface ITypeBuilder
    {
        GeneratedWrapper BuildWrapperType(ITypeSymbol typeInfo, string sourceTypeName);
    }
}