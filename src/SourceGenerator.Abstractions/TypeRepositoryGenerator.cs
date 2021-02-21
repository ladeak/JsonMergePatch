using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions
{
    public class TypeRepositoryGenerator
    {
        public string CreateTypeRepository(IEnumerable<(string, string)> typeRegistrations)
        {
            StringBuilder sb = new StringBuilder(@"
namespace LaDeak.JsonMergePatch.Generated
{
    public class TypeRepositoryContainer
    {
        public LaDeak.JsonMergePatch.Abstractions.ITypeRepository Repository { get; } = new LaDeak.JsonMergePatch.Abstractions.TypeRepository();

        private TypeRepositoryContainer()
        {
");
            foreach ((var originalType, var generatedType) in typeRegistrations ?? Enumerable.Empty<(string, string)>())
                sb.AppendLine($"            Repository.Add<{originalType}, {generatedType}>();");

            sb.Append(@"
        }

        public static TypeRepositoryContainer Instance { get; } = new TypeRepositoryContainer();
    }
}");
            return sb.ToString();
        }


    }
}
