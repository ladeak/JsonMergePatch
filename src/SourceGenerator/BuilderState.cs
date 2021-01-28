using System;
using System.Text;

namespace SourceGenerator
{
    public class BuilderState
    {
        public StringBuilder Builder { get; }
        public int Identation { get; }
        public TypeInformation TypeInfo { get; }

        public BuilderState(TypeInformation typeInfo) : this(new StringBuilder(), 0, typeInfo)
        {
        }

        private BuilderState(StringBuilder builder, int identation, TypeInformation typeInfo)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            Identation = identation;
            TypeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
        }

        public BuilderState IncrementIdentation() => new BuilderState(Builder, Identation + 4, TypeInfo);

        public void AppendLine(string line) => Builder.AppendLineWithIdentation(Identation, line);

        public void AppendLine() => Builder.AppendLine();
    }
}
