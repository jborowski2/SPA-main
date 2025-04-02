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

        public ASTNode(string type, string value = null, int? lineNumber = null)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
        }

        public void AddChild(ASTNode child)
        {
            Children.Add(child);
        }
        public void PrintTree(int level = 0)
        {
            string indent = new string(' ', level * 2);
            switch (Type)
            {
                case "procedure":
                    Console.WriteLine(indent + $"PROCEDURE {Value}");
                    foreach (var child in Children)
                        child.PrintTree(level + 1);
                    break;

                case "if":
                    Console.WriteLine(indent + $"IF ({Value})");
                    Console.WriteLine(indent + "  THEN:");
                    if (Children.Count > 0) Children[0].PrintTree(level + 2);
                    Console.WriteLine(indent + "  ELSE:");
                    if (Children.Count > 1) Children[1].PrintTree(level + 2);
                    break;

                case "while":
                    Console.WriteLine(indent + $"WHILE ({Value})");
                    if (Children.Count > 0) Children[0].PrintTree(level + 1);
                    break;

                case "call":
                    Console.WriteLine(indent + $"CALL {Value}");
                    break;

                case "assign":
                    Console.WriteLine(indent + $"ASSIGN {Value}");
                    foreach (var child in Children)
                        child.PrintTree(level + 1);
                    break;

                default:
                    Console.WriteLine(indent + $"{Type}: {Value}");
                    foreach (var child in Children)
                        child.PrintTree(level + 1);
                    break;
            }
        }

        public override string ToString() => $"ASTNode({Type}, {Value}, [{string.Join(", ", Children)}])";
    }
}
