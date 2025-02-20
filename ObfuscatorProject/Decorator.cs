using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ObfuscatorProject
{
    public class Decorator : ObfuscatorComponent
    {
        protected ObfuscatorComponent decorator;
        protected static Random random = new Random();

        public Decorator(ObfuscatorComponent decorator)
        {
            this.decorator = decorator;
            
        }

        // this method looking mad sus
        public string GenerateRandomString(int length)
        {
            const string firstchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char firstChar = firstchars[random.Next(firstchars.Length)];
            var rest = new string(Enumerable.Repeat(chars, length-1)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return firstChar + rest;
        }

        public virtual SyntaxTree Obfuscate(SyntaxTree tree)
        {
            return this.decorator.Obfuscate(tree);
        }
    }

    
    public class VariableAliasDecorator : Decorator
    {   
        public VariableAliasDecorator(ObfuscatorComponent decorator) : base(decorator)
        {

        }

        public SyntaxTree ObfuscateDeclarationHelper(SyntaxTree tree, Dictionary<string, string> dict) 
        {
            var root = tree.GetRoot();
            var declarations = root.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
            foreach (var declaration in declarations)
            {
                foreach (var variable in declaration.Variables) 
                {
                    if(dict.Count == 0)
                    {
                        string newName = GenerateRandomString(random.Next(4, 12));
                        dict.Add(variable.Identifier.ToString(), newName);
                        var newDeclarationIdentifier = variable.WithIdentifier(SyntaxFactory.Identifier(newName)).WithAdditionalAnnotations();
                        root = root.ReplaceNode(variable, newDeclarationIdentifier);
                        return ObfuscateDeclarationHelper(root.SyntaxTree, dict);
                    }
                    if (dict.ContainsValue(variable.Identifier.ToString()))
                    {
                        continue;
                    }
                    string obsName = GenerateRandomString(random.Next(4, 12));
                    if (!dict.Keys.Contains(variable.Identifier.ToString()))
                    {
                        dict.Add(variable.Identifier.ToString(), obsName);
                    }
                    var newDec = variable.WithIdentifier(SyntaxFactory.Identifier(obsName));
                    root = root.ReplaceNode(variable, newDec);
                    return ObfuscateDeclarationHelper(root.SyntaxTree, dict);

                }    
            }
            return tree;
        }
        public override SyntaxTree Obfuscate(SyntaxTree tree)
        {
            Dictionary<String, String> newVarNames = new Dictionary<String, String>();
            var root = ObfuscateDeclarationHelper(tree, newVarNames).GetRoot();
            root = ObfuscateIdentifierHelper(root.SyntaxTree, newVarNames).GetRoot();

            return this.decorator.Obfuscate(root.SyntaxTree);
        }
        public SyntaxTree ObfuscateIdentifierHelper(SyntaxTree tree, Dictionary<string, string> dict)
        {
            var root = tree.GetRoot();
            var identifiers = root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var key in dict.Keys)
            {
                var identifierNodes = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id => id.Identifier.Text.Equals(key))
                        .ToList();
                if(identifierNodes.Count == 0)
                {
                    dict.Remove(key);
                    return ObfuscateIdentifierHelper(tree, dict);
                }
                var newIdentifier = identifierNodes[0].WithIdentifier(SyntaxFactory.Identifier(dict[key]).WithLeadingTrivia(identifierNodes[0].GetLeadingTrivia()).WithTrailingTrivia(identifierNodes[0].GetTrailingTrivia()));
                root = root.ReplaceNode(identifierNodes[0], newIdentifier);
                return ObfuscateIdentifierHelper(root.SyntaxTree, dict);
            }
            return tree;
        }
    }
    public class IfStatementDecorator : Decorator
    {
        SyntaxAnnotation generatedAnnotation = new SyntaxAnnotation("Generated");
        

        public IfStatementDecorator(ObfuscatorComponent decorator) : base(decorator)
        {
        }
        public override SyntaxTree Obfuscate(SyntaxTree tree)
        {
            
            List<string> allIfs = new List<string>();
            var root = tree.GetRoot();
            var methodsInClass = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
            List<string> methodsAsString = new List<string>();
            foreach (var method in methodsInClass)
            {
                methodsAsString.Add(method.ToString());
            }
            root = DeclareSthelper(tree, methodsAsString).GetRoot();
            var ifStatements = root.DescendantNodesAndSelf().OfType<IfStatementSyntax>();
            foreach (var ifStatement in ifStatements)
            {
                if (ifStatement.GetAnnotations("Generated").Count() == 0)
                {
                    allIfs.Add(ifStatement.ToString());
                }
            }
            root = IfStatementHelper(root.SyntaxTree, allIfs).GetRoot();
            var allWhiles = root.DescendantNodesAndSelf().OfType<WhileStatementSyntax>();
            List<string> generatedWhiles = new List<string>();
            foreach(var whileStatement in allWhiles)
            {
                if(whileStatement.GetAnnotations("Generated").Count() != 0 && whileStatement.GetAnnotations("Generated Loop").Count() == 0)
                {
                    generatedWhiles.Add(whileStatement.ToString());
                }
            }
            tree = StResetHelper(root.SyntaxTree, generatedWhiles);
            return base.Obfuscate(tree);
        }

        private SyntaxTree StResetHelper(SyntaxTree tree, List<string> generatedWhiles)
        {
            if(generatedWhiles.Count() == 0)
            {
                return tree;
            }
            var root = tree.GetRoot();
            var allWhiles = root.DescendantNodesAndSelf().OfType<WhileStatementSyntax>();
            foreach (var whileStatement in allWhiles) 
            {
                if (generatedWhiles.Contains(whileStatement.ToString()))
                {
                    List<SyntaxNode> toInsert = new List<SyntaxNode>();
                    toInsert.Add(SyntaxFactory.ParseStatement("__st__ = 1;"));
                    root = root.InsertNodesBefore(whileStatement,toInsert);
                    generatedWhiles.Remove(whileStatement.ToString());
                    return StResetHelper(root.SyntaxTree, generatedWhiles);
                }
            }
            return tree;
        }

        public SyntaxTree DeclareSthelper(SyntaxTree tree, List<String> methods)
        {
            var root = tree.GetRoot();
            var methodsInClass = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();

            foreach (var method in methodsInClass)
            {
                if (methods.Count > 0 & methods.Contains(method.ToString()))
                {
                    var stInsert = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var ")).AddVariables(SyntaxFactory.VariableDeclarator("__st__").WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))))));
                    var newBody = method.Body.WithStatements(method.Body.Statements.Insert(0,stInsert));
                    var newMethod = method.WithBody(newBody);
                    root = root.ReplaceNode(method, newMethod);
                    methods.Remove(method.ToString());
                    return DeclareSthelper(root.SyntaxTree, methods);
                }
            }
            return tree;
        }

        public SyntaxTree IfStatementHelper(SyntaxTree tree, List<string> ifs)
        {
            var root = tree.GetRoot();
            
            var ifStatements = root.DescendantNodesAndSelf().OfType<IfStatementSyntax>();
            if (ifs.Count() == 0) 
            {
                return root.SyntaxTree;
            }
            foreach (var ifStatement in ifStatements)
            {
                if (ifs.Contains(ifStatement.ToString()))
                {
                    var asBlock = (BlockSyntax) ifStatement.Statement;
                    var blockStatements = asBlock.Statements;
                    var whileStatement = SyntaxFactory.WhileStatement(SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression,SyntaxFactory.IdentifierName("__st__"), SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))
                , // while(__st__)
                    SyntaxFactory.Block( // Block of the while loop
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("__st__"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))
                            ),
                            SyntaxFactory.Block(
                                SyntaxFactory.IfStatement(
                                    ifStatement.Condition
                                    ,
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.IdentifierName("__st__"),
                                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(3))
                                            )
                                        ),
                                        SyntaxFactory.ContinueStatement()
                                    )
                                ).WithAdditionalAnnotations(generatedAnnotation),
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("__st__"),
                                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(2))
                                    )
                                ),
                                SyntaxFactory.ContinueStatement()
                            )
                        ).WithAdditionalAnnotations(generatedAnnotation),
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("__st__"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(2))
                            ),
                            SyntaxFactory.Block(
                                ElseBlockHelper(ifStatement),SyntaxFactory.ParseStatement("continue;")// Else clause
                            )
                        ).WithAdditionalAnnotations(generatedAnnotation),
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("__st__"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(3))
                            ),

                                asBlock.AddStatements(SyntaxFactory.ParseStatement("__st__ = 0;")).AddStatements(SyntaxFactory.ParseStatement("continue;"))// if clause
                           
                        ).WithAdditionalAnnotations(generatedAnnotation)
                    )
                ).WithAdditionalAnnotations(generatedAnnotation);
                    ifs.Remove(ifStatement.ToString());
                    root = root.ReplaceNode(ifStatement, whileStatement);
                    return IfStatementHelper(root.SyntaxTree, ifs);
                }
            }


            return root.SyntaxTree;
        }
        public StatementSyntax ElseBlockHelper( IfStatementSyntax statement)
        {
            if(statement.Else == null)
            {
                return SyntaxFactory.EmptyStatement();
            }
            return statement.Else.Statement;
        }
    }

    public class ForLoopDecorator : Decorator
    {
        private SyntaxAnnotation generatedAnnotation = new SyntaxAnnotation("Generated");
        private SyntaxAnnotation generatedAnnotation2 = new SyntaxAnnotation("Generated Loop");

        public ForLoopDecorator(ObfuscatorComponent decorator) : base(decorator)
        {
        }

        public override SyntaxTree Obfuscate(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var methods = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
            List<string> methodStrings = new List<string>();
            foreach(var methodStatement in methods)
            {
                methodStrings.Add(methodStatement.ToString());
            }
            root = DeclareLthelper(tree, methodStrings).GetRoot();
            var forLoops = root.DescendantNodesAndSelf().OfType<ForStatementSyntax>();
            List<string> fors = new List<string>();
            foreach (var forStatement in forLoops)
            {
                fors.Add(forStatement.ToString());
            }
            root = ForStatementHelper(root.SyntaxTree,fors).GetRoot();
            var allWhiles = root.DescendantNodesAndSelf().OfType<WhileStatementSyntax>();
            List<string> generatedWhiles = new List<string>();
            foreach (var whileStatement in allWhiles)
            {
                if (whileStatement.GetAnnotations("Generated Loop").Count() != 0)
                {
                    generatedWhiles.Add(whileStatement.ToString());
                }
            }
            root = LtResetHelper(root.SyntaxTree,generatedWhiles).GetRoot();
            return base.Obfuscate(root.SyntaxTree);
        }

        public SyntaxTree DeclareLthelper(SyntaxTree tree, List<String> methods)
        {
            var root = tree.GetRoot();
            var methodsInClass = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();

            foreach (var method in methodsInClass)
            {
                if (methods.Count > 0 & methods.Contains(method.ToString()))
                {
                    var stInsert = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var ")).AddVariables(SyntaxFactory.VariableDeclarator("__lt__").WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))))));
                    var newBody = method.Body.WithStatements(method.Body.Statements.Insert(0, stInsert));
                    var newMethod = method.WithBody(newBody);
                    root = root.ReplaceNode(method, newMethod);
                    methods.Remove(method.ToString());
                    return DeclareLthelper(root.SyntaxTree, methods);
                }
            }
            return tree;
        }
        private SyntaxTree LtResetHelper(SyntaxTree tree, List<string> generatedWhiles)
        {
            if (generatedWhiles.Count() == 0)
            {
                return tree;
            }
            var root = tree.GetRoot();
            var allWhiles = root.DescendantNodesAndSelf().OfType<WhileStatementSyntax>();
            foreach (var whileStatement in allWhiles)
            {
                if (generatedWhiles.Contains(whileStatement.ToString()))
                {
                    List<SyntaxNode> toInsert = new List<SyntaxNode>();
                    toInsert.Add(SyntaxFactory.ParseStatement("__lt__ = 1;"));
                    root = root.InsertNodesBefore(whileStatement, toInsert);
                    generatedWhiles.Remove(whileStatement.ToString());
                    return LtResetHelper(root.SyntaxTree, generatedWhiles);
                }
            }
            return tree;
        }

        public SyntaxTree ForStatementHelper(SyntaxTree tree, List<string> fors)
        {
            var root = tree.GetRoot();

            var forStatements = root.DescendantNodesAndSelf().OfType<ForStatementSyntax>();
            if (fors.Count() == 0)
            {
                return root.SyntaxTree;
            }
            foreach (var forStatement in forStatements)
            {
                if (fors.Contains(forStatement.ToString()))
                {
                    var asBlock = (BlockSyntax)forStatement.Statement;
                    var blockStatements = asBlock.Statements;
                    var whileStatement = SyntaxFactory.WhileStatement(SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression, SyntaxFactory.IdentifierName("__lt__"), SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))
                , // while(__st__)
                    SyntaxFactory.Block( // Block of the while loop
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("__lt__"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)) // 1
                            ),
                            SyntaxFactory.Block(
                                SyntaxFactory.IfStatement(
                                    forStatement.Condition
                                    ,
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.IdentifierName("__lt__"),
                                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(2))
                                            )
                                        ),
                                        SyntaxFactory.ContinueStatement()
                                    )
                                ).WithAdditionalAnnotations(generatedAnnotation),

                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("__lt__"),
                                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(3))
                                    )
                                ),
                                SyntaxFactory.ContinueStatement()
                            )
                        ).WithAdditionalAnnotations(generatedAnnotation),
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("__lt__"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(2)) // 2
                            ),
                                asBlock.AddStatements(SyntaxFactory.ParseStatement("__lt__ = 1;")).AddStatements(SyntaxFactory.ParseStatement(forStatement.Incrementors.ToFullString() + ";")).AddStatements(SyntaxFactory.ParseStatement("continue;"))// body                                
                            
                        ).WithAdditionalAnnotations(generatedAnnotation),
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("__lt__"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(3)) // 3
                            ),

                                SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement("__lt__ = 0;")).AddStatements(SyntaxFactory.ParseStatement("continue;"))
                        ).WithAdditionalAnnotations(generatedAnnotation)
                    )
                ).WithAdditionalAnnotations(generatedAnnotation).WithAdditionalAnnotations(generatedAnnotation2);
                    fors.Remove(forStatement.ToString());
                    root = root.ReplaceNode(forStatement, whileStatement);
                    foreach(var whileCandidate in root.DescendantNodesAndSelf().OfType<WhileStatementSyntax>())
                    {
                        if (whileCandidate.ToString().Equals(whileStatement.ToString()))
                        {
                            List<SyntaxNode> toInsert = new List<SyntaxNode>();
                            toInsert.Add(SyntaxFactory.ParseStatement(forStatement.Declaration.ToString() + ";"));
                            root = root.InsertNodesBefore(whileCandidate, toInsert);
                        }
                    }
                    
                    return ForStatementHelper(root.SyntaxTree, fors);
                }
            }
            return null;
        }
        }



    public class DeadSpaceDecorator : Decorator
    {
        public DeadSpaceDecorator(ObfuscatorComponent decorator) : base(decorator) { }

        public override SyntaxTree Obfuscate(SyntaxTree tree)
        {
            List<string> initialAssignments = new List<string>();
            var root = tree.GetRoot();
            var assignmentExpressions = root.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>();
            foreach (var assignment in assignmentExpressions)
            {
                int randomize = random.Next(1, 100);
                if (randomize > 40)
                {
                    initialAssignments.Add(assignment.ToString());
                }
            }
            root = DeadSpaceHelper(tree, initialAssignments).GetRoot();
            return this.decorator.Obfuscate(root.SyntaxTree);

        }
        public SyntaxTree DeadSpaceHelper(SyntaxTree tree, List<string> assignments)
        {
            var root = tree.GetRoot();
            var assignmentExpressions = root.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>();
            if (assignments.Count == 0)
            {
                return tree;
            }
            foreach (var assignment in assignmentExpressions)
            {
                if (assignments.First().Equals(assignment.ToString()))
                {
                    var assignmentParent = assignment.Parent as ExpressionStatementSyntax;
                    var trueExpression = SyntaxFactory.ParseExpression("true");
                    SyntaxAnnotation generatedAnnotation = new SyntaxAnnotation("Generated");
                    var expressionBlock = SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ExpressionStatement(assignment))).
                        WithLeadingTrivia(assignment.GetLeadingTrivia().Insert(0,SyntaxFactory.CarriageReturnLineFeed));
                    var uselessIf = SyntaxFactory.IfStatement(trueExpression, expressionBlock).WithLeadingTrivia(assignment.GetLeadingTrivia()).WithTrailingTrivia(assignment.GetTrailingTrivia()).
                        WithLeadingTrivia(assignmentParent.GetLeadingTrivia()).WithTrailingTrivia(assignmentParent.GetTrailingTrivia()).WithAdditionalAnnotations(generatedAnnotation);
                    root = root.ReplaceNode(assignmentParent, uselessIf);
                    assignments.Remove(assignments.First());
                    return DeadSpaceHelper(root.SyntaxTree, assignments);
                }
            }
            return tree;
        }
    }


    public class MethodAliasDecorator: Decorator
    {
        public MethodAliasDecorator(ObfuscatorComponent decorator) : base(decorator)
        {

        }

        public override SyntaxTree Obfuscate(SyntaxTree tree)
        {

            Dictionary<String, String> newMethodNames = new Dictionary<String, String>();
            var root = tree.GetRoot();
            var declarations = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
            foreach (var declaration in declarations)
            {
                newMethodNames.Add(declaration.Identifier.ToString(),GenerateRandomString(random.Next(8, 15)));
            }
            foreach (var key in newMethodNames.Keys)
            {
                var identifierNodes = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Where(id => id.Identifier.Text == key)
                        .ToList();

                foreach (var identifier in identifierNodes)
                {
                    var newIdentifier = identifier.WithIdentifier(SyntaxFactory.Identifier(newMethodNames[key]));
                    root = root.ReplaceNode(identifier, newIdentifier);
                }
            }

            return this.decorator.Obfuscate(root.SyntaxTree);
        }
    }
    //
    public class CoreObfuscator : ObfuscatorComponent
    {
        
        public SyntaxTree Obfuscate(SyntaxTree tree)
        {
            return tree;
        }
    }
}
