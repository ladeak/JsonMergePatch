using System.Collections.Generic;
using LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator;

public class TypeInformation
{
    public string? Name { get; set; }
    public string? SourceTypeName { get; set; }
    public List<PropertyInformation> Properties { get; } = new List<PropertyInformation>();
    public ITypeSymbol? TypeSymbol { get; set; }
}

public class PropertyInformation
{
    public IPropertySymbol? Property { get; set; }
    public bool IsConvertedToNullableType { get; set; }
    public ApplyPatchBuilder? Builder { get; set; }
}
