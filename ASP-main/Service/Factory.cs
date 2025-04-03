using ASP_main.Interfaces;
using ASP_main.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Service
{
    //Nazwa robocza - zmienić później jak wpadnę na pomysł
    public class Factory
    {
        public static IStatementList CreateStatementList()
        {
            //TODO:
            return null;
        }
        public static IPKB CreateProgramKnowledgeBase()
        {
            return PKB.GetInstance();
        }

        public static IDesignExtractor CreateDesignExtractor()
        {
            return new DesignExtractor();
        }

        public static IFollowsTable CreateFollowsTable()
        {
            return new FollowsTable();
        }
        public static IVariableList CreateVariableList()
        {
            return null;
        }
        public static IModifiesTable CreateModifiesTable()
        {
            return null;
            //TODO:
            //return new ModifiesTable();
        }
        public static IParentTable CreateParentTable()
        {
            return null;
            //TODO:
            //return new ParentTable();
        }
        public static IUsesTable CreateUsesTable()
        {
            return null;
            //TODO:
            //return new UsesTable();
        }
    }
}
