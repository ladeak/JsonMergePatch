using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class TypeRepositoryGenerator
    {
        public string CreateTypeRepository(IEnumerable<(string, string)> typeRegistrations)
        {
            StringBuilder sb = new StringBuilder(@"
namespace LaDeak.JsonMergePatch.Generated
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

        public void Add<TSource, TWrapper>() where TWrapper : LaDeak.JsonMergePatch.Abstractions.Patch<TSource>
        {
            _repository.Add(typeof(TSource), typeof(TWrapper));
        }

        public bool TryGet(System.Type source, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out System.Type wrapper)
        {
            return _repository.TryGetValue(source, out wrapper);
        }

        public static LaDeak.JsonMergePatch.Abstractions.ITypeRepository Instance { get; } = new TypeRepository();
    }
}");
            return sb.ToString();
        }
    }
}






