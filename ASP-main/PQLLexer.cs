using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASP_main
{
    public class PQLLexer
    {
        private static readonly (string, string)[] TokenSpecs =
 {
    // Słowa kluczowe - dopasowywane jako całe słowa
   ("PARENT_STAR", @"\bParent\*"),
    ("FOLLOWS_STAR", @"\bFollows\*"),
    ("CALLS_STAR", @"\bCalls\*"),

    // Następnie podstawowe wersje bez *
    ("PARENT", @"\bParent\b"),
    ("FOLLOWS", @"\bFollows\b"),
    ("CALLS", @"\bCalls\b"),

    // Słowa kluczowe - case sensitive
    ("SELECT", @"\bSelect\b"),
    ("SUCH_THAT", @"\bsuch\s+that\b"),  // Uwzględnia spację między słowami
    ("MODIFIES", @"\bModifies\b"),
    ("USES", @"\bUses\b"),
    ("PATTERN", @"\bpattern\b"),
    ("WITH", @"\bwith\b"),
    ("AND", @"\bAND\b"),  // Zakładam, że "And" jest case-sensitive (wielka litera)
    ("WHILE", @"\bwhile\b"),
    ("IF", @"\bif\b"),
    ("PROG_LINE", @"\bprog_line\b"),

    // Typy encji
    ("STMT", @"\bstmt\b"),
    ("ASSIGN", @"\bassign\b"),
    ("VARIABLE", @"\bvariable\b"),
    ("PROCEDURE", @"\bprocedure\b"),
    ("CONSTANT", @"\bconstant\b"),

    // Atrybuty
    ("STMT_ATTR", @"stmt#"),
    ("VAR_ATTR", @"varName"),
    ("PROC_ATTR", @"procName"),  // Dodane dla spójności
    ("VALUE_ATTR", @"value"),  // Spójność nazewnictwa

    // Literały i symbole
    ("NUMBER", @"\d+"),
    ("NAME", @"[a-zA-Z][a-zA-Z0-9]*"),  // Uproszczone, zakładając że keywords już są złapane
    ("COMMA", ","),
    ("SEMICOLON", ";"),
    ("DOT", @"\."),
    ("QUOTE", "\""),
    ("LPAREN", @"\("),
    ("RPAREN", @"\)"),
    ("UNDERSCORE", "_"),
    ("EQUALS", "="),
    ("LT", "<"),  // Dodane na wypadek
    ("GT", ">"),  // Dodane na wypadek

    // Białe znaki - pomijane
    ("SKIP", @"\s+"),

    // Domyślny token dla nieznanych znaków
    ("UNKNOWN", @".")  // Zawsze ostatni
};
        private readonly string _query;
        private readonly List<Token> _tokens = new List<Token>();

        public PQLLexer(string query)
        {
            _query = query;
            Tokenize();
           // PrintTokens();
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
