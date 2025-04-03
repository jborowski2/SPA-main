using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IParentTable
    {
        void SetParent(Statement firstStatement, Statement secondStatement);
        Statement GetParent(Statement statement);
        IStatementList GetParentT(Statement statement);
        IStatementList GetParentedBy(Statement statement);
        IStatementList GetParentedByT(Statement statement);
        bool IsParent(Statement firstStatement, Statement secondStatement);
        bool IsParentT(Statement firstStatement, Statement secondStatement);
    }
}
