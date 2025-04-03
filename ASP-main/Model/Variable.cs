using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class Variable : Factor
    {
        public string Name { get; set; }

        public Token Token { get; set; }

        public Variable(string name)
        {
            Name = name;
        }

        public Variable(Token token)
        {
            Name = token.Value.ToString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
