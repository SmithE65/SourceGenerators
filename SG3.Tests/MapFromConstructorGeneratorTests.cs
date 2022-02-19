using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SG3.SourceGenerators;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SG3.Tests;

public class MapFromConstructorGeneratorTests
{
    private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

    [Fact]
    public void MapFromConstructorGeneratorRuns()
    {
        // Create the 'input' compilation that the generator will act on
        Compilation inputCompilation = CreateCompilation(@"
#nullable enable
using MapFrom;
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }

    public class MyModel
    {
        public User? User { get; set; }

        public int RandomNumber { get; set; }
    }

    public record class User(int Id, string Username);

    [MapFrom(typeof(MyModel))]
    public partial class MyViewModel
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int RandomNumber { get; set; }
    }
}
");

        // directly create an instance of the generator
        // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
        var generator = new MapFromConstructorGenerator();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        Assert.True(diagnostics.IsEmpty); // there were no diagnostics created by the generators
        Assert.True(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
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
