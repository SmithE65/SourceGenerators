using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SG4.SourceGenerators
{
    [Generator]
    public class RepositoryGenerator : ISourceGenerator
    {
        private const string dbContextName = "ApplicationDbContext";

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is DbSetSyntaxReceiver receiver))
            {
                return;
            }

            // TODO: Generate repositories
            var registrations = new List<(string RepoName, string InterfaceName)>();

            foreach (var type in receiver.EntityTypes)
            {
                var (repoName, interfaceName, source) = BuildRepository(type);
                if (!string.IsNullOrEmpty(repoName))
                {
                    context.AddSource($"{repoName}.g.cs", source);
                    registrations.Add((repoName, interfaceName));
                }
            }

            var regClass = BuildRegistrations(registrations);
            context.AddSource("RepositoryRegistration.g.cs", regClass);

            (string RepoName, string InterfaceName, string Source) BuildRepository(INamedTypeSymbol entity)
            {
                var typeName = entity.Name;
                var interfaceName = $"I{typeName}Repository";
                var className = $"{typeName}Repository";
                var key = entity.GetMembers()
                    .Where(x => x.Kind == SymbolKind.Property)
                    .Select(x => x as IPropertySymbol)
                    .FirstOrDefault(x => x != null && x.GetAttributes().Any(a => a.AttributeClass.Name == "KeyAttribute"));

                var baseInterfaces = $"IRepository<{typeName}>";
                var methodImplementations = new StringBuilder();

                if (key != null)
                {
                    if (key.Type.Name == "Int32")
                    {
                        baseInterfaces += $", IIntegerKey<{typeName}>";
                        methodImplementations.AppendLine($"\t public {typeName}? Find(int id) => Table.FirstOrDefault(x => x.{key.Name} == id);"); 
                    }
                    else if (key.Type.Name == "String")
                    {
                        baseInterfaces += $", IStringKey<{typeName}>";
                        methodImplementations.AppendLine($"\t public {typeName}? Find(string id) => Table.FirstOrDefault(x => x.{key.Name} == id);");
                    }
                }

                methodImplementations.AppendLine($"\t public {typeName}? Find(Expression<Func<{typeName}, bool>> expression) => Table.FirstOrDefault(expression);");
                methodImplementations.AppendLine($"\t public {typeName}[] GetAll() => Table.ToArray();");

                var namespaces = new List<string>
                {
                    "using System.Linq.Expressions;",
                    $"using {receiver.ContextNamespace};",
                    $"using {receiver.ContextNamespace}.Repositories.Base;",
                    $"using {entity.ContainingNamespace};"
                };

                var repo = $@"
{string.Join(Environment.NewLine, namespaces.OrderBy(x => x))}

namespace { receiver.ContextNamespace }.Repositories;

#nullable enable

public partial interface {interfaceName} : {baseInterfaces} {{ }}

internal partial class {className} : EfCoreRepositoryBase<{typeName}>, I{typeName}Repository
{{
    public {className}({dbContextName} context) : base(context) {{ }}
{methodImplementations}
}}
";

                return (className, interfaceName, repo);
            }

            string BuildRegistrations(List<(string RepoName, string InterfaceName)> repositories)
            {
                var sb = new StringBuilder();
                repositories.ForEach(x => sb.AppendLine($"\t\tservices.AddScoped<{x.InterfaceName},{x.RepoName}>();"));

                return $@"
using { receiver.ContextNamespace }.Repositories;
namespace { receiver.ContextNamespace };

public static class RepositoryRegistration
{{
    public static IServiceCollection RegisterRepos(this IServiceCollection services)
    {{
{sb}
        return services;
    }}
}}
";
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DbSetSyntaxReceiver());
        }

        class DbSetSyntaxReceiver : ISyntaxContextReceiver
        {
            public List<INamedTypeSymbol> EntityTypes { get; } = new List<INamedTypeSymbol>();
            public string ContextNamespace { get; private set; }

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                    if (symbol.Name == dbContextName)
                    {
                        ContextNamespace = symbol.ContainingNamespace.ToString();
                        var entitySymbols = symbol.GetMembers().Where(x => x.Kind == SymbolKind.Property && x is IPropertySymbol prop && prop.Type.Name.Contains("DbSet"))
                            .Select(x => x as IPropertySymbol)
                            .Select(x => x.Type as INamedTypeSymbol)
                            .Where(x => x != null && x.IsGenericType && x.TypeParameters.Count() == 1)
                            .SelectMany(x => x.TypeArguments.OfType<INamedTypeSymbol>());
                        EntityTypes.AddRange(entitySymbols);
                    }
                }
            }
        }
    }
}
