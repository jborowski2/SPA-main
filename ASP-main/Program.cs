
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

            string filePath = "code.txt";
            string code = File.ReadAllText(filePath);
            Lexer lexer = new Lexer(code);
      List<Token> tokens = lexer.GetTokens();
      Parser parser = new Parser(tokens);
      ASTNode ast = parser.ParseProgram();
      ast.PrintTree();

      // Process PQL query
      string query = "assign a;  \r\nSelect a such that Uses(a, t) ";
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
            for (int i = 0; i < results.Count; i++)
            {
                Console.Write(results[i]);

                // Dodaj przecinek i spację jeśli to nie jest ostatni element
                if (i < results.Count - 1)
                {
                    Console.Write(", ");
                }
            }
        }
  }
}