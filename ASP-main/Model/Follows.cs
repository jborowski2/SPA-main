using ASP_main.Interfaces;
using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class Follows
    {
        public Statement FirstStatement { get; set; }
        public Statement SecondStatement { get; set; }
        public Follows(Statement s1, Statement s2)
        {
            FirstStatement = s1;
            SecondStatement = s2;
        }

    }
}
