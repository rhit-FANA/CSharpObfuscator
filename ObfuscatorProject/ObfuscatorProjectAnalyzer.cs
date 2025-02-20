using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text;
namespace ObfuscatorProject
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ObfuscationAnalyzer : DiagnosticAnalyzer
    {
        private SemanticModel _semanticModel;
        public const string DiagnosticId = "ObfuscatorProject";
        //public HashSet<Node> visited = new HashSet<Node>();

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }


        

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeMethod);
        }
        private void AnalyzeMethod(CompilationAnalysisContext context)
        {
            Console.WriteLine("Analyze Calls");
            var compilation = context.Compilation;
            var decorator = new ForLoopDecorator(new IfStatementDecorator(new DeadSpaceDecorator(new VariableAliasDecorator(new MethodAliasDecorator(new CoreObfuscator())))));

            foreach (var syntaxTrees in context.Compilation.SyntaxTrees)
            {
                //Console.WriteLine($"{syntaxTrees.ToString()}");
                Console.WriteLine(decorator.Obfuscate(syntaxTrees).GetRoot().ToFullString());

            }

        }
    }   
}
