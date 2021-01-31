namespace LaDeak.JsonMergePatch
{
    public abstract class Patch<T>
    {
        /// <summary>
        /// List of properites used by the request.
        /// </summary>
        public bool[] Properties;

        /// <summary>
        /// Applies updates on input type using Json Merge Patch rules.
        /// </summary>
        /// <param name="input">The entity to be patched.</param>
        /// <returns>The patched entity.</returns>
        public abstract T ApplyPatch(T input);
    }
}
