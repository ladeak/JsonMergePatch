using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator;

public class TypeRepositoryGenerator
{
    public string CreateTypeRepository(IEnumerable<(string, string)> typeRegistrations, string assemblyName)
    {
        StringBuilder sb = new StringBuilder(@$"
namespace {NameBuilder.GetNamespace(assemblyName)}");
        sb.Append(@"
{
    public class TypeRepository : LaDeak.JsonMergePatch.Abstractions.ITypeRepository
    {       
        private System.Collections.Generic.Dictionary<System.Type, System.Type> _repository = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        private TypeRepository()
        {
");
        foreach ((var originalType, var generatedType) in typeRegistrations ?? Enumerable.Empty<(string, string)>())
            sb.AppendLine($"            Add<{originalType}, {generatedType}>();");

        sb.Append(@"
        }

        public static LaDeak.JsonMergePatch.Abstractions.ITypeRepository Instance { get; } = new TypeRepository();

        public void Add<TSource, TWrapper>() where TWrapper : LaDeak.JsonMergePatch.Abstractions.Patch<TSource>
        {
            _repository.Add(typeof(TSource), typeof(TWrapper));
        }

        public void Add(System.Type source, System.Type wrapper)
        {
            if (wrapper.IsSubclassOf(typeof(LaDeak.JsonMergePatch.Abstractions.Patch<>).MakeGenericType(source)))
                _repository.Add(source, wrapper);
        }

        public bool TryGet(System.Type source, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out System.Type wrapper)
        {
            return _repository.TryGetValue(source, out wrapper);
        }

        public System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Type, System.Type>> GetAll() => _repository;
    }
}");
        return sb.ToString();
    }
}






