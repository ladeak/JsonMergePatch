using System;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

/// <summary>
/// Builds ApplyPatch method for types where no wrapper is generated, or the type is nullable.
/// </summary>
public class SimpleNonGeneratableBuilder : ApplyPatchBuilder
{
    private readonly IPropertySymbol _propertySymbol;
    private readonly bool _isConvertedToNullable;

    public SimpleNonGeneratableBuilder(IPropertySymbol propertySymbol, bool isConvertedToNullable)
    {
        if (GeneratedTypeFilter.IsGeneratableType(propertySymbol.Type))
            throw new ArgumentException("The given property is not simple generatable");
        _propertySymbol = propertySymbol;
        _isConvertedToNullable = isConvertedToNullable;
    }

    public override BuilderState BuildInitOnly(BuilderState state, int i)
    {
        if (_isConvertedToNullable)
            state.AppendLine($"{_propertySymbol.Name} = Properties[{i}] ? ({_propertySymbol.Name}.HasValue ? this.{_propertySymbol.Name}.Value : default) : input.{_propertySymbol.Name},");
        else
            state.AppendLine($"{_propertySymbol.Name} = Properties[{i}] ? this.{_propertySymbol.Name} : input.{_propertySymbol.Name},");
        return state;
    }

    public override BuilderState BuildInstantiation(BuilderState state, int i)
    {
        state.AppendLine($"if (Properties[{i}])");
        if (_isConvertedToNullable)
            state.IncrementIdentation().AppendLine($"input.{_propertySymbol.Name} = {_propertySymbol.Name}.HasValue ? {_propertySymbol.Name}.Value : default;");
        else
            state.IncrementIdentation().AppendLine($"input.{_propertySymbol.Name} = {_propertySymbol.Name};");
        return state;
    }

    public override BuilderState BuildPatch(BuilderState state)
    {
        return state;
    }
}

