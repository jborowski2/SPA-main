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
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var token = new Token("TEST", "value", 42);
            var expected = "Token(TEST, value, Line 42)";

            // Act
            var result = token.ToString();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToString_WithNullValue_ShouldHandleGracefully()
        {
            // Arrange
            var token = new Token("TEST", null, 42);
            var expected = "Token(TEST, , Line 42)";

            // Act
            var result = token.ToString();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
