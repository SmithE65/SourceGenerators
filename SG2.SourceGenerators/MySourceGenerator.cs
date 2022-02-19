using Microsoft.CodeAnalysis;
using System;

namespace SG2.SourceGenerators
{
    [Generator]
    public class MySourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            string source = @"
namespace MyGeneratedNamespace
{
    public class MyGeneratedClass
    {
        public static void MyGeneratedMethod(string message) => System.Console.WriteLine($""From generated method: {message}"");
    }
}";

            context.AddSource("MyGeneratedClass.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // Nothing to do here
        }
    }
}
