
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
      string code = @" 
procedure First { 
x = 2; 
z = 3; 
call Second; } 
procedure Second { 
  x = 0;  
  i = 5; 
  while i  { 
   
x = x + 2 * y; 
   
   
call Third; 
i = i  - 1; } 
  if x then { 
   
x = x + 1; } 
else { 
  
z = 1; } 
  z = z + x + i; 
  y = z + 2;  
  x = x * y + z; } 
procedure Third { 
z = 5;   
v = z;  } 
";

      Lexer lexer = new Lexer(code);
      List<Token> tokens = lexer.GetTokens();
      Parser parser = new Parser(tokens);
      ASTNode ast = parser.ParseProgram();
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