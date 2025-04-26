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
        public void Tokenize_IgnoresWhitespace()
        {
            // Arrange
            string code = "  procedure   main \n { \t x = 1; }";
            var lexer = new Lexer(code);
            
            // Act
            var tokens = lexer.GetTokens();
            
            // Assert
            Assert.Equal(8, tokens.Count);
            Assert.All(tokens, t => Assert.NotEqual("SKIP", t.Type));
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
            Assert.Equal(2, tokens[3].LineNumber); // x
            Assert.Equal(2, tokens[4].LineNumber); // =
            Assert.Equal(2, tokens[5].LineNumber); // 1
            Assert.Equal(2, tokens[6].LineNumber); // ;
            Assert.Equal(3, tokens[7].LineNumber); // y
            Assert.Equal(3, tokens[8].LineNumber); // =
            Assert.Equal(3, tokens[9].LineNumber); // 2
            Assert.Equal(3, tokens[10].LineNumber); // ;
            Assert.Equal(4, tokens[11].LineNumber); // }
        }
    }
}