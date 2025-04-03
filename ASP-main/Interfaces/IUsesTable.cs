using ASP_main.Model;
using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IUsesTable
    {
        void SetUses(Statement statement, Variable variable);
        void SetUses(Procedure procedure, Variable variable);
        IStatementList GetUsesStatements(Variable variable);
        IProcedureList GetUsesProcedures(Variable variable);
        IVariableList GetUsedBy(Statement statement);
        IVariableList GetUsedBy(Procedure procedure);
        bool IsUses(Statement statement, Variable variable);
        bool IsUses(Procedure procedure, Variable variable);
    }
}
