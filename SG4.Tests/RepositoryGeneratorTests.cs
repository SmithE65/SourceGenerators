using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SG4.SourceGenerators;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SG4.Tests
{
    public class RepositoryGeneratorTests
    {
        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        [Fact]
        public void RepositoryGeneratorRuns()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
#nullable enable
using System;
namespace MyCode.Data
{
    public class KeyAttribute : Attribute {}

    public class DbSet<T> {}

    public class ApplicationDbContext
    {
        public DbSet<Entity1> EntityOnes { get; set; }
        public DbSet<Entity2> EntityTwos { get; set; }
    }

    public class Entity1
    {
        [Key]
        public int Entity1ID { get; set; }
        public string AColumn { get; set; }
    }

    public class Entity2
    {
        [Key]
        public string Entity2ID { get; set; }
        public string AColumn { get; set; }
    }
}
");

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            var generator = new RepositoryGenerator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Assert.True(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            Assert.True(outputCompilation.SyntaxTrees.Count() == 3); // test input plus two generated repos
            var diag = outputCompilation.GetDiagnostics();
            Assert.Empty(diag); // verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            Assert.True(runResult.GeneratedTrees.Length == 1);
            Assert.True(runResult.Diagnostics.IsEmpty);

            // Or you can access the individual results on a by-generator basis
            GeneratorRunResult generatorResult = runResult.Results[0];
            Assert.True(generatorResult.Generator == generator);
            Assert.True(generatorResult.Diagnostics.IsEmpty);
            Assert.True(generatorResult.GeneratedSources.Length == 1);
            Assert.True(generatorResult.Exception is null);
        }
    }
}