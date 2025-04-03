using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IFollowsTable
    {
        void SetFollows(Statement firstStatement, Statement secondStatement);
        Statement GetFollows(Statement statement);
        IStatementList GetFollowsT(Statement statement);
        Statement GetFollowedBy(Statement statement);
        IStatementList GetFollowedByT(Statement statement);
        bool IsFollows(Statement firstStatement, Statement secondStatement);
        bool IsFollowsT(Statement firstStatement, Statement secondStatement);
    }
}
