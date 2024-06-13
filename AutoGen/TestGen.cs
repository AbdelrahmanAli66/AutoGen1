using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace AutoGen;

[Generator]
public class TestGen : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //var provider = context.SyntaxProvider.CreateSyntaxProvider(
        //    predicate: static (node, _) => node is ClassDeclarationSyntax,
        //    transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
        //    ).Where(m => m is not null);

        //var complication = context.CompilationProvider.Combine(provider.Collect());

        //context.RegisterSourceOutput(complication, Execute);
        var files11 = context.AdditionalTextsProvider.WithTrackingName("");
        var files = context.AdditionalTextsProvider
               .Where(a => a.Path.EndsWith(".txt"))
               .Select((a, c) => (Path.GetFileNameWithoutExtension(a.Path), a.GetText(c)!.ToString()));
        var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());
        context.RegisterSourceOutput(compilationAndFiles, (productionContext, sourceContext) => Generate(productionContext, sourceContext));
    }

    private void Generate(SourceProductionContext productionContext, (Compilation Left, ImmutableArray<(string, string)> Right) sourceContext)
    {
        var code = $@"namespace Consts;
    public static class ConstStrings
    {{
        public static List<string> Stringlist = [ {sourceContext.Right.FirstOrDefault().Item1}];
    }}
        ";
        productionContext.AddSource("GeneratedGen.g.cs", code);
    }




    //private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
    //{
    //    var code = @$"
    //        namespace TempTest;
    //        public static class TempClass
    //        {{
    //           public static string {tuple.Right.FirstOrDefault().Identifier.Value} {{get; set;}}
    //        }}
    //        ";
    //    context.AddSource("GeneratedGen.g.cs", code);
    //}
}
