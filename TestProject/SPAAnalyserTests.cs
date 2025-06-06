using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ASP_main;
using SPA_main;

namespace TestProject
{
    public class SPAAnalyzerTests
    {
        private ASTNode CreateSimpleAST()
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

        private void PrepareBasicPKB()
        {
            var pkb = PKB.GetInstance();
            pkb.SetRoot(CreateSimpleAST());
        }

        private List<Token> LexPQL(string query)
        {
            var lexer = new PQLLexer(query);
            return lexer.GetTokens();
        }

        [Fact]
        public void ExecuteQuery_SimpleSelect_ReturnsAllStatements()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; Select s";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("1", result);
            Assert.Contains("2", result);
            Assert.Contains("3", result);
            Assert.Contains("4", result);
            Assert.Contains("5", result);
            Assert.Contains("6", result);
            Assert.Contains("7", result);
            Assert.Contains("10", result);
            Assert.Contains("20", result);
        }

        [Fact]
        public void ExecuteQuery_FollowsRelation_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; Select s such that Follows(1, s)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Single(result);
            Assert.Contains("2", result);
        }

        [Fact]
        public void ExecuteQuery_ParentAndParentStarRelations_Work()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; Select s such that Parent*(3, s)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("4", result);
            Assert.Contains("5", result);
        }

        [Fact]
        public void ExecuteQuery_PatternAssignWithVarAndConstant_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; Select a pattern a(\"x\", _)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("2", result);
            Assert.Contains("7", result);
            Assert.Contains("10", result);
        }

        [Fact]
        public void ExecuteQuery_WithWithClause_IntEquality_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; Select s with s.stmt# = 2";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Single(result);
            Assert.Contains("2", result);
        }

        [Fact]
        public void ExecuteQuery_SuchThatAndPatternAndWith_AllTogether()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; Select a such that Follows(1, a) pattern a(\"x\", _) with a.stmt# = 2";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Single(result);
            Assert.Contains("2", result);
        }

        [Fact]
        public void ExecuteQuery_CallsStarRelation_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "procedure p; Select p such that Calls*(\"main\", p)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("init", result);
            Assert.Contains("helper", result);
        }

        [Fact]
        public void ExecuteQuery_PatternWithUnderscoreString_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; Select a pattern a(_, _\"3\"_)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("7", result);
        }

        [Fact]
        public void ExecuteQuery_UsesRelation_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; variable v; Select s such that Uses(s, \"y\")";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("3", result);
        }

        [Fact]
        public void ExecuteQuery_ModifiesRelation_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; variable v; Select v such that Modifies(a, v)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("x", result);
            Assert.Contains("y", result);
            Assert.Contains("z", result);
        }

        [Fact]
        public void ExecuteQuery_NextAndNextStarRelation_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; Select s such that Next(1, s)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("2", result);
        }

        [Fact]
        public void ExecuteQuery_NextStarRelation_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s; Select s such that Next*(1, s)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("2", result);
            Assert.Contains("3", result);
            Assert.Contains("4", result);
            Assert.Contains("5", result);
            Assert.Contains("6", result);
            Assert.Contains("7", result);
        }

        [Fact]
        public void ExecuteQuery_PatternWithDifferentVariable_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; Select a pattern a(\"y\", _)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Single(result);
            Assert.Contains("4", result);
        }

        [Fact]
        public void ExecuteQuery_WithClause_StringEquality_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "procedure p; Select p with p.procName = \"main\"";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Single(result);
            Assert.Contains("main", result);
        }

        [Fact]
        public void ExecuteQuery_PatternWithWildcardVar_Works()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; Select a pattern a(_, _)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            // Wszystkie assigny
            Assert.Contains("2", result);
            Assert.Contains("4", result);
            Assert.Contains("7", result);
            Assert.Contains("10", result);
            Assert.Contains("20", result);
        }

        [Fact]
        public void ExecuteQuery_SelectVarName_WithWithClause()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "variable v; Select v with v.varName = \"x\"";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Single(result);
            Assert.Contains("x", result);
        }

        [Fact]
        public void ExecuteQuery_NoResultIfNoSuchPattern()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "assign a; Select a pattern a(\"nonexistent\", _)";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("none", result);
        }

        [Fact]
        public void ExecuteQuery_WithClause_AttributeComparison()
        {
            // Arrange
            PrepareBasicPKB();
            string query = "stmt s1, s2; Select s1 such that Follows(s1, s2) with s1.stmt# = s2.stmt#";
            var tokens = LexPQL(query);
            var parser = new PQLParser(tokens);
            var queryObj = parser.ParseQuery();
            var pkb = PKB.GetInstance();
            var analyser = new SPAAnalyzer(pkb);

            // Act
            var result = analyser.Analyze(queryObj);

            // Assert
            Assert.Contains("none", result);
        }
    }
}
