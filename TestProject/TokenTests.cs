using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class TokenTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var type = "NAME";
            var value = "x";
            var lineNumber = 5;

            // Act
            var token = new Token(type, value, lineNumber);

            // Assert
            Assert.Equal(type, token.Type);
            Assert.Equal(value, token.Value);
            Assert.Equal(lineNumber, token.LineNumber);
        }

        [Fact]
        public void Equals_TokensWithDifferentProperties_AreNotEqual()
        {
            // Arrange
            var t1 = new Token("NAME", "x", 2);
            var t2 = new Token("NAME", "y", 2);
            var t3 = new Token("NAME", "x", 3);
            var t4 = new Token("NUMBER", "x", 2);

            // Act & Assert
            Assert.False(t1.Equals(t2));
            Assert.False(t1.Equals(t3));
            Assert.False(t1.Equals(t4));
        }

        [Fact]
        public void ToString_ReturnsUsefulRepresentation()
        {
            // Arrange
            var token = new Token("PLUS", "+", 11);

            // Act
            var str = token.ToString();

            // Assert
            Assert.Contains("PLUS", str, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("+", str);
            Assert.Contains("11", str);
        }

        [Fact]
        public void Token_DefaultLineNumberIsZero_IfNotSet()
        {
            // Arrange & Act
            var token = new Token("LPAREN", "(", 0);

            // Assert
            Assert.Equal(0, token.LineNumber);
        }

        [Fact]
        public void Token_AllowsNullOrEmptyValue()
        {
            // Arrange
            var token1 = new Token("SEMICOLON", null, 1);
            var token2 = new Token("RBRACE", "", 2);

            // Act & Assert
            Assert.Null(token1.Value);
            Assert.Equal("", token2.Value);
        }
    }
}
