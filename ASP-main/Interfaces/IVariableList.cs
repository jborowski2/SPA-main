using ASP_main.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IVariableList : IEnumerable<Variable>
    {
        Variable this[int i] { get; }
        int AddVariable(Variable variable);
        Variable GetVariableByIndex(int index);
        Variable GetVariableByName(string name);
        int GetIndex(Variable variable);
        int GetIndexByName(string name);
        int GetSize();
        bool Contains(Variable variable);
        bool Contains(string name);
        IVariableList Intersection(IVariableList otherVariableList);
    }
}
