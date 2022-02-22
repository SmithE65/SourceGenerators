using Microsoft.CodeAnalysis;
using System;

namespace SG5.SourceGenerators
{
    [Generator]
    public class ConfigGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            bool isEnabled = false;
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("builder_property.MyGenerator_IsEnabled", out var amIActive))
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