using SPA_main;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ASP_main
{
    /// <summary>
    /// Represents the Program Knowledge Base (PKB).
    /// Stores the root of the AST and a mapping from line numbers to AST nodes.
    /// Implemented as a singleton.
    /// </summary>
    public sealed class PKB
    {
        /// <summary>
        /// Singleton instance of PKB, initialized lazily and thread-safe.
        /// </summary>
        public int maxlinenumber;
        
        private static readonly Lazy<PKB> _instance = new Lazy<PKB>(() => new PKB());

        /// <summary>
        /// Root of the abstract syntax tree (AST).
        /// </summary>
        public ASTNode Root { get; private set; }

        /// <summary>
        /// Dictionary mapping line numbers to corresponding AST nodes.
        /// </summary>
        public Dictionary<int, ASTNode> LineToNode { get; private set; }

        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// </summary>
        private PKB() {
        }

        /// <summary>
        /// Returns the singleton instance of the PKB class.
        /// </summary>
        /// <returns>The single shared instance of PKB.</returns>
        public static PKB GetInstance() => _instance.Value;

        /// <summary>
        /// Sets the root of the AST and indexes all nodes by line number.
        /// </summary>
        /// <param name="root">The root node of the AST.</param>
        public void SetRoot( ASTNode root )
        {
            this.Root = root;
            LineToNode = new Dictionary<int, ASTNode>();
            IndexTree( root );
        }

        /// <summary>
        /// Recursively indexes AST nodes by their line numbers.
        /// Nodes without a line number are skipped.
        /// </summary>
        /// <param name="node">The current AST node to process.</param>
        private void IndexTree( ASTNode node)
        {
            if (node == null)
                return;

            if (node.LineNumber.HasValue)
            {
                int lineNum = node.LineNumber.Value;
                if(!LineToNode.ContainsKey(lineNum))
                    LineToNode[lineNum] = node;
                maxlinenumber = Math.Max( maxlinenumber, lineNum );
            }
            foreach (var child in node.Children)
            {
                IndexTree( child );
            }

        }


        /// <summary>
        /// Retrieves the AST node corresponding to the given line number.
        /// </summary>
        /// <param name="lineNumber">The source code line number.</param>
        /// <returns>The AST node at the specified line.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no node is found for the given line number.</exception>
        public ASTNode GetNodeByLine(int lineNumber)
        {
            LineToNode.TryGetValue(lineNumber, out var node);
            if (node == null)
                throw new KeyNotFoundException($"No node found for line {lineNumber}");
            return node;
        }
        


    }
}
