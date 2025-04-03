using ASP_main.Model;
using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IModifiesTable
    {
        void SetModifies(Statement statement, Variable variable);
        IStatementList GetModifies(Variable variable);
        IVariableList GetModifiedBy(Statement statement);
        bool IsModified(Statement statement, Variable variable);
    }
}
