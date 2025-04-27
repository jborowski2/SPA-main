
using ASP_main;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

            PKB pkb = PKB.GetInstance();
            pkb.SetRoot(ast);

            // now you can use dictionary

            foreach (var pair in pkb.LineToNode)
            {
                Console.WriteLine($"Linia: {pair.Key}, hash code węzła: {pair.Value.GetHashCode()}");
            }
            var node = pkb.GetNodeByLine(11);

            //Console.WriteLine($"parametr : {11}, linia wezła {node.LineNumber}");
            pkb.Root.PrintTree();


            Console.WriteLine("\nWypisuję zawartość słowników i zbiorów w PKB:\n");

            Console.WriteLine("Follows:");
            foreach (var pair in pkb.Follows ?? new Dictionary<string, string>())
            {
                Console.WriteLine($"{pair.Key} -> {pair.Value}");
            }

            Console.WriteLine("\nParent:");
            foreach (var pair in pkb.Parent ?? new Dictionary<string, List<string>>())
            {
                Console.WriteLine($"{pair.Key} -> [{string.Join(", ", pair.Value)}]");
            }

            Console.WriteLine("\nIsParent:");
            foreach (var pair in pkb.IsParent ?? new HashSet<(string Parent, string Child)>())
            {
                Console.WriteLine($"{pair.Parent} -> {pair.Child}");
            }

            Console.WriteLine("\nModifiesStmt:");
            foreach (var pair in pkb.ModifiesStmt ?? new Dictionary<string, List<string>>())
            {
                Console.WriteLine($"{pair.Key} -> [{string.Join(", ", pair.Value)}]");
                // TO DO modifies doać terzeba ify i while , dodawnie parenta przy dodawniu modifies i tak w góre ,przy uses jak jest petla to ten warunek w petli jest używany 
            }

            Console.WriteLine("\nModifiesVar:");
            foreach (var pair in pkb.ModifiesVar ?? new Dictionary<string, List<string>>())
            {
                Console.WriteLine($"{pair.Key} -> [{string.Join(", ", pair.Value)}]");
            }

            Console.WriteLine("\nIsModifiesStmtVar:");
            foreach (var pair in pkb.IsModifiesStmtVar ?? new HashSet<(string Stmt, string Var)>())
            {
                Console.WriteLine($"{pair.Stmt} -> {pair.Var}");
            }

            Console.WriteLine("\nIsModifiesProcVar:");
            foreach (var pair in pkb.IsModifiesProcVar ?? new HashSet<(string Proc, string Var)>())
            {
                Console.WriteLine($"{pair.Proc} -> {pair.Var}");
            }

            Console.WriteLine("\nUsesStmt:");
            foreach (var pair in pkb.UsesStmt ?? new Dictionary<string, List<string>>())
            {
                Console.WriteLine($"{pair.Key} -> [{string.Join(", ", pair.Value)}]");// jego parent też używa tej zmiennej 
            }

            Console.WriteLine("\nUsesVar:");
            foreach (var pair in pkb.UsesVar ?? new Dictionary<string, List<string>>())
            {
                Console.WriteLine($"{pair.Key} -> [{string.Join(", ", pair.Value)}]");
            }

            Console.WriteLine("\nIsUsesStmtVar:");        // ile bedzie procedur w tescie ostatecznie bo np A -> B -> C x jest w C czuli a i b też używają x komplikacja złożoności obliczeniowej
            foreach (var pair in pkb.IsUsesStmtVar ?? new HashSet<(string Stmt, string Var)>())
            {
                Console.WriteLine($"{pair.Stmt} -> {pair.Var}");
            }

            Console.WriteLine("\nIsUsesProcVar:");
            foreach (var pair in pkb.IsUsesProcVar ?? new HashSet<(string Proc, string Var)>())
            {
                Console.WriteLine($"{pair.Proc} -> {pair.Var}");
            }
            Console.WriteLine("\nCalls:");
            foreach (var pair in pkb.Calls ?? new Dictionary<string, List<string>>())
            {
                Console.WriteLine($"{pair.Key} -> [{string.Join(", ", pair.Value)}]");
            }

            Console.WriteLine("\nIsCalls:");
            foreach (var pair in pkb.IsCalls ?? new HashSet<(string, string)>())
            {
                Console.WriteLine($"{pair.Item1} -> {pair.Item2}");
            }

            Console.WriteLine("\nIsCallsStar:");
            foreach (var pair in pkb.IsCallsStar ?? new HashSet<(string, string)>())
            {
                Console.WriteLine($"{pair.Item1} -> {pair.Item2}");
            }

            Console.WriteLine("\nConsts:");
            foreach (var pair in pkb.ConstValues )
            {
                Console.WriteLine($"{pair}");
            }
            Console.WriteLine("\nAssigns:");
            foreach (var pair in pkb.Assings)
            {
                Console.WriteLine($"{pair}");
            }
            Console.WriteLine("\nWhiles:");
            foreach (var pair in pkb.Whiles)
            {
                Console.WriteLine($"{pair}");
            }
            Console.WriteLine("\nIfs:");
            foreach (var pair in pkb.Ifs)
            {
                Console.WriteLine($"{pair}");
            }
            Console.WriteLine("\nVariables:");
            foreach (var pair in pkb.Variables)
            {
                Console.WriteLine($"{pair}");
            }
            Console.WriteLine("\nProcedures:");
            foreach (var pair in pkb.Procedures)
            {
                Console.WriteLine($"{pair}");
            }
            Console.WriteLine("\nKoniec wypisywania słowników i zbiorów.");

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


            string query = " stmt s; Select s such that Follows (4, s)";
            while (true)
            {
                
               // string query = ReadPqlQuery();
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
                    SPAAnalyzer analyzer = new SPAAnalyzer(pkb);
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
                break;
            }

        }
    }
}
