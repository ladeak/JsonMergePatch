using System;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

/// <summary>
/// Builds ApplyPatch method for user types where a Wrapper DTO is generated.
/// </summary>
public class SimpleGeneratableBuilder : ApplyPatchBuilder
{
    private readonly IPropertySymbol _propertySymbol;

    public SimpleGeneratableBuilder(IPropertySymbol propertySymbol)
    {
        if (!GeneratedTypeFilter.IsGeneratableType(propertySymbol.Type))
            throw new ArgumentException("The given property is not simple generatable");
        _propertySymbol = propertySymbol;
    }

    public override BuilderState BuildInitOnly(BuilderState state, int i)
    {
        state.AppendLine($"{_propertySymbol.Name} = Properties[{i}] ? this.{_propertySymbol.Name}?.ApplyPatch(input.{_propertySymbol.Name}) : input.{_propertySymbol.Name},");
        return state;
    }

    public override BuilderState BuildInstantiation(BuilderState state, int i)
    {
        state.AppendLine($"if (Properties[{i}])");
        state.IncrementIdentation().AppendLine($"input.{_propertySymbol.Name} = {_propertySymbol.Name}?.ApplyPatch(input.{_propertySymbol.Name});");
        return state;
    }

    public override BuilderState BuildPatch(BuilderState state)
    {
        return state;
    }
}

