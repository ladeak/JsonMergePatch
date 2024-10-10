using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;

/// <summary>
/// Builds ApplyPatch method for Dictionaries with generic type parameters where no wrapper is generated, or the type is nullable.
/// </summary>
public class NonGeneratableDictionaryPatchBuilder : ApplyPatchBuilder
{
    private readonly INamedTypeSymbol _namedType;
    private readonly string _propertyName;
    private readonly bool _hasGeneratableType;
    private readonly bool _isConvertedToNullableType;

    public NonGeneratableDictionaryPatchBuilder(INamedTypeSymbol namedType, string propertyName, bool hasGeneratableType, bool isConvertedToNullableType)
    {
        if (!namedType.Name.Contains("Dictionary") || namedType.ContainingNamespace.ToDisplayString() != "System.Collections.Generic")
            throw new ArgumentException($"Input argument type is not a valid for {nameof(NonGeneratableDictionaryPatchBuilder)}");
        _namedType = namedType;
        _propertyName = propertyName;
        _hasGeneratableType = hasGeneratableType;
        _isConvertedToNullableType = isConvertedToNullableType;
    }

    public override BuilderState BuildInitOnly(BuilderState state, int i)
    {
        state.AppendLine($"{_propertyName} = Properties[{i}] && input.{_propertyName} == null ? new() : input.Values,");
        return state;
    }

    public override BuilderState BuildInstantiation(BuilderState state, int i)
    {
        state.AppendLine($"if (Properties[{i}])");
        state.IncrementIdentation().AppendLine($"input.{_propertyName} ??= new();");
        return state;
    }

    public override BuilderState BuildPatch(BuilderState state)
    {
        if (!_hasGeneratableType)
            PopulateDictionary(state, _propertyName);
        return state;
    }

    private void PopulateDictionary(BuilderState state, string propertyName)
    {
        state.AppendLine($"if({propertyName} != null)");
        state.AppendLine("{");
        var ifBody = state.IncrementIdentation();
        ifBody.AppendLine($"foreach(var item in {propertyName})");
        ifBody.AppendLine("{");
        var foreachBody = ifBody.IncrementIdentation();
        foreachBody.AppendLine("if(item.Value is null)");
        foreachBody.IncrementIdentation().AppendLine($"input.{propertyName}.Remove(item.Key);");
        foreachBody.AppendLine("else");
        if (_isConvertedToNullableType)
            foreachBody.IncrementIdentation().AppendLine($"input.{propertyName}[item.Key] = item.Value.Value;");
        else
            foreachBody.IncrementIdentation().AppendLine($"input.{propertyName}[item.Key] = item.Value;");
        ifBody.AppendLine("}");
        state.AppendLine("}");
    }
}
