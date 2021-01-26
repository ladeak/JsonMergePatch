using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class HelloWorldGenerator : ISourceGenerator
    {
        //private PatchActionReceiver _sourceData

        public void Execute(GeneratorExecutionContext context)
        {
            // begin creating the source we'll inject into the users compilation
            StringBuilder sourceBuilder = new StringBuilder(@"
using System;
namespace HelloWorldGenerated
{
    public static class HelloWorld
    {
        public static void SayHello() 
        {
            Console.WriteLine(""Hello from generated code!"");
            Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
");

            // using the context, get a list of syntax trees in the users compilation
            IEnumerable<SyntaxTree> syntaxTrees = context.Compilation.SyntaxTrees;


            foreach (SyntaxTree tree in syntaxTrees)
            {
                var walker = new PatchParametersWalker(context.Compilation.GetSemanticModel(tree));
                var result = walker.Process(tree.GetRoot());
                sourceBuilder.AppendLine($@"Console.WriteLine(@"" - {string.Join(",", result)}"");");
            }

            // finish creating the source to inject
            sourceBuilder.Append(@"
        }
    }
}");

            // inject the created source into the users compilation
            context.AddSource("helloWorldGenerated", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
        }
    }
}
