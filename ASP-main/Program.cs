
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace SPA_main
{
    class Program
    {
        static void Main()
        {
            string code = @"procedure main {x = y + z;           
          while y {x = 1 + y + x + 1 + 7;}}";

            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.GetTokens();
            Parser parser = new Parser(tokens);
            ASTNode ast = parser.ParseProgram();
            ast.PrintTree();
        }
    }
}