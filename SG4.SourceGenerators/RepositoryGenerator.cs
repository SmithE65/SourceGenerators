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
                var (repoMetaData, source) = BuildRepository(receiver.ContextNamespace, type);
                if (!string.IsNullOrEmpty(repoMetaData.RepoName))
                {
                    context.AddSource($"{repoMetaData.RepoName}.g.cs", source);
                    context.AddSource($"{type.Name}Controller.g.cs", BuildCrudController(repoMetaData, receiver.ContextNamespace));
                    registrations.Add((repoMetaData.RepoName, repoMetaData.RepoInterfaceName));
                }
            }

            var regClass = BuildRegistrations(receiver.ContextNamespace, registrations);
            context.AddSource("RepositoryRegistration.g.cs", regClass);
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

        #region TypeBuilders

        private static (RepoMetaData Metadata, string Source) BuildRepository(string baseNamespace, INamedTypeSymbol entity)
        {
            var repoMetaData = new RepoMetaData
            {
                EntityName = entity.Name,
                EntityNamespace = entity.ContainingNamespace.ToString(),
                RepoInterfaceName = $"I{entity.Name}Repository",
                RepoName = $"{entity.Name}Repository",
                RepoNamespace = $"{baseNamespace}.Repositories"
            };

            var baseInterfaces = $"IRepository<{repoMetaData.EntityName}>";
            var methodImplementations = new StringBuilder();

            var key = entity.GetMembers()
                .Where(x => x.Kind == SymbolKind.Property)
                .Select(x => x as IPropertySymbol)
                .FirstOrDefault(x => x != null && x.GetAttributes().Any(a => a.AttributeClass.Name == "KeyAttribute"));

            if (key != null)
            {
                if (key.Type.Name == "Int32")
                {
                    repoMetaData.HasFindByInt = true;
                    baseInterfaces += $", IIntegerKey<{repoMetaData.EntityName}>";
                    methodImplementations.AppendLine($"\t public {repoMetaData.EntityName}? Find(int id) => Table.FirstOrDefault(x => x.{key.Name} == id);");
                }
                else if (key.Type.Name == "String")
                {
                    repoMetaData.HasFindByString = true;
                    baseInterfaces += $", IStringKey<{repoMetaData.EntityName}>";
                    methodImplementations.AppendLine($"\t public {repoMetaData.EntityName}? Find(string id) => Table.FirstOrDefault(x => x.{key.Name} == id);");
                }
            }

            methodImplementations.AppendLine($"\t public {repoMetaData.EntityName}? Find(Expression<Func<{repoMetaData.EntityName}, bool>> expression) => Table.FirstOrDefault(expression);");
            methodImplementations.AppendLine($"\t public {repoMetaData.EntityName}[] GetAll() => Table.Take(10).ToArray();"); // Don't want GetAll for most types in the real world, so limiting to 10

            var namespaces = new List<string>
                {
                    "using System.Linq.Expressions;",
                    $"using {baseNamespace};",
                    $"using {repoMetaData.RepoNamespace}.Base;",
                    $"using {repoMetaData.EntityNamespace};"
                };

            var repo = $@"
{string.Join(Environment.NewLine, namespaces.OrderBy(x => x))}

namespace {repoMetaData.RepoNamespace};

#nullable enable

public partial interface {repoMetaData.RepoInterfaceName} : {baseInterfaces} {{ }}

internal partial class {repoMetaData.RepoName} : EfCoreRepositoryBase<{repoMetaData.EntityName}>, {repoMetaData.RepoInterfaceName}
{{
    public {repoMetaData.RepoName}({dbContextName} context) : base(context) {{ }}
{methodImplementations}
}}
";

            return (repoMetaData, repo);
        }

        private static string BuildRegistrations(string baseNamespace, List<(string RepoName, string InterfaceName)> repositories)
        {
            var sb = new StringBuilder();
            repositories.ForEach(x => sb.AppendLine($"\t\tservices.AddScoped<{x.InterfaceName},{x.RepoName}>();"));

            return $@"
using {baseNamespace}.Repositories;
namespace {baseNamespace};

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

        private static string BuildCrudController(RepoMetaData data, string baseNamespace)
        {
            var sb = new StringBuilder();
            if (data.HasFindByInt)
            {
                sb.AppendLine($@"
    [HttpGet(""{{id}}"")]
    public IActionResult GetById(int id)
    {{
        return Ok(_repo.Find(id));
    }}
");
            }
            if (data.HasFindByString)
            {
                sb.AppendLine($@"
    [HttpGet(""{{id}}"")]
    public IActionResult GetById(string id)
    {{
        return Ok(_repo.Find(id));
    }}
");
            }

            return $@"
using {data.RepoNamespace};
using Microsoft.AspNetCore.Mvc;

namespace {baseNamespace}.Controllers;

[Route(""api/[controller]/[action]"")]
[ApiController]
public partial class {data.EntityName}Controller : ControllerBase
{{
    private readonly {data.RepoInterfaceName} _repo;
    public {data.EntityName}Controller({data.RepoInterfaceName} repo)
    {{
        _repo = repo;
    }}

{sb}

    [HttpGet]
    public IActionResult GetAll()
    {{
        return Ok(_repo.GetAll());
    }}
}}
";
        }

        private class RepoMetaData
        {
            public string EntityName { get; set; }
            public string EntityNamespace { get; set; }
            public string RepoName { get; set; }
            public string RepoInterfaceName { get; set; }
            public string RepoNamespace { get; set; }
            public bool HasFindByInt { get; set; }
            public bool HasFindByString { get; set; }
        }

        #endregion
    }
}
