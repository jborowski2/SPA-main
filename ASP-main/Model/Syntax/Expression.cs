using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model.Syntax
{
    public class Expression : Factor
    {
        public Factor Left { get; set; }
        public Factor Right { get; set; }
        public Token Operation { get; set; }

        public Expression(Factor left, Token operation, Factor right)
        {
            Left = left;
            Operation = operation;
            Right = right;
        }

        public override string ToString()
        {
            return Left.ToString() + Operation.Value.ToString() + Right.ToString();
        }
    }
}
