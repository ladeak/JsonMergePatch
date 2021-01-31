namespace JsonMergePatch.SourceGenerator
{
    public class Casing
    {
        public static string ToCamelCase(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
