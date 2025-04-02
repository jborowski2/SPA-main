using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA_main
{
    public class Token
    {
        public string Type { get; }
        public string Value { get; }
        public int LineNumber { get; }

        public Token(string type, string value, int lineNumber)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
        }

        public override string ToString() => $"Token({Type}, {Value}, Line {LineNumber})";
    }

}
