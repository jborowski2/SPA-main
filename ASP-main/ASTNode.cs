using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA_main
{
    class ASTNode
    {
        public string Type { get; }
        public string Value { get; }
        public int? LineNumber { get; }
        public List<ASTNode> Children { get; } = new List<ASTNode>();
        public ASTNode Parent { get; set; }
        public ASTNode Follows { get; set; }
        public ASTNode FollowedBy { get; set; }

        public ASTNode(string type, string value = null, int? lineNumber = null)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
        }

        public void AddChild(ASTNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }
        public void SetFollows(ASTNode nextNode)
        {
            this.Follows = nextNode;
            if (nextNode != null)
            {
                nextNode.FollowedBy = this;
            }
        }
        public void PrintTree(int level = 0)
        {
            string indent = new string(' ', level * 2);
            string lineInfo = LineNumber.HasValue ? $" [Line {LineNumber}]" : "";

            switch (Type)
            {
                case "procedure":
                    Console.WriteLine(indent + $"PROCEDURE {Value}{lineInfo}");
                    foreach (var child in Children)
                        child.PrintTree(level + 1);
                    break;

                case "if":
                    Console.WriteLine(indent + $"IF ({Value}){lineInfo}");
                    Console.WriteLine(indent + "  THEN:");
                    if (Children.Count > 0) Children[0].PrintTree(level + 2);
                    Console.WriteLine(indent + "  ELSE:");
                    if (Children.Count > 1) Children[1].PrintTree(level + 2);
                    break;

                case "while":
                    Console.WriteLine(indent + $"WHILE ({Value}){lineInfo}");
                    if (Children.Count > 0) Children[0].PrintTree(level + 1);
                    break;

                case "call":
                    Console.WriteLine(indent + $"CALL {Value}{lineInfo}");
                    break;

                case "assign":
                    Console.WriteLine(indent + $"ASSIGN {Value}{lineInfo}");
                    foreach (var child in Children)
                        child.PrintTree(level + 1);
                    break;

                default:
                    Console.WriteLine(indent + $"{Type}: {Value}{lineInfo}");
                    foreach (var child in Children)
                        child.PrintTree(level + 1);
                    break;
            }
        }

        public override string ToString() => $"ASTNode({Type}, {Value}, [{string.Join(", ", Children)}])";
    }
}
