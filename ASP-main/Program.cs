
using ASP_main;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;



namespace SPA_main
{
    class Program
    {
        static string ReadPqlQuery()
        {
            StringBuilder queryBuilder = new StringBuilder();

            // Wczytaj pierwszą linię
            string line1 = Console.ReadLine();
            if (line1 == null) return null;

            // Wczytaj drugą linię
            string line2 = Console.ReadLine();
            if (line2 == null) return null;
           
            // Połącz linie z zachowaniem formatowania
            return line1 + " " + line2;
        }

        static void Main(string[] args)
        {
            string code;
            if (args.Length == 0)
            {
                code = "code.txt";
            }
            else
                code = args[0];
            code = File.ReadAllText(code);

            
            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.GetTokens();
            Parser parser = new Parser(tokens);
            ASTNode ast = parser.ParseProgram();
              ast.PrintTree();
            Console.WriteLine("Ready");

            //   
            // Process PQL query
            //   string query = " stmt s, s1; Select s such that Modifies (s, t)";
            // string query = " variable v; Select v such that Modifies (1, v)";
            //     string query = " stmt s, s1; Select s such that Uses (s, t)";

            //  string query = " variable v; Select BOOLEAN such that Modifies(19, \"t\")";
            // string query = " stmt s, s1; Select s such that Parent (s, s1) with s1.stmt# = 9";
            //  string query = " stmt s, s1;\n Select s such that Follows (s, s1) with s1.stmt# = 9";
            //    Console.WriteLine("\nProcessing PQL query: " + query);


            string query = " stmt s; Select s such that Follows (2, 2)";
            while (true)
            {
                
                string query = ReadPqlQuery();
                //Console.WriteLine(query);
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
                // Console.WriteLine("\nResults:");
                string wynik = null;
                    for (int i = 0; i < results.Count; i++)
                    {
                    wynik += results[i];

                        // Dodaj przecinek i spację jeśli to nie jest ostatni element
                        if (i < results.Count - 1)
                        {
                            wynik += ", ";
                        }
                        
                    }
                Console.WriteLine(wynik);
            }

        }
    }
}
