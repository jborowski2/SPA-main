using ASP_main.Model.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Interfaces
{
    public interface IStatementList
    {
        Statement this[int i] { get; }
        int AddStatement(Statement statement);
        Statement GetStatementByIndex(int index);
        Statement GetStatementByProgramLine(int programLine);
        int GetIndex(Statement statement);
        int GetIndexByProgramLine(int programLine);
        int GetSize();

    }
}
