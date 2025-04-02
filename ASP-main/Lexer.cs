using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPA_main
{
    class Lexer
    {
        private static readonly (string, string)[] TokenSpecs =
        {
        ("PROCEDURE", "procedure"),
        ("WHILE", "while"),
        ("NAME", "[a-zA-Z][a-zA-Z0-9#]*"),
        ("NUMBER", "\\d+"),
        ("ASSIGN", "="),
        ("PLUS", "\\+"),
        ("LBRACE", "\\{"),
        ("RBRACE", "\\}"),
        ("SEMICOLON", ";"),
        ("SKIP", "[ \\t\\n]+")
    };

        private readonly string _code;
        private readonly List<Token> _tokens = new List<Token>();

        public Lexer(string code)
        {
            _code = code;
            Tokenize();
        }

        private void Tokenize()
        {
            string pattern = string.Join("|", TokenSpecs.Select(spec => $"(?<{spec.Item1}>{spec.Item2})"));
            int lineNumber = 0;
            int positionInLine = 0;

            foreach (Match match in Regex.Matches(_code, pattern))
            {
                // Oblicz numer linii na podstawie znaków nowej linii w dopasowanym tekście
                var newlineCount = match.Value.Count(c => c == '\n');
                if (newlineCount > 0)
                {
                    lineNumber += newlineCount;
                    positionInLine = match.Value.Length - match.Value.LastIndexOf('\n') - 1;
                }
                else
                {
                    positionInLine += match.Value.Length;
                }

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
        }

        public List<Token> GetTokens() => _tokens;
    }
}
