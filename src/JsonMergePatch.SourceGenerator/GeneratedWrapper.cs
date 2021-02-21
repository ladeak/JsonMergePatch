using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class GeneratedWrapper
    {
        public string? FileName { get; set; }
        public string? SourceCode { get; set; }
        public string? SourceTypeFullName { get; set; }
        public string? TargetTypeFullName { get; set; }
        public List<ITypeSymbol>? ToProcessTypes { get; set; }
    }
}
