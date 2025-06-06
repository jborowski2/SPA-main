using System;
using System.Collections.Generic;
using Xunit;
using SPA_main;
using ASP_main;

namespace TestProject
{
    public class PKBTests
    {
        public ASTNode BuildSampleAST()
        {
            // procedure init { x = 0; }
            var procInit = new ASTNode("procedure", "init");
            procInit.ProcName = "init";
            var stmtLstInit = new ASTNode("stmtLst") { ProcName = "init" };
            var assignInit = new ASTNode("assign", "x", 10);
            assignInit.AddChild(new ASTNode("const", "0"));
            stmtLstInit.AddChild(assignInit);
            procInit.AddChild(stmtLstInit);

            // procedure helper { z = 5; }
            var procHelper = new ASTNode("procedure", "helper");
            procHelper.ProcName = "helper";
            var stmtLstHelper = new ASTNode("stmtLst") { ProcName = "helper" };
            var assignHelper = new ASTNode("assign", "z", 20);
            assignHelper.AddChild(new ASTNode("const", "5"));
            stmtLstHelper.AddChild(assignHelper);
            procHelper.AddChild(stmtLstHelper);

            // procedure main { call init; x = 1; if ... while ... }
            var procMain = new ASTNode("procedure", "main");
            procMain.ProcName = "main";
            var stmtLstMain = new ASTNode("stmtLst") { ProcName = "main" };

            var callInit = new ASTNode("call", "init", 1);
            var assign1 = new ASTNode("assign", "x", 2);
            assign1.AddChild(new ASTNode("const", "1"));
            var ifNode = new ASTNode("if", "y", 3);

            // THEN: y = 2;
            var thenLst = new ASTNode("stmtLst") { ProcName = "main" };
            var assignThen = new ASTNode("assign", "y", 4);
            assignThen.AddChild(new ASTNode("const", "2"));
            thenLst.AddChild(assignThen);

            // ELSE: call helper;
            var elseLst = new ASTNode("stmtLst") { ProcName = "main" };
            var callHelper = new ASTNode("call", "helper", 5);
            elseLst.AddChild(callHelper);

            ifNode.AddChild(thenLst);
            ifNode.AddChild(elseLst);

            var whileNode = new ASTNode("while", "z", 6);
            var whileLst = new ASTNode("stmtLst") { ProcName = "main" };
            var assignWhile = new ASTNode("assign", "x", 7);
            assignWhile.AddChild(new ASTNode("const", "3"));
            whileLst.AddChild(assignWhile);
            whileNode.AddChild(whileLst);

            // relacje follows
            callInit.SetFollows(assign1);
            assign1.SetFollows(ifNode);
            ifNode.SetFollows(whileNode);

            stmtLstMain.AddChild(callInit);
            stmtLstMain.AddChild(assign1);
            stmtLstMain.AddChild(ifNode);
            stmtLstMain.AddChild(whileNode);
            procMain.AddChild(stmtLstMain);

            // ROOT = program
            var root = new ASTNode("program");
            root.AddChild(procMain);
            root.AddChild(procHelper);
            root.AddChild(procInit);
            return root;
        }


        [Fact]
        public void SetRoot_PopulatesAllRelations()
        {
            // Arrange
            var pkb = PKB.GetInstance();
            pkb.SetRoot(BuildSampleAST());

            // Act & Assert

            //--Weryfikacja indeksowania wezlow
            Assert.NotNull(pkb.GetNodeByLine(1)); // call init
            Assert.NotNull(pkb.GetNodeByLine(2)); // x = 1
            Assert.NotNull(pkb.GetNodeByLine(3)); // if y
            Assert.NotNull(pkb.GetNodeByLine(4)); // y = 2
            Assert.NotNull(pkb.GetNodeByLine(5)); // call helper
            Assert.NotNull(pkb.GetNodeByLine(6)); // while z
            Assert.NotNull(pkb.GetNodeByLine(7)); // x = 3
            Assert.NotNull(pkb.GetNodeByLine(10)); // x = 0 (init)
            Assert.NotNull(pkb.GetNodeByLine(20)); // z = 5 (helper)

            //--Follows
            Assert.Equal("2", pkb.Follows["1"]);
            Assert.Equal("3", pkb.Follows["2"]);
            Assert.Equal("6", pkb.Follows["3"]);

            //--Parent i ParentStar
            pkb.ComputeParentStar();
            Assert.Contains(("3", "4"), pkb.IsParent); // if->then
            Assert.Contains(("3", "5"), pkb.IsParent); // if->else
            Assert.Contains(("6", "7"), pkb.IsParent); // while->body

            //--Calls i CallsStar
            Assert.Contains(("main", "init"), pkb.IsCalls);
            Assert.Contains(("main", "helper"), pkb.IsCallsStar);
            Assert.Contains(("main", "helper"), pkb.IsCalls);

            //--Modifies/Uses
            Assert.Contains("x", pkb.ModifiesStmt["2"]);
            Assert.Contains("y", pkb.ModifiesStmt["4"]);
            Assert.Contains("x", pkb.ModifiesStmt["7"]);
            Assert.Contains("z", pkb.ModifiesStmt["20"]);
            Assert.Contains("x", pkb.ModifiesStmt["10"]);
            Assert.Contains("helper", pkb.Procedures);
            Assert.Contains("init", pkb.Procedures);
            Assert.Contains("main", pkb.Procedures);

            //--IF/WHILE uses warunek
            Assert.Contains("y", pkb.UsesStmt["3"]);
            Assert.Contains("z", pkb.UsesStmt["6"]);

            //--Next & NextStar
            Assert.Contains("2", pkb.Next["1"]);
            Assert.Contains("3", pkb.Next["2"]);
            Assert.Contains("4", pkb.Next["3"]);
            Assert.Contains("5", pkb.Next["3"]);
            Assert.Contains("6", pkb.Next["4"]);
            Assert.Contains("6", pkb.Next["5"]);
            pkb.ComputeNextStar();
            Assert.Contains(("1", "6"), pkb.IsNextStar);

            //--Assign
            Assert.Contains("2", pkb.Assign);
            Assert.Contains("4", pkb.Assign);
            Assert.Contains("7", pkb.Assign);
            Assert.Contains("10", pkb.Assign);
            Assert.Contains("20", pkb.Assign);
        }
    }
}
