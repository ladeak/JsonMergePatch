using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace JsonMergePatch.SourceGenerator
{
    public class BuilderState
    {
        public StringBuilder Builder { get; }
        public int Identation { get; }
        public TypeInformation TypeInfo { get; }
        public List<ITypeSymbol> ToProcessTypeSymbols { get; }

        public BuilderState(TypeInformation typeInfo) : this(new StringBuilder(), 0, typeInfo, new List<ITypeSymbol>())
        {
        }

        private BuilderState(StringBuilder builder, int identation, TypeInformation typeInfo, List<ITypeSymbol> toProcessTypeSymbols)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            Identation = identation;
            TypeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
            ToProcessTypeSymbols = toProcessTypeSymbols;
        }

        public BuilderState IncrementIdentation() => new BuilderState(Builder, Identation + 4, TypeInfo, ToProcessTypeSymbols);

        public void AppendLine(string line) => Builder.AppendLineWithIdentation(Identation, line);

        public void AppendLine() => Builder.AppendLine();
    }
}
