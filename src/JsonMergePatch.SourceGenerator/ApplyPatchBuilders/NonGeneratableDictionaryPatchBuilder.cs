using System;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders
{
    public class NonGeneratableDictionaryPatchBuilder : ApplyPatchBuilder
    {
        private readonly INamedTypeSymbol _namedType;
        private readonly IPropertySymbol _property;
        private readonly bool _isConvertedToNullableType;

        public NonGeneratableDictionaryPatchBuilder(INamedTypeSymbol namedType, IPropertySymbol property, bool isConvertedToNullableType)
        {
            if (!namedType.Name.Contains("Dictionary") || namedType.ContainingNamespace.ToDisplayString() != "System.Collections.Generic")
                throw new ArgumentException($"Input argument type is not a valid for {nameof(NonGeneratableDictionaryPatchBuilder)}");
            _namedType = namedType;
            _property = property;
            _isConvertedToNullableType = isConvertedToNullableType;
        }

        public override BuilderState BuildInitOnly(BuilderState state, int i)
        {
            state.AppendLine($"{_property.Name} = Properties[{i}] && input.{_property.Name} == null ? new() : input.Values,");
            return state;
        }

        public override BuilderState BuildInstantiation(BuilderState state, int i)
        {
            state.AppendLine($"if (Properties[{i}])");
            state.IncrementIdentation().AppendLine($"input.{_property.Name} ??= new();");
            return state;
        }

        public override BuilderState BuildPatch(BuilderState state)
        {
            if (!GeneratedTypeFilter.IsGeneratableType(_property.Type))
                PopulateDictionary(state, _property);
            return state;
        }

        private void PopulateDictionary(BuilderState state, IPropertySymbol property)
        {
            var propertyName = property.Name;
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
}
