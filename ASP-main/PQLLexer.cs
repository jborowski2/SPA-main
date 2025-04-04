using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASP_main
{
    class PQLLexer
    {
        private static readonly (string, string)[] TokenSpecs =
        {
        ("SELECT", "Select"),
        ("SUCH_THAT", "such that"),
        ("MODIFIES", "Modifies"),
        ("USES", "Uses"),
        ("PARENT", "Parent(?!\\*)"),
        ("PARENT_STAR", "Parent\\*"),
        ("PATTERN", "pattern"),
        ("WITH", "with"),
        ("AND", "and"),
        ("STMT", "stmt"),
        ("VARIABLE", "variable"),
        ("PROCEDURE", "procedure"),
        ("CONSTANT", "constant"),
        ("NUMBER", "\\d+"),
        ("NAME", "[a-zA-Z][a-zA-Z0-9]*"),
        ("COMMA", ","),
        ("SEMICOLON", ";"),
        ("DOT", "\\."),
        ("QUOTE", "\""),
        ("LPAREN", "\\("),
        ("RPAREN", "\\)"),
        ("UNDERSCORE", "_"),
        ("SKIP", "[ \\t\\n]+")
    };

        private readonly string _query;
        private readonly List<Token> _tokens = new List<Token>();

        public PQLLexer(string query)
        {
            _query = query;
            Tokenize();
            PrintTokens();
        }

        private void PrintTokens()
        {
            Console.WriteLine("=== Tokens ===");
            foreach (var token in _tokens)
            {
                Console.WriteLine(token.ToString());
            }
            Console.WriteLine("==============");
        }
        private void Tokenize()
        {
            string pattern = string.Join("|", TokenSpecs.Select(spec => $"(?<{spec.Item1}>{spec.Item2})"));
            int lineNumber = 1;
            int currentLineStart = 0;

            // Dzielimy zapytanie na linie, aby śledzić numery linii
            string[] lines = _query.Split('\n');

            foreach (string line in lines)
            {
                foreach (Match match in Regex.Matches(line, pattern, RegexOptions.IgnoreCase))
                {
                    foreach (var spec in TokenSpecs)
                    {
                        if (match.Groups[spec.Item1].Success)
                        {
                            if (spec.Item1 != "SKIP")
                            {
                                _tokens.Add(new Token(spec.Item1, match.Value, lineNumber));
                            }
                            break;
                        }
                    }
                }
                lineNumber++;
            }
        }
        public List<Token> GetTokens() => _tokens;
    }
}
