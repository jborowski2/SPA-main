using ASP_main.Interfaces;
using ASP_main.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class Procedure
    {
        public string Name { get; set; }
        public IStatementList Body { get; set; }
        public Procedure(string name, IStatementList body)
        {
            Name = name;
            Body = body;
        }
        public Procedure(string name)
        {
            Name = name;
            Body = Factory.CreateStatementList();
        }


    }
}
