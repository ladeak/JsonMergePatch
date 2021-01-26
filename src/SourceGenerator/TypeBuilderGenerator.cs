using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    public class TypeBuilderGenerator
    {
        private readonly IEnumerable<SyntaxTree> _syntaxTrees;
        private readonly Compilation _compilation;

        public TypeBuilderGenerator(IEnumerable<SyntaxTree> syntaxTrees, Compilation compilation)
        {
            _syntaxTrees = syntaxTrees ?? throw new ArgumentNullException(nameof(syntaxTrees));
            _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        }

        public IEnumerable<GeneratedWrapper> Generate()
        {
            var result = new List<GeneratedWrapper>();
            foreach (SyntaxTree tree in _syntaxTrees)
            {
                var walker = new PatchParametersWalker(_compilation.GetSemanticModel(tree));
                var typesToWrap = walker.Process(tree.GetRoot());
                foreach (var typeInfo in typesToWrap)
                {
                    var sourceTypeName = $"{typeInfo.ContainingNamespace}.{typeInfo.Name}";
                    if (!result.Any(x => x.SourceTypeFullName == sourceTypeName))
                    {
                        var name = GetName(typeInfo);
                        var typeSource = CreateWrapperType(typeInfo, name, sourceTypeName);
                        result.Add(new GeneratedWrapper() { FileName = $"LaDeakJsonMergePatch{name}", SourceCode = typeSource, SourceTypeFullName = sourceTypeName, TargetTypeFullName = $"LaDeak.JsonMergePatch.Shared.{name}" });
                    }
                }
            }
            return result;
        }

        private string GetName(ITypeSymbol typeInfo) => $"{typeInfo.Name}Wrapped";

        public string CreateWrapperType(ITypeSymbol typeInfo, string name, string sourceTypeName)
        {
            if (!Debugger.IsAttached) Debugger.Launch();
            int propertyCount = typeInfo.GetMembers().Count();

            StringBuilder sb = new StringBuilder(@"
using System;

namespace LaDeak.JsonMergePatch.Generated
{
");
            sb.AppendLine($"    public class {name} : LaDeak.JsonMergePatch.Shared.Patch<{sourceTypeName}>");
            sb.AppendLine("    {");
            AddConstructor(sb, name, 3);
            sb.Append(@"
    {
        private DateTime _date;
        private int _temperatureC;
        private string _summary;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                Properties[0] = true;
                _date = value;
            }
        }

        public int TemperatureC
        {
            get { return _temperatureC; }
            set
            {
                Properties[1] = true;
                _temperatureC = value;
            }
        }

        public string Summary
        {
            get { return _summary; }
            set
            {
                Properties[2] = true;
                _summary = value;
            }
        }

        public override CoreWebApi.Controllers.WeatherForecast ApplyPatch(CoreWebApi.Controllers.WeatherForecast input)
        {
            if (Properties[0])
                input.Date = Date;
            if (Properties[1])
                input.TemperatureC = TemperatureC;
            if (Properties[2])
                input.Summary = Summary;
            return input;
        }
    }
}");
            return sb.ToString();
        }

        private StringBuilder AddConstructor(StringBuilder sb, string name, int propertyCount)
        {
            sb.AppendLine($"        public {name}()");
            sb.AppendLine("        {");
            sb.AppendLine($"        Properties = new bool[{propertyCount}];");
            sb.AppendLine("        }");
            return sb;
        }
    }
}
