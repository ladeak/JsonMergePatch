using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator;

public static class IEnumerableExtensions
{
    public static bool AnyOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source is null || !source.GetEnumerator().MoveNext())
            return true;
        return source.Any(predicate);
    }
}
