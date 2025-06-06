using Xunit;
using SPA_main;
using System.Linq;

namespace TestProject
{
    public class LexerTests
    {
        [Fact]
        public void Tokenize_SimpleProcedure_CreatesCorrectTokens()
        {
            // Arrange
            string code = "procedure main { x = y + 1; }";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal(10, tokens.Count);
            Assert.Equal("PROCEDURE", tokens[0].Type);
            Assert.Equal("main", tokens[1].Value);
            Assert.Equal("LBRACE", tokens[2].Type);
            Assert.Equal("NAME", tokens[3].Type);
            Assert.Equal("x", tokens[3].Value);
            Assert.Equal("ASSIGN", tokens[4].Type);
            Assert.Equal("NAME", tokens[5].Type);
            Assert.Equal("y", tokens[5].Value);
            Assert.Equal("PLUS", tokens[6].Type);
            Assert.Equal("NUMBER", tokens[7].Type);
            Assert.Equal("1", tokens[7].Value);
            Assert.Equal("SEMICOLON", tokens[8].Type);
            Assert.Equal("RBRACE", tokens[9].Type);
        }

        [Fact]
        public void Tokenize_ShouldIgnoreEmptyLines()
        {
            // Arrange
            string code = "procedure main {\n\n\nx = 1;\n\n}";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Contains(tokens, t => t.Type == "NAME" && t.Value == "x");
            Assert.DoesNotContain(tokens, t => string.IsNullOrWhiteSpace(t.Value));
        }

        [Fact]
        public void Tokenize_HandlesLineNumbersCorrectly()
        {
            // Arrange
            string code = "procedure main {\n x = 1;\n y = 2;\n}";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal(1, tokens[0].LineNumber); // procedure
            Assert.Equal(1, tokens[1].LineNumber); // main
            Assert.Equal(1, tokens[2].LineNumber); // {
            Assert.Equal(1, tokens[3].LineNumber); // x
            Assert.Equal(1, tokens[4].LineNumber); // =
            Assert.Equal(1, tokens[5].LineNumber); // 1
            Assert.Equal(1, tokens[6].LineNumber); // ;
            Assert.Equal(2, tokens[7].LineNumber); // y
            Assert.Equal(2, tokens[8].LineNumber); // =
            Assert.Equal(2, tokens[9].LineNumber); // 2
            Assert.Equal(2, tokens[10].LineNumber); // ;
            Assert.Equal(3, tokens[11].LineNumber); // }
        }

        [Fact]
        public void Tokenize_IgnoresWhitespaceAndTabs()
        {
            // Arrange
            string code = " \t procedure\tmain  \n\t{\nx=1;\t}";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.All(tokens, t => Assert.NotEqual("SKIP", t.Type));
            Assert.Equal("PROCEDURE", tokens[0].Type);
            Assert.Equal("main", tokens[1].Value);
            Assert.Equal("LBRACE", tokens[2].Type);
            Assert.Equal("NAME", tokens[3].Type);
            Assert.Equal("x", tokens[3].Value);
            Assert.Equal("ASSIGN", tokens[4].Type);
            Assert.Equal("NUMBER", tokens[5].Type);
            Assert.Equal("1", tokens[5].Value);
            Assert.Equal("SEMICOLON", tokens[6].Type);
            Assert.Equal("RBRACE", tokens[7].Type);
        }

        [Fact]
        public void Tokenize_AllTokenTypes_AreRecognized()
        {
            // Arrange
            string code = "procedure foo { while (x) { if y then { z = 3 + 5; } else { call proc2; } } }";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();
            var types = new HashSet<string>(tokens.ConvertAll(t => t.Type));

            // Assert
            Assert.Contains("PROCEDURE", types);
            Assert.Contains("WHILE", types);
            Assert.Contains("LPAREN", types);
            Assert.Contains("NAME", types);
            Assert.Contains("IF", types);
            Assert.Contains("THEN", types);
            Assert.Contains("LBRACE", types);
            Assert.Contains("ASSIGN", types);
            Assert.Contains("NUMBER", types);
            Assert.Contains("PLUS", types);
            Assert.Contains("CALL", types);
            Assert.Contains("SEMICOLON", types);
            Assert.Contains("RBRACE", types);
            Assert.Contains("ELSE", types);
        }

        [Fact]
        public void Tokenize_ProcedureWithElseBraces_TreatedCorrectly()
        {
            // Arrange
            string code = "procedure main { if x then { y = 1; } else { z = 2; } }";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Contains(tokens, t => t.Type == "ELSE");
            Assert.Contains(tokens, t => t.Type == "LBRACE");
            Assert.Contains(tokens, t => t.Type == "RBRACE");
        }

        [Fact]
        public void GetTokens_ReturnsEmptyListForEmptyInput()
        {
            // Arrange
            string code = "   \n\t";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Empty(tokens);
        }

        [Fact]
        public void Tokenize_ShouldTokenizeMultipleStatementsOnOneLine()
        {
            // Arrange
            string code = "procedure main { x = 1; y = 2; }";
            var lexer = new Lexer(code);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Contains(tokens, t => t.Type == "NAME" && t.Value == "x");
            Assert.Contains(tokens, t => t.Type == "NAME" && t.Value == "y");
            Assert.Contains(tokens, t => t.Type == "NUMBER" && t.Value == "1");
            Assert.Contains(tokens, t => t.Type == "NUMBER" && t.Value == "2");
        }
    }
}