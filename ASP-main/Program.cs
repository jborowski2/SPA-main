
using ASP_main;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace SPA_main
{
    class Program
    {
        static void Main()
        {
            // Parse the program
            string code = @"procedure main {
            x = y + z;           
            while y {
                        x = 1 + y + x + 1 + 7;}
                    }";

            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.GetTokens();
            Parser parser = new Parser(tokens);
            ASTNode ast = parser.ParseProgram();
            Console.WriteLine("Program AST:");
            ast.PrintTree();

            // Process PQL query
            string query = "stmt s; Select s such that Modifies(s, \"x\")";
            Console.WriteLine("\nProcessing PQL query: " + query);

            PQLLexer pqlLexer = new PQLLexer(query);
            List<Token> pqlTokens = pqlLexer.GetTokens();
            PQLParser pqlParser = new PQLParser(pqlTokens);
            PQLQuery pqlQuery = pqlParser.ParseQuery();

            Console.WriteLine("\nQuery parsed:");
            Console.WriteLine($"Selected: {pqlQuery.Selected.Name}");
            foreach (var rel in pqlQuery.Relations)
            {
                Console.WriteLine($"Relation: {rel.Type}({rel.Arg1}, {rel.Arg2})");
            }

            // Analyze the query
            SPAAnalyzer analyzer = new SPAAnalyzer(ast);
            var results = analyzer.Analyze(pqlQuery);
            Console.WriteLine("\nResults:");
            foreach (var res in results)
            {
                Console.WriteLine(res);
            }
        }
    }
}