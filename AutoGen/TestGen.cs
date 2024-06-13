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
        IncrementalValuesProvider<AdditionalText> textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".txt"));
        IncrementalValuesProvider<(string name, string content)> namesAndContents = textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path), content: text.GetText(cancellationToken)!.ToString()));
        var complication = context.CompilationProvider.Combine(namesAndContents.Collect());
        context.RegisterSourceOutput(namesAndContents, (spc, nameAndContent) =>
        {
            var x = nameAndContent;
            spc.AddSource($"ConstStrings.{nameAndContent.name}", $@"
    public static partial class ConstStrings
    {{
        public const string {nameAndContent.name} = ""{nameAndContent.content}"";
    }}");
        });
        context.RegisterSourceOutput(complication, Extract);
    }

    private void Extract(SourceProductionContext context, (Compilation Left, ImmutableArray<(string name, string content)> Right) tuple)
    {
        var code = $@"ConstStrings.{tuple.Right.FirstOrDefault().name}
    public static partial class ConstStrings
    {{
        public const string {tuple.Right.FirstOrDefault().name} = ""{tuple.Right.FirstOrDefault().content}"";
    }}";
        context.AddSource("GeneratedGen.g.cs", code);

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
