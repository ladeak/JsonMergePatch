namespace LaDeak.JsonMergePatch.SourceGenerator;

public interface ITypeBuilderGenerator
{
    IEnumerable<GeneratedWrapper> Generate();
}