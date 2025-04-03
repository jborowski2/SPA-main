using ASP_main.Interfaces;
using ASP_main.Model.Syntax;
using ASP_main.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class While : Statement
    {
        public Variable Condition { get; set; }
        public IStatementList Body { get; set; }

        public While(int programLine) : base(programLine)
        {
            Body = Factory.CreateStatementList();
        }
        public While(Variable condition, IStatementList body, int programLine) : base(programLine)
        {
            Condition = condition;
            Body = body;
        }
        public override string ToString()
        {
            return base.ToString() + " while " + Condition.ToString() + " then ";
        }
    }
}
