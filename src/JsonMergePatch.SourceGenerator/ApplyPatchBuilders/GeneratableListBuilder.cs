using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

/// <summary>
/// Builds ApplyPatch method for <see cref="System.Collections.Generic.List{T}"/> with user types where a Wrapper DTO is generated for the generic type argument.
/// </summary>
public class GeneratableListBuilder : ApplyPatchBuilder
{
    private readonly ITypeSymbol _firstGenericType;
    private readonly string _propertyName;

    public GeneratableListBuilder(INamedTypeSymbol namedType, string propertyName)
    {
        if (!namedType.Name.Contains("List")
           || namedType.ContainingNamespace.ToDisplayString() != "System.Collections.Generic"
           || !GeneratedTypeFilter.IsGeneratableType(namedType.TypeArguments.First()))
        {
            throw new ArgumentException("Input argument type is not a generic list with generatable type.");
        }
        _firstGenericType = namedType.TypeArguments.First();
        _propertyName = propertyName;
    }

    public override BuilderState BuildInitOnly(BuilderState state, int i)
    {
        state.AppendLine($"{_propertyName} = Properties[{i}] && input.{_propertyName} == null ? new() : input.{_propertyName},");
        return state;
    }

    public override BuilderState BuildInstantiation(BuilderState state, int i)
    {
        state.AppendLine($"if (Properties[{i}])");
        state.IncrementIdentation().AppendLine($"input.{_propertyName} = new();");
        return state;
    }

    public override BuilderState BuildPatch(BuilderState state)
    {
        if (_firstGenericType != null && GeneratedTypeFilter.IsGeneratableType(_firstGenericType))
            PopulateGeneratableListProperties(state, _propertyName);
        return state;
    }

    private void PopulateGeneratableListProperties(BuilderState state, string propertyName)
    {
        state.AppendLine($"if({propertyName} != null)");
        state.AppendLine("{");
        var ifBody = state.IncrementIdentation();
        ifBody.AppendLine($"foreach(var item in {propertyName})");
        ifBody.AppendLine("{");
        ifBody.IncrementIdentation().AppendLine($"input.{propertyName}.Add(item?.ApplyPatch(null));");
        ifBody.AppendLine("}");
        state.AppendLine("}");
    }
}

