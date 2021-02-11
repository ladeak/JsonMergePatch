using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public interface ITypeBuilder
    {
        GeneratedWrapper BuildWrapperType(ITypeSymbol typeInfo, string sourceTypeName);
    }
}