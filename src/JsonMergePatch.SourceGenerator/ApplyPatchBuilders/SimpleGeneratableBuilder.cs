namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

/// <summary>
/// Builds ApplyPatch method for user types where a Wrapper DTO is generated.
/// </summary>
public class SimpleGeneratableBuilder(string name) : ApplyPatchBuilder
{
    public override BuilderState BuildInitOnly(BuilderState state, int i)
    {
        state.AppendLine($"{name} = Properties[{i}] ? this.{name}?.ApplyPatch(input.{name}) : input.{name},");
        return state;
    }

    public override BuilderState BuildInstantiation(BuilderState state, int i)
    {
        state.AppendLine($"if (Properties[{i}])");
        state.IncrementIdentation().AppendLine($"input.{name} = {name}?.ApplyPatch(input.{name});");
        return state;
    }

    public override BuilderState BuildPatch(BuilderState state)
    {
        return state;
    }
}

