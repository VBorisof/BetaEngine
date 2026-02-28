using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Beta.GuiHandlerGenerator
{
    [Generator]
    public class GuiHandlerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
               predicate: (s, t) => s is ClassDeclarationSyntax,
               transform: GetTypeSymbols).Collect();

            context.RegisterSourceOutput(classDeclarations, GenerateSource);
        }

        private ITypeSymbol GetTypeSymbols(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var decl = (ClassDeclarationSyntax)context.Node;

            if (context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is ITypeSymbol typeSymbol)
            {
                return typeSymbol;
            }

            return null;
        }

        private void GenerateSource(SourceProductionContext context, ImmutableArray<ITypeSymbol> typeSymbols)
        {
            const string baseTypeName = "GuiHandler";
            const string handlerAttributeName = "HandlerForAttribute";
            string ns = "Beta.GuiHandlerRegistries";

            var sb = new StringBuilder();

            sb.AppendLine("//////////////////////////////////////////////////");
            sb.AppendLine("// Auto-generated code.");
            sb.AppendLine("//////////////////////////////////////////////////");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Beta.Gui.Events;");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine();
            sb.AppendLine("public static class GuiHandlerRegistryBootstrap");
            sb.AppendLine("{");
            sb.AppendLine("    public static void Init()");
            sb.AppendLine("    {");
            sb.AppendLine("        var handlers = GuiHandlerRegistry.GetHandlers();");
            foreach (var symbol in typeSymbols)
            {
                if (symbol is null)
                {
                    continue;
                }

                // Only look in GuiHandler types.
                if (!string.Equals(symbol.BaseType.Name, baseTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var className = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine(
                    $"        {className}.RemoveInstance();"
                );
                sb.AppendLine(
                    $"        handlers[{className}.Instance] = [];"
                );

                var methods = symbol.GetMembers();
                foreach (var method in methods)
                {
                    var handlerForAttr = method.GetAttributes().FirstOrDefault(a =>
                        string.Equals(
                            a.AttributeClass.Name,
                            handlerAttributeName,
                            StringComparison.OrdinalIgnoreCase));

                    if (handlerForAttr is null)
                    {
                        continue;
                    }

                    var guiEventArg = handlerForAttr.ConstructorArguments[0];
                    var elemIdArg = handlerForAttr.ConstructorArguments[1];
                    var methodName = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    sb.AppendLine($"        handlers[{className}.Instance].Add(new GuiEventHandlerMapping");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            ElemId = \"{elemIdArg.Value.ToString()}\",");
                    sb.AppendLine($"            GuiEventType = (GuiEventType){guiEventArg.Value.ToString()},");
                    sb.AppendLine($"            Method = (typeof({className})).GetMethod(\"{methodName}\", BindingFlags.Instance | BindingFlags.Public),");
                    sb.AppendLine("        });");
                }
            }
            sb.AppendLine("    }");

            sb.AppendLine("}");

            context.AddSource($"GuiHandlerRegistryBootstrap.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}