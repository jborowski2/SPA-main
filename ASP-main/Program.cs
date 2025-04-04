
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
      string code = @"procedure Circle { 
 t = 1; 
 a = t + 10; 
 d = t * a + 2; 
 call Triangle; 
 b = t + a; 
 call Hexagon; 
 b = t + a; 
 if t then { 
       
k = a - d; 
       while c { 
          d = d + t; 
          c = d + 1; }
          a = d + t; }
          else { 
       a = d + t; 
       call Hexagon; 
       c = c - 1; } 
 call Rectangle; } 
procedure Rectangle { 
 while c  { 
       t = d + 3 * a + c; 
       call Triangle; 
       c = c + 20; } 
 d = t; } 
procedure Triangle { 
 while d { 
  if t then { 
    d = t + 2; } 
  else {
    a = t * a + d + k * b; }} 
c = t + k + d; } 
";

      Lexer lexer = new Lexer(code);
      List<Token> tokens = lexer.GetTokens();
      Parser parser = new Parser(tokens);
      ASTNode ast = parser.ParseProgram();
      ast.PrintTree();

      // Process PQL query
      string query = "stmt s; Select s such that Parent (s, 12) ";
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