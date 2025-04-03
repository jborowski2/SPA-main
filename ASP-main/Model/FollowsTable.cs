using ASP_main.Interfaces;
using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class FollowsTable : IFollowsTable
    {
        List<Follows> FollowsList = new List<Follows>();

        public Statement GetFollowedBy(Statement statement)
        {
            return FollowsList.Where(x => x.SecondStatement == statement).FirstOrDefault().FirstStatement;
        }

        public Statement GetFollows(Statement stmt)
        {
            throw new NotImplementedException();
        }

        public bool IsFollows(Statement firstStatement, Statement secondStatement)
        {
            throw new NotImplementedException();
        }

        public bool IsFollowsT(Statement firstStatement, Statement secondStatement)
        {
            throw new NotImplementedException();
        }

        public void SetFollows(Statement stmt1, Statement stmt2)
        {
            throw new NotImplementedException();
        }

        IStatementList GetFollowedByT(Statement statement)
        {
            throw new NotImplementedException();
        }

        IStatementList GetFollowsT(Statement statement)
        {
            throw new NotImplementedException();
        }

    }
}
