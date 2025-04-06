using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SPA_main;

namespace TestProject
{
    public class ASTNodeTests
    {
        [Fact]
        public void AddChild_ShouldSetParentAndAddToChildren()
        {
            // Arrange
            var parent = new ASTNode("parent");
            var child = new ASTNode("child");

            // Act
            parent.AddChild(child);

            // Assert
            Assert.Single(parent.Children);
            Assert.Equal(child, parent.Children[0]);
            Assert.Equal(parent, child.Parent);
        }

        [Fact]
        public void SetFollows_ShouldSetBidirectionalRelationship()
        {
            // Arrange
            var node1 = new ASTNode("node1");
            var node2 = new ASTNode("node2");

            // Act
            node1.SetFollows(node2);

            // Assert
            Assert.Equal(node2, node1.Follows);
            Assert.Equal(node1, node2.FollowedBy);
        }
    }
}
