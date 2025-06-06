using Xunit;
using ASP_main;
using System.Linq;

namespace TestProject
{
    public class PQLLexerTests
    {
        [Fact]
        public void Tokenize_SimpleQuery_CreatesCorrectTokens()
        {
            // Arrange
            string query = "stmt s; Select s such that Follows(s, 2)";
            var lexer = new PQLLexer(query);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal(12, tokens.Count);
            Assert.Equal("STMT", tokens[0].Type);
            Assert.Equal("s", tokens[1].Value);
            Assert.Equal("SEMICOLON", tokens[2].Type);
            Assert.Equal("SELECT", tokens[3].Type);
            Assert.Equal("s", tokens[4].Value);
            Assert.Equal("SUCH_THAT", tokens[5].Type);
            Assert.Equal("Follows", tokens[6].Value);
            Assert.Equal("LPAREN", tokens[7].Type);
            Assert.Equal("s", tokens[8].Value);
            Assert.Equal("COMMA", tokens[9].Type);
            Assert.Equal("NUMBER", tokens[10].Type);
            Assert.Equal("RPAREN", tokens[11].Type);
        }

        [Fact]
        public void Tokenize_QueryWithPattern_CreatesCorrectTokens()
        {
            // Arrange
            string query = "assign a; Select a pattern a(\"x\", _\"y\"_)";
            var lexer = new PQLLexer(query);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal(18, tokens.Count);
            Assert.Equal("ASSIGN", tokens[0].Type);
            Assert.Equal("a", tokens[1].Value);
            Assert.Equal("SEMICOLON", tokens[2].Type);
            Assert.Equal("SELECT", tokens[3].Type);
            Assert.Equal("a", tokens[4].Value);
            Assert.Equal("PATTERN", tokens[5].Type);
            Assert.Equal("a", tokens[6].Value);
            Assert.Equal("LPAREN", tokens[7].Type);
            Assert.Equal("QUOTE", tokens[8].Type);
            Assert.Equal("x", tokens[9].Value);
            Assert.Equal("QUOTE", tokens[10].Type);
            Assert.Equal("COMMA", tokens[11].Type);
            Assert.Equal("UNDERSCORE", tokens[12].Type);
            Assert.Equal("QUOTE", tokens[13].Type);
            Assert.Equal("y", tokens[14].Value);
            Assert.Equal("QUOTE", tokens[15].Type);
            Assert.Equal("UNDERSCORE", tokens[16].Type);
            Assert.Equal("RPAREN", tokens[17].Type);
        }

        [Fact]
        public void Tokenize_MultipleDeclarationsAndWhitespace()
        {
            // Arrange
            string query = "stmt s1, s2;\nSelect s1";
            var lexer = new PQLLexer(query);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal("STMT", tokens[0].Type);
            Assert.Equal("s1", tokens[1].Value);
            Assert.Equal("COMMA", tokens[2].Type);
            Assert.Equal("s2", tokens[3].Value);
            Assert.Equal("SEMICOLON", tokens[4].Type);
            Assert.Equal("SELECT", tokens[5].Type);
            Assert.Equal("s1", tokens[6].Value);
        }

        [Fact]
        public void Tokenize_StringLiterals_And_Underscore()
        {
            // Arrange
            string query = "variable v; Select v such that Uses(v, \"x\") pattern a(_, _\"y\"_)";
            var lexer = new PQLLexer(query);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Contains(tokens, t => t.Type == "QUOTE" && t.Value == "\"");
            Assert.Contains(tokens, t => t.Type == "UNDERSCORE");
            Assert.Contains(tokens, t => t.Value == "y");
            Assert.Contains(tokens, t => t.Type == "USES");
        }

        [Fact]
        public void Tokenize_QueryWithWithClause()
        {
            // Arrange
            string query = "stmt s; Select s with s.stmt# = 5";
            var lexer = new PQLLexer(query);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal("WITH", tokens[5].Type);     // with
            Assert.Equal("NAME", tokens[6].Type);     // s
            Assert.Equal("DOT", tokens[7].Type);      // .
            Assert.Equal("STMT", tokens[8].Type);     // stmt#
            Assert.Equal("EQUALS", tokens[9].Type);   // =
            Assert.Equal("NUMBER", tokens[10].Type);  // 5
            Assert.Equal("5", tokens[10].Value);
        }

        [Fact]
        public void Tokenize_HandlesLineNumbers()
        {
            // Arrange
            string query = "stmt s;\nSelect s";
            var lexer = new PQLLexer(query);

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Equal(1, tokens[0].LineNumber); // stmt
            Assert.Equal(1, tokens[1].LineNumber); // s
            Assert.Equal(1, tokens[2].LineNumber); // ;
            Assert.Equal(2, tokens[3].LineNumber); // Select
            Assert.Equal(2, tokens[4].LineNumber); // s
        }

        [Fact]
        public void GetTokens_ReturnsEmptyListForEmptyQuery()
        {
            // Arrange
            var lexer = new PQLLexer("");

            // Act
            var tokens = lexer.GetTokens();

            // Assert
            Assert.Empty(tokens);
        }
    }
}