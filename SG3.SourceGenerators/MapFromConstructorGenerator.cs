using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG3.SourceGenerators
{
    [Generator]
    public class MapFromConstructorGenerator : ISourceGenerator
    {
        private const string mapFromAttributeText = @"
namespace MapFrom
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class MapFromAttribute : System.Attribute
    {
        public MapFromAttribute()
        {
        }

        public MapFromAttribute(System.Type sourceType)
        {
            SourceType = sourceType;
        }

        public System.Type SourceType { get; set; }
    }
}";

        private const string mapFromAttributeName = "MapFrom.MapFromAttribute";

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is MapFromSyntaxReceiver receiver))
            {
                return;
            }

            foreach (var target in receiver.TypesToMap)
            {
                var sourceTypes = target.GetAttributes()
                    .Where(x => x.AttributeClass.ToDisplayString() == mapFromAttributeName)
                    .Select(x => x.ConstructorArguments.FirstOrDefault().Value.ToString());

                foreach (var st in sourceTypes)
                {
                    var (hintName, source) = CreateConstructor(target.Name, st);
                    context.AddSource(hintName, source);
                }
            }

            // TODO: Create mapping and have constructor map from old type to new
            (string HintName, string Source) CreateConstructor(string targetName, string sourceName)
            {
                var hint = $"{targetName}_from_{sourceName}.g.cs";
                var source = $"public partial class {targetName} {{ public {targetName}({sourceName} val) {{ System.Console.WriteLine(\"Constructor created for {targetName} from {sourceName}.\"); }} }}";
                return (hint, source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization((i) => i.AddSource("MapFromAttribute.g.cs", mapFromAttributeText));
            context.RegisterForSyntaxNotifications(() => new MapFromSyntaxReceiver());
        }

        class MapFromSyntaxReceiver : ISyntaxContextReceiver
        {
            public List<INamedTypeSymbol> TypesToMap { get; } = new List<INamedTypeSymbol>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                    if (symbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == "MapFrom.MapFromAttribute"))
                    {
                        TypesToMap.Add(symbol);
                    }
                }
            }
        }
    }
}
