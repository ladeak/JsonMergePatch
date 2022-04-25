namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

public abstract class ApplyPatchBuilder
{
    /// <summary>
    /// Generates logic to instantiate object for init property.
    /// </summary>
    /// <param name="state">The state representing the ApplyPatch method.</param>
    /// <param name="i">The index of the property in the 'Properties' collection.</param>
    /// <returns>Returns the new state.</returns>
    public abstract BuilderState BuildInitOnly(BuilderState state, int i);

    /// <summary>
    /// Generates logic to instantiate object for non-init property.
    /// </summary>
    /// <param name="state">The state representing the ApplyPatch method.</param>
    /// <param name="i">The index of the property in the 'Properties' collection.</param>
    /// <returns>Returns the new state.</returns>
    public abstract BuilderState BuildInstantiation(BuilderState state, int i);

    /// <summary>
    /// Generates merging logic for complex state (Dictionary, List).
    /// </summary>
    /// <param name="state">The state representing the ApplyPatch method.</param>
    /// <returns>Returns the new state.</returns>
    public abstract BuilderState BuildPatch(BuilderState state);
}
