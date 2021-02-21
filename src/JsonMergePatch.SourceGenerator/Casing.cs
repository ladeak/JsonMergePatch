using System;
using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class Casing
    {
        [ThreadStatic]
        public static StringBuilder? _builder;

        public static string PrefixUnderscoreCamelCase(string name)
        {
            _builder ??= new StringBuilder();
            _builder.Clear();
            _builder.Append('_');
            _builder.Append(char.ToLowerInvariant(name[0]));
            for (int i = 1; i < name.Length; i++)
                _builder.Append(name[i]);
            return _builder.ToString();
        }
    }
}
