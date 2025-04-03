using ASP_main.Interfaces;
using ASP_main.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model.Syntax
{
    public class Statement
    {
        public int ProgramLine { get; set; }
        public Statement(int programLine)
        {
            ProgramLine = programLine;
        }
    }
}
