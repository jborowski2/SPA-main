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
     ("SELECT", @"\bSelect\b"),
    ("SUCH_THAT", @"\bsuch that\b"),
    ("MODIFIES", @"\bModifies\b"),
    ("CALLS", @"\bCalls\b"),
    ("USES", @"\bUses\b"),
    ("WHILE", @"\bwhile\b"),
    ("IF", @"\bif\b"),
    ("PROG_LINE", @"\bprog_line\b"),
    ("PARENT_STAR", @"\bParent\*"),
    ("PARENT", @"\bParent\b(?!\*)"),
    ("FOLLOWS_STAR", @"\bFollows\*"),
    ("FOLLOWS", @"\bFollows\b(?!\*)"),
    ("FOLLOWS_STAR", @"\bFollows\*"),
    ("FOLLOWS", @"\bFollows\b(?!\*)"),
    ("NEXT", @"\bNext\b(?!\*)"),
    ("NEXT_STAR", @"\bNext\*"),
    ("PATTERN", @"\bpattern\b"),
    ("WITH", @"\bwith\b"),
    ("AND", @"\band\b"),
    ("STMT", @"\bstmt\b"),
    ("CALL", @"\bcall\b"),
    ("ASSIGN", @"\bassign\b"),
    ("VARIABLE", @"\bvariable\b"),
    ("PROCEDURE", @"\bprocedure\b"),
    ("CONSTANT", @"\bconstant\b"),
    
    // Atrybuty
    ("STMT_ATTR", @"stmt#"),
    ("VAR_ATTR", @"varName"),
     ("VAR_ATTR", @"Name"),
    ("NUM_ATTR", @"value"),
   
    // Literały i inne
    ("NUMBER", @"\d+"),
    ("NAME", @"(?!\b(if|while|select|stmt|procedure|variable|constant|assign)\b)[a-zA-Z][a-zA-Z0-9]*"),
    ("COMMA", ","),
    ("SEMICOLON", ";"),
    ("DOT", @"\."),
    ("QUOTE", "\""),
    ("LPAREN", @"\("),
    ("RPAREN", @"\)"),
    ("UNDERSCORE", "_"),
    ("EQUALS", "="),
    ("SKIP", @"[ \t\n]+"),
    ("LESS_THAN", "<"),
    ("GREATER_THAN", ">"),
    // Pattern
    ("PLUS", "\\+"),
    ("MINUS", "\\-"),
    ("MULTIPLY", "\\*"),
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
