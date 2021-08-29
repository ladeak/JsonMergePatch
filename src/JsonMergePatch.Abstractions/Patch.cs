using System.Diagnostics.CodeAnalysis;

namespace LaDeak.JsonMergePatch.Abstractions;

public abstract class Patch<T>
{
    /// <summary>
    /// List of properites used by the request.
    /// </summary>
    protected bool[] Properties;

    /// <summary>
    /// Applies updates on input type using Json Merge Patch rules.
    /// </summary>
    /// <param name="input">The entity to be patched.</param>
    /// <returns>The patched entity.</returns>
    public abstract T ApplyPatch([AllowNull] T input);

    /// <summary>
    /// Returns a  patched version of the default value of <see cref="T"/>.
    /// </summary>
    /// <returns>Object typed <see cref="T"/>.</returns>
    public T ApplyOnDefault() => ApplyPatch(default(T));
}
