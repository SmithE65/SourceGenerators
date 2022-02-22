using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SG5.SourceGenerators
{
    [Generator]
    public class FromFileGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            // find anything that matches our files
            var myFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith(".txt"));

            var methods = new StringBuilder();
            foreach (var file in myFiles)
            {
                var name = Path.GetFileNameWithoutExtension(file.Path);
                var content = file.GetText(context.CancellationToken);
                methods.AppendLine($"\tpublic const string {name} = \"{content}\";");
            }

            var myStrings = $@"
namespace Generated;
public static class MyStrings
{{
{methods}
}}
";
            context.AddSource($"MyStrings.generated.cs", myStrings);
        }
    }
}
