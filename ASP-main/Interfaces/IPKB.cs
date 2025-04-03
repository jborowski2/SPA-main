using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IPKB
    {
        IVariableList Variables { get; }
        IStatementList Statements { get; }
        IFollowsTable FollowsTable { get; }
        IParentTable ParentTable { get; }
        IModifiesTable ModifiesTable { get; }
        IUsesTable UsesTable { get; }
        void LoadData(string code);
    }
}
