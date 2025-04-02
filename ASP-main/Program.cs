
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace SPA_main
{
    class Program
    {
        static void Main()
        {
            string code = @"procedure First { 
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
v = z;  } ";

            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.GetTokens();
            Parser parser = new Parser(tokens);
            ASTNode ast = parser.ParseProgram();
            ast.PrintTree();
        }
    }
}