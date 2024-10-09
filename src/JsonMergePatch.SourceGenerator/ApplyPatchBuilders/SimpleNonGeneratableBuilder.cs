namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

/// <summary>
/// Builds ApplyPatch method for types where no wrapper is generated, or the type is nullable.
/// </summary>
public class SimpleNonGeneratableBuilder(string name, bool isConvertedToNullable) : ApplyPatchBuilder
{
    public override BuilderState BuildInitOnly(BuilderState state, int i)
    {
        if (isConvertedToNullable)
            state.AppendLine($"{name} = Properties[{i}] ? ({name}.HasValue ? this.{name}.Value : default) : input.{name},");
        else
            state.AppendLine($"{name} = Properties[{i}] ? this.{name} : input.{name},");
        return state;
    }

    public override BuilderState BuildInstantiation(BuilderState state, int i)
    {
        state.AppendLine($"if (Properties[{i}])");
        if (isConvertedToNullable)
            state.IncrementIdentation().AppendLine($"input.{name} = {name}.HasValue ? {name}.Value : default;");
        else
            state.IncrementIdentation().AppendLine($"input.{name} = {name};");
        return state;
    }

    public override BuilderState BuildPatch(BuilderState state)
    {
        return state;
    }
}

