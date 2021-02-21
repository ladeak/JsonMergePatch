using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendLineWithIdentation(this StringBuilder sb, int identation, string line)
        {
            sb.Append(' ', identation);
            sb.AppendLine(line);
            return sb;
        }
    }
}
