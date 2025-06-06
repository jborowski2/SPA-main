using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class ParserTests
    {
        private List<Token> CreateTokens(params (string type, string value, int line)[] tokens)
        {
            var result = new List<Token>();
            foreach (var token in tokens)
            {
                result.Add(new Token(token.type, token.value, token.line));
            }
            return result;
        }

        [Fact]
        public void ParseProgram_EmptyProgram_ShouldReturnEmptyProgramNode()
        {
            // Arrange
            var tokens = new List<Token>();
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseProgram();

            // Assert
            Assert.Equal("program", result.Type);
            Assert.Empty(result.Children);
        }

        [Fact]
        public void ParseProcedure_SimpleProcedure_ShouldCreateCorrectAST()
        {
            // Arrange
            var tokens = CreateTokens(
                ("PROCEDURE", "procedure", 1),
                ("NAME", "main", 1),
                ("LBRACE", "{", 1),
                ("RBRACE", "}", 1)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseProcedure();

            // Assert
            Assert.Equal("procedure", result.Type);
            Assert.Equal("main", result.Value);
            Assert.Single(result.Children);
            Assert.Equal("stmtLst", result.Children[0].Type);
        }

        [Fact]
        public void ParseStmtLst_WithAssignments_ShouldSetFollowsRelations()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NAME", "x", 1),
                ("ASSIGN", "=", 1),
                ("NUMBER", "1", 1),
                ("SEMICOLON", ";", 1),
                ("NAME", "y", 2),
                ("ASSIGN", "=", 2),
                ("NUMBER", "2", 2),
                ("SEMICOLON", ";", 2),
                ("RBRACE", "}", 3)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseStmtLst();

            // Assert
            Assert.Equal(2, result.Children.Count);
            Assert.Equal(result.Children[0].Follows, result.Children[1]);
            Assert.Equal(result.Children[1].FollowedBy, result.Children[0]);
        }

        [Fact]
        public void ParseIf_ShouldCreateCorrectIfNodeStructure()
        {
            // Arrange
            var tokens = CreateTokens(
                ("IF", "if", 1),
                ("NAME", "condition", 1),
                ("THEN", "then", 1),
                ("LBRACE", "{", 1),
                ("NAME", "x", 2),
                ("ASSIGN", "=", 2),
                ("NUMBER", "1", 2),
                ("SEMICOLON", ";", 2),
                ("RBRACE", "}", 3),
                ("ELSE", "else", 3),
                ("LBRACE", "{", 3),
                ("NAME", "y", 4),
                ("ASSIGN", "=", 4),
                ("NUMBER", "2", 4),
                ("SEMICOLON", ";", 4),
                ("RBRACE", "}", 5)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseIf();

            // Assert
            Assert.Equal("if", result.Type);
            Assert.Equal("condition", result.Value);
            Assert.Equal(2, result.Children.Count);
            Assert.Equal("stmtLst", result.Children[0].Type);
            Assert.Equal("stmtLst", result.Children[1].Type);
        }

        [Fact]
        public void ParseWhile_ShouldCreateCorrectWhileNodeStructure()
        {
            // Arrange
            var tokens = CreateTokens(
                ("WHILE", "while", 1),
                ("NAME", "condition", 1),
                ("LBRACE", "{", 1),
                ("NAME", "x", 2),
                ("ASSIGN", "=", 2),
                ("NUMBER", "1", 2),
                ("SEMICOLON", ";", 2),
                ("RBRACE", "}", 3)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseWhile();

            // Assert
            Assert.Equal("while", result.Type);
            Assert.Equal("condition", result.Value);
            Assert.Single(result.Children);
            Assert.Equal("stmtLst", result.Children[0].Type);
        }

        [Fact]
        public void ParseAssign_SimpleAssignment_ShouldCreateCorrectNode()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NAME", "x", 1),
                ("ASSIGN", "=", 1),
                ("NUMBER", "5", 1),
                ("SEMICOLON", ";", 1)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseAssign();

            // Assert
            Assert.Equal("assign", result.Type);
            Assert.Equal("x", result.Value);
            Assert.Single(result.Children);
            Assert.Equal("const", result.Children[0].Type);
        }

        [Fact]
        public void ParseExpr_ComplexExpression_ShouldCreateCorrectTree()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NAME", "x", 1),
                ("PLUS", "+", 1),
                ("NUMBER", "2", 1),
                ("MULTIPLY", "*", 1),
                ("NAME", "y", 1),
                ("SEMICOLON", ";", 1)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseExpr();

            // Assert
            Assert.Equal("plus_op", result.Type);
            Assert.Equal("PLUS", result.Value);
            Assert.Equal(2, result.Children.Count);
            Assert.Equal("var", result.Children[0].Type);
            Assert.Equal("x", result.Children[0].Value);
            Assert.Equal("multiply_op", result.Children[1].Type);
            Assert.Equal("*", result.Children[1].Value);
            Assert.Equal(2, result.Children[1].Children.Count);
            Assert.Equal("const", result.Children[1].Children[0].Type);
            Assert.Equal("2", result.Children[1].Children[0].Value);
            Assert.Equal("var", result.Children[1].Children[1].Type);
            Assert.Equal("y", result.Children[1].Children[1].Value);
        }

        [Fact]
        public void ParseCall_ShouldCreateCorrectCallNode()
        {
            // Arrange
            var tokens = CreateTokens(
                ("CALL", "call", 1),
                ("NAME", "procedureName", 1),
                ("SEMICOLON", ";", 1)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseCall();

            // Assert
            Assert.Equal("call", result.Type);
            Assert.Equal("procedureName", result.Value);
            Assert.Equal(1, result.LineNumber);
        }

        [Fact]
        public void Eat_WithInvalidToken_ShouldThrowException()
        {
            // Arrange
            var tokens = CreateTokens(
                ("PROCEDURE", "procedure", 1),
                ("NAME", "main", 1)
            );
            var parser = new Parser(tokens);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => parser.Eat("LBRACE"));
            Assert.Contains("Unexpected token", ex.Message);
        }

        [Fact]
        public void ParseFactor_WithParentheses_ShouldHandleNestedExpressions()
        {
            // Arrange
            var tokens = CreateTokens(
                ("LPAREN", "(", 1),
                ("NUMBER", "5", 1),
                ("PLUS", "+", 1),
                ("NAME", "x", 1),
                ("RPAREN", ")", 1),
                ("SEMICOLON", ";", 1)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseFactor();

            // Assert
            Assert.Equal("plus_op", result.Type);
            Assert.Equal(2, result.Children.Count);
        }

        [Fact]
        public void ParseProgram_WithMultipleProcedures_ShouldCreateHierarchy()
        {
            // Arrange
            var tokens = CreateTokens(
                ("PROCEDURE", "procedure", 1),
                ("NAME", "main", 1),
                ("LBRACE", "{", 1),
                ("RBRACE", "}", 1),
                ("PROCEDURE", "procedure", 2),
                ("NAME", "helper", 2),
                ("LBRACE", "{", 2),
                ("RBRACE", "}", 2)
            );
            var parser = new Parser(tokens);

            // Act
            var result = parser.ParseProgram();

            // Assert
            Assert.Equal("program", result.Type);
            Assert.Equal(2, result.Children.Count);
            Assert.Equal("procedure", result.Children[0].Type);
            Assert.Equal("procedure", result.Children[1].Type);
        }
    }
}
