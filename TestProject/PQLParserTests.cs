using ASP_main;
using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class PQLParserTests
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
        public void ParseQuery_SimpleSelect_ShouldParseCorrectly()
        {
            // Arrange
            var tokens = CreateTokens(
                ("stmt", "stmt", 1),
                ("NAME", "s", 1),
                ("SEMICOLON", ";", 1),
                ("SELECT", "Select", 2),
                ("NAME", "s", 2)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseQuery();

            // Assert
            Assert.Single(result.Declarations);
           // Assert.Equal("s", result.Selected.Name);
            Assert.Empty(result.Relations);
            Assert.Empty(result.WithClauses);
        }

        [Fact]
        public void ParseQuery_WithModifiesRelation_ShouldParseCorrectly()
        {
            // Arrange
            var tokens = CreateTokens(
                ("stmt", "stmt", 1),
                ("NAME", "s", 1),
                ("SEMICOLON", ";", 1),
                ("variable", "variable", 1),
                ("NAME", "v", 1),
                ("SEMICOLON", ";", 1),
                ("SELECT", "Select", 2),
                ("NAME", "s", 2),
                ("SUCH_THAT", "such that", 2),
                ("MODIFIES", "Modifies", 2),
                ("LPAREN", "(", 2),
                ("NAME", "s", 2),
                ("COMMA", ",", 2),
                ("NAME", "v", 2),
                ("RPAREN", ")", 2)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseQuery();

            // Assert
            Assert.Equal(2, result.Declarations.Count);
            Assert.Single(result.Relations);
            Assert.Equal("Modifies", result.Relations[0].Type);
            Assert.Equal("s", result.Relations[0].Arg1);
            Assert.Equal("v", result.Relations[0].Arg2);
        }

        [Fact]
        public void ParseDeclaration_MultipleDeclarations_ShouldParseAll()
        {
            // Arrange
            var tokens = CreateTokens(
                ("stmt", "stmt", 1),
                ("NAME", "s", 1),
                ("COMMA", ",", 1),
                ("NAME", "s1", 1),
                ("SEMICOLON", ";", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseDeclaration();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("s", result[0].Name);
            Assert.Equal("s1", result[1].Name);
            Assert.Equal("stmt", result[0].Type);
            Assert.Equal("stmt", result[1].Type);
        }

        [Fact]
        public void ParseWithClause_StmtNumberComparison_ShouldParseCorrectly()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NAME", "s", 1),
                ("DOT", ".", 1),
                ("STMT", "stmt#", 1),
                ("EQUALS", "=", 1),
                ("NUMBER", "5", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseWithClause();

            // Assert
            Assert.Equal("s", result.Left.Reference);
            Assert.Equal("stmt#", result.Left.Attribute);
            Assert.Equal("5", result.Right.Value);
            Assert.True(result.Right.IsValue);
        }

        [Fact]
        public void ParseWithClause_VarNameComparison_ShouldParseCorrectly()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NAME", "v", 1),
                ("DOT", ".", 1),
                ("VAR_ATTR", "varName", 1),
                ("EQUALS", "=", 1),
                ("QUOTE", "\"", 1),
                ("NAME", "x", 1),
                ("QUOTE", "\"", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseWithClause();

            // Assert
            Assert.Equal("v", result.Left.Reference);
            Assert.Equal("varName", result.Left.Attribute);
            Assert.Equal("x", result.Right.Value);
            Assert.True(result.Right.IsValue);
        }

        [Fact]
        public void ParseSelected_WithValidName_ShouldReturnSelected()
        {
            // Arrange
            var tokens = CreateTokens(("NAME", "s", 1));
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseSelected();

            // Assert
            //Assert.Equal("s", result.Name);
            Assert.Equal(1, parser._index);
        }


        [Fact]
        public void ParseRelation_ParentStar_ShouldParseCorrectly()
        {
            // Arrange
            var tokens = CreateTokens(
                ("PARENT_STAR", "Parent*", 1),
                ("LPAREN", "(", 1),
                ("NAME", "s", 1),
                ("COMMA", ",", 1),
                ("NAME", "s1", 1),
                ("RPAREN", ")", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseRelation();

            // Assert
            Assert.Equal("Parent*", result.Type);
            Assert.Equal("s", result.Arg1);
            Assert.Equal("s1", result.Arg2);
        }

        [Fact]
        public void ParseQuery_WithAndClause_ShouldParseBothRelations()
        {
            // Arrange
            var tokens = CreateTokens(
                ("stmt", "stmt", 1),
                ("NAME", "s", 1),
                ("SEMICOLON", ";", 1),
                ("SELECT", "Select", 2),
                ("NAME", "s", 2),
                ("SUCH_THAT", "such that", 2),
                ("MODIFIES", "Modifies", 2),
                ("LPAREN", "(", 2),
                ("NAME", "s", 2),
                ("COMMA", ",", 2),
                ("NAME", "v", 2),
                ("RPAREN", ")", 2),
                ("AND", "and", 2),
                ("USES", "Uses", 2),
                ("LPAREN", "(", 2),
                ("NAME", "s", 2),
                ("COMMA", ",", 2),
                ("NAME", "v", 2),
                ("RPAREN", ")", 2)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseQuery();

            // Assert
            Assert.Equal(2, result.Relations.Count);
            Assert.Equal("Modifies", result.Relations[0].Type);
            Assert.Equal("Uses", result.Relations[1].Type);
        }

        [Fact]
        public void ParseWithArgument_QuotedString_ShouldParseAsValue()
        {
            // Arrange
            var tokens = CreateTokens(
                ("QUOTE", "\"", 1),
                ("NAME", "test", 1),
                ("QUOTE", "\"", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseWithArgument();

            // Assert
            Assert.Equal("test", result.Value);
            Assert.True(result.IsValue);
        }

        [Fact]
        public void ParseWithArgument_Number_ShouldParseAsValue()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NUMBER", "42", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseWithArgument();

            // Assert
            Assert.Equal("42", result.Value);
            Assert.True(result.IsValue);
        }

        [Fact]
        public void ParseWithArgument_AttributeReference_ShouldParseCorrectly()
        {
            // Arrange
            var tokens = CreateTokens(
                ("NAME", "s", 1),
                ("DOT", ".", 1),
                ("STMT", "stmt#", 1)
            );
            var parser = new PQLParser(tokens);

            // Act
            var result = parser.ParseWithArgument();

            // Assert
            Assert.Equal("s", result.Reference);
            Assert.Equal("stmt#", result.Attribute);
            Assert.False(result.IsValue);
        }

        [Fact]
        public void Eat_WithInvalidToken_ShouldThrowException()
        {
            // Arrange
            var tokens = CreateTokens(
                ("SELECT", "Select", 1)
            );
            var parser = new PQLParser(tokens);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => parser.Eat("WITH"));
            Assert.Contains("Unexpected token", ex.Message);
        }
    }
}
