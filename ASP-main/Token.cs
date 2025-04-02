using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main
{
    public class Token
    {
        public string Type { get; }
        public string Value { get; }

        public Token(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"Token({Type}, {Value})";
    }

}
