namespace LaDeak.JsonMergePatch.Shared
{
    public abstract class Patch<T>
    {
        public bool[] Properties;

        public abstract T ApplyPatch(T input);
    }
}
