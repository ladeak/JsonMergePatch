namespace LaDeak.JsonMergePatch.SourceGenerator;

public interface ITypeBuilder
{
    GeneratedWrapper BuildWrapperType(GeneratorClassInfo typeInfo);
}