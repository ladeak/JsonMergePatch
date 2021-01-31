﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace JsonMergePatch.SourceGenerator
{
    public class TypeInformation
    {
        public string Name { get; set; }
        public string SourceTypeName { get; set; }
        public List<IPropertySymbol> Properties { get; set; }
        public ITypeSymbol TypeSymbol { get; set; }
    }
}
