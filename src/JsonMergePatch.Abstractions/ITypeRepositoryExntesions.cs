namespace LaDeak.JsonMergePatch.Abstractions
{
    public static class ITypeRepositoryExntesions
    {
        public static ITypeRepository Extend(this ITypeRepository repository, ITypeRepository other)
        {
            foreach (var item in other.GetAll())
                if (!repository.TryGet(item.Key, out _))
                    repository.Add(item.Key, item.Value);
            return repository;
        }
    }
}