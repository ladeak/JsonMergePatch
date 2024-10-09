using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator;

public class BuilderState
{
    public StringBuilder Builder { get; }
    public int Identation { get; }
    public GeneratorClassInfo TypeInfo { get; }

    public BuilderState(GeneratorClassInfo typeInfo) : this(new StringBuilder(), 0, typeInfo)
    {
    }

    private BuilderState(StringBuilder builder, int identation, GeneratorClassInfo typeInfo)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Identation = identation;
        TypeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
    }

    public BuilderState IncrementIdentation() => new BuilderState(Builder, Identation + 4, TypeInfo);

    public void AppendLine(string line) => Builder.AppendLineWithIdentation(Identation, line);

    public void AppendLine() => Builder.AppendLine();
}
