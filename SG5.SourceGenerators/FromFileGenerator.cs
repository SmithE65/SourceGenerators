using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
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

    [Generator]
    public class ConfigGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            bool isEnabled = false;
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("is_enabled", out var amIActive))
            {
                isEnabled = amIActive.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            var mySwitch = $@"
namespace Generated;
public static class MySwitch
{{
    public const string Test = ""I was {(isEnabled ? "enabled" : "disabled")} at compile time."";
}}
";

            context.AddSource("Enabled.g.cs", mySwitch);
        }
    }
}