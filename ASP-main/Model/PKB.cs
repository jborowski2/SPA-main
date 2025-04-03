using ASP_main.Interfaces;
using ASP_main.Service;
using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class PKB : IPKB
    {
        private static PKB instance;
        private PKB() { }
        public static PKB GetInstance()
        {
            if (instance == null)
                instance = new PKB();
            return instance;
        }

        public IVariableList Variables { get; set; }
        public IStatementList Statements { get; set; }
        public IFollowsTable FollowsTable { get; set; }
        public IParentTable ParentTable { get; set; }
        public IModifiesTable ModifiesTable { get; set; }
        public IUsesTable UsesTable { get; set; }

        public void LoadData(string programCode)
        {
            Lexer lexer = new Lexer(programCode);
            Parser parser = new Parser(lexer.GetTokens());
            ASTNode root = parser.ParseProgram(); //Generowanie drzewa
            IDesignExtractor designExtractor = Factory.CreateDesignExtractor();
            designExtractor.ExtractData(root);

            Variables = designExtractor.Variables;
            Statements = designExtractor.Statements;
            FollowsTable = designExtractor.FollowsTable;
            ParentTable = designExtractor.ParentTable;
            ModifiesTable = designExtractor.ModifiesTable;
            UsesTable = designExtractor.UsesTable;
        }
    }
}
