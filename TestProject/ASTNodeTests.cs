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
        public void Constructor_ShouldSetTypeValueAndLineNumber()
        {
            // Arrange & Act
            var node = new ASTNode("assign", "x", 3);

            // Assert
            Assert.Equal("assign", node.Type);
            Assert.Equal("x", node.Value);
            Assert.Equal(3, node.LineNumber);
            Assert.Empty(node.Children);
            Assert.Null(node.Parent);
        }

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
        public void AddChild_ShouldPropagateProcNameIfSet()
        {
            // Arrange
            var parent = new ASTNode("procedure", "main");
            parent.ProcName = "main";
            var child = new ASTNode("assign", "x");

            // Act
            parent.AddChild(child);

            // Assert
            Assert.Equal("main", child.ProcName);
        }

        [Fact]
        public void SetProcNameRecursively_ShouldPropagateToAllDescendants()
        {
            // Arrange
            var root = new ASTNode("procedure", "main");
            root.ProcName = "main";
            var child = new ASTNode("assign", "x");
            var grandchild = new ASTNode("const", "5");
            child.AddChild(grandchild);

            // Act
            root.AddChild(child);

            // Assert
            Assert.Equal("main", child.ProcName);
            Assert.Equal("main", grandchild.ProcName);
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

        [Fact]
        public void PrintTree_ShouldNotThrow_ForSimpleTree()
        {
            // Arrange
            var root = new ASTNode("procedure", "main", 1);
            var assign = new ASTNode("assign", "x", 2);
            root.AddChild(assign);

            // Act & Assert
            var ex = Record.Exception(() => root.PrintTree());
            Assert.Null(ex);
        }

        [Fact]
        public void ToString_ShouldReturnReadableRepresentation()
        {
            // Arrange
            var node = new ASTNode("assign", "x", 5);

            // Act
            var str = node.ToString();

            // Assert
            Assert.Contains("ASTNode(assign, x", str);
        }

        [Fact]
        public void ParentAndChildren_ShouldBeConsistent()
        {
            // Arrange
            var parent = new ASTNode("procedure", "main");
            var child = new ASTNode("assign", "x");

            // Act
            parent.AddChild(child);

            // Assert
            Assert.Equal(parent, child.Parent);
            Assert.Contains(child, parent.Children);
        }

        [Fact]
        public void MultipleChildrenAndFollows_ShouldBeSetCorrectly()
        {
            // Arrange
            var parent = new ASTNode("stmtLst");
            var a = new ASTNode("assign", "x", 1);
            var b = new ASTNode("assign", "y", 2);
            var c = new ASTNode("assign", "z", 3);
            parent.AddChild(a);
            parent.AddChild(b);
            parent.AddChild(c);

            // Act
            a.SetFollows(b);
            b.SetFollows(c);

            // Assert
            Assert.Equal(b, a.Follows);
            Assert.Equal(c, b.Follows);
            Assert.Equal(a, b.FollowedBy);
            Assert.Equal(b, c.FollowedBy);
        }
    }
}
