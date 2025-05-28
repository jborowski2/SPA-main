using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPA_main
{
    public class Lexer
    {
        private static readonly (string, string)[] TokenSpecs =
        {
        ("PROCEDURE", "procedure"),
        ("WHILE", "while"),
        ("IF", "if"),
        ("THEN", "then"),
        ("ELSE", "else"),
        ("CALL", "call"),
        ("NAME", "[a-zA-Z_][a-zA-Z0-9#_]*"),
        ("NUMBER", "\\d+"),
        ("ASSIGN", "="),
        ("PLUS", "\\+"),
        ("MINUS", "\\-"),
        ("MULTIPLY", "\\*"),
        ("LBRACE", "\\{"),
        ("RBRACE", "\\}"),
        ("LPAREN", "\\("),
        ("RPAREN", "\\)"),
        ("SEMICOLON", ";"),
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
            string[] allLines = _code.Split('\n');
            int lineNumber = 1;

            for (int i = 0; i < allLines.Length; i++)
            {
                string line = allLines[i].Trim();

                // Całkowicie pomijamy puste linie
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Sprawdzamy czy to linia z deklaracją procedury
                bool isProcedureLine = line.StartsWith("procedure") && line.EndsWith("{") ||
                           (line == "else{") ||
                           (line == "else {");

                // Tokenizujemy zawartość linii
                foreach (Match match in Regex.Matches(line, pattern))
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

                // Inkrementujemy numer linii tylko jeśli to nie jest linia z procedure
                // i nie jest to pusta linia (już sprawdzone na początku)

                //z tym ifem nie przechodzi testów jednostkowych, samo lineNumber++ przechodzi
                if (!isProcedureLine)
                {
                    lineNumber++;
                }
            }
        }

        public List<Token> GetTokens() => _tokens;
    }
}
