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
            var program = new ASTNode("program");
            var procedure = new ASTNode("procedure", "main");
            program.AddChild(procedure);

            var stmtList = new ASTNode("stmtList");
            procedure.AddChild(stmtList);

            // Stmt 1: assign x = 5
            var assign1 = new ASTNode("assign", "x", 1);
            stmtList.AddChild(assign1);
            var const1 = new ASTNode("const", "5");
            assign1.AddChild(const1);

            // Stmt 2: while y
            var whileNode = new ASTNode("while", "y", 2);
            stmtList.AddChild(whileNode);

            var whileBody = new ASTNode("stmtList");
            whileNode.AddChild(whileBody);

            // Stmt 3: assign z = 1 (inside while)
            var assign2 = new ASTNode("assign", "z", 3);
            whileBody.AddChild(assign2);
            var const2 = new ASTNode("const", "1");
            assign2.AddChild(const2);

            // Stmt 4: assign y = 0 (inside while)
            var assign3 = new ASTNode("assign", "y", 4);
            whileBody.AddChild(assign3);
            var const3 = new ASTNode("const", "0");
            assign3.AddChild(const3);

            // Stmt 5: assign x = 10 (after while)
            var assign4 = new ASTNode("assign", "x", 5);
            stmtList.AddChild(assign4);
            var const4 = new ASTNode("const", "10");
            assign4.AddChild(const4);

            // Set follows relationships
            assign1.SetFollows(whileNode);
            assign2.SetFollows(assign3);
            whileNode.SetFollows(assign4);

            return program;
        }
    }
}
        //[Fact]
        //    public void Analyze_ModifiesX_ReturnsCorrectStatements()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("assign", "a"));
        //        query.Selected = new Selected("a");
        //        query.Relations.Add(new Relation("Modifies", "a", "x"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Equal(2, results.Count);
        //        Assert.Contains("1", results); // x = 5
        //        Assert.Contains("5", results); // x = 10
        //    }

        //    [Fact]
        //    public void Analyze_UsesY_ReturnsWhileStatement()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Uses", "s", "y"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("2", results[0]); // while(y)
        //    }

        //    [Fact]
        //    public void Analyze_ParentOfWhileBody_ReturnsInnerStatements()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Parent", "2", "s"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Equal(2, results.Count);
        //        Assert.Contains("3", results); // z = 1
        //        Assert.Contains("4", results); // y = 0
        //    }

        //    [Fact]
        //    public void Analyze_FollowsRelations_ReturnsCorrectNextStatements()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Follows", "1", "s"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("2", results[0]); // while po assign
        //    }

        //    [Fact]
        //    public void Analyze_FollowsInWhileBody_ReturnsCorrectOrder()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Follows", "3", "s"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("4", results[0]); // y = 0 po z = 1
        //    }

        //    [Fact]
        //    public void Analyze_ModifiesWithParent_ReturnsNestedAssignments()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("assign", "a"));
        //        query.Declarations.Add(new Declaration("while", "w"));
        //        query.Selected = new Selected("a");
        //        query.Relations.Add(new Relation("Parent", "w", "a"));
        //        query.Relations.Add(new Relation("Modifies", "a", "y"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("4", results[0]); // y = 0
        //    }

        //    [Fact]
        //    public void Analyze_NonexistentVariable_ReturnsEmpty()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Modifies", "s", "nonexistent"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("None", results[0]);
        //    }

        //    [Fact]
        //    public void Analyze_BooleanQueryForExistingModifies_ReturnsTrue()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("BOOLEAN");
        //        query.Relations.Add(new Relation("Modifies", "1", "x"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("True", results[0]);
        //    }

        //    [Fact]
        //    public void Analyze_BooleanQueryForInvalidModifies_ReturnsFalse()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("BOOLEAN");
        //        query.Relations.Add(new Relation("Modifies", "2", "x"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("False", results[0]);
        //    }

        //    [Fact]
        //    public void Analyze_BooleanQueryForExistingFollows_ReturnsTrue()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("BOOLEAN");
        //        query.Relations.Add(new Relation("Follows", "1", "2"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("True", results[0]);
        //    }

        //    [Fact]
        //    public void Analyze_BooleanQueryForInvalidFollows_ReturnsFalse()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("BOOLEAN");
        //        query.Relations.Add(new Relation("Follows", "5", "1"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("False", results[0]);
        //    }

        //    [Fact]
        //    public void Analyze_ParentStar_ReturnsAllNestedStatements()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Parent*", "2", "s"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Equal(2, results.Count);
        //        Assert.Contains("3", results); // z = 1
        //        Assert.Contains("4", results); // y = 0
        //    }

        //    [Fact]
        //    public void Analyze_FollowsStar_ReturnsAllFollowingInScope()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("s");
        //        query.Relations.Add(new Relation("Follows*", "1", "s"));

        //        var results = analyzer.Analyze(query);

        //        Assert.Equal(2, results.Count);
        //        Assert.Contains("2", results); // while(y)
        //        Assert.Contains("5", results); // x = 10
        //    }

        //    [Fact]
        //    public void Analyze_BooleanUses_ReturnsCorrectTruthValue()
        //    {
        //        var ast = CreateSimpleAST();
        //        var analyzer = new SPAAnalyzer(ast);

        //        var query = new PQLQuery();
        //        query.Declarations.Add(new Declaration("stmt", "s"));
        //        query.Selected = new Selected("BOOLEAN");
        //        query.Relations.Add(new Relation("Uses", "2", "y")); // while używa y

        //        var results = analyzer.Analyze(query);

        //        Assert.Single(results);
        //        Assert.Equal("True", results[0]);
        //    }
        //}
     
