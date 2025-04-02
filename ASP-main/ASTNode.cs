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
        public List<ASTNode> Children { get; } = new List<ASTNode>();

        public ASTNode(string type, string value = null)
        {
            Type = type;
            Value = value;
        }

        public void AddChild(ASTNode child)
        {
            Children.Add(child);
        }
        public void PrintTree(int level = 0)
        {
            Console.WriteLine(new string(' ', level * 2) + $"{Type}: {Value}");
            foreach (var child in Children)
            {
                child.PrintTree(level + 1);
            }
        }

        public override string ToString() => $"ASTNode({Type}, {Value}, [{string.Join(", ", Children)}])";
    }
}
