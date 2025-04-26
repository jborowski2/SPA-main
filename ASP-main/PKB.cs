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
        private static readonly Lazy<PKB> _instance = new Lazy<PKB>(() => new PKB());

        /// <summary>
        /// Root of the abstract syntax tree (AST).
        /// </summary>
        public ASTNode Root { get; private set; }

        /// <summary>
        /// Dictionary mapping line numbers to corresponding AST nodes.
        /// </summary>
        public Dictionary<int, ASTNode> LineToNode { get; private set; }

        #region Follows Relation
        public Dictionary<string, string> Follows { get; private set; }
        #endregion

        #region Parent Relation
        public Dictionary<string, List<string>> Parent { get; private set; }
        public HashSet<(string Parent, string Child)> IsParent { get; private set; }
        #endregion

        #region Modifies Relation
        public Dictionary<string, List<string>> ModifiesStmt { get; private set; }
        public Dictionary<string, List<string>> ModifiesVar { get; private set; }
        public HashSet<(string Stmt, string Var)> IsModifiesStmtVar { get; private set; }
        public HashSet<(string Proc, string Var)> IsModifiesProcVar { get; private set; }
        #endregion

        #region USes Relation
        public Dictionary<string, List<string>> UsesStmt { get; private set; }
        public Dictionary<string, List<string>> UsesVar { get; private set; }
        public HashSet<(string Stmt, string Var)> IsUsesStmtVar { get; private set; }
        public HashSet<(string Proc, string Var)> IsUsesProcVar { get; private set; }
        #endregion

        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// </summary>
        private PKB() {
            LineToNode = new Dictionary<int, ASTNode>();
            Follows = new Dictionary<string, string>();

            Parent = new Dictionary<string, List<string>>();
            IsParent = new HashSet<(string Parent, string Child)>();

            ModifiesStmt = new Dictionary<string, List<string>>();
            ModifiesVar = new Dictionary<string, List<string>>();
            IsModifiesStmtVar = new HashSet<(string Stmt, string Var)>();
            IsModifiesProcVar = new HashSet<(string Proc, string Var)>();

            UsesStmt = new Dictionary<string, List<string>>();
            UsesVar = new Dictionary<string, List<string>>();
            IsUsesStmtVar = new HashSet<(string Stmt, string Var)>();
            IsUsesProcVar = new HashSet<(string Proc, string Var)>();
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
            IndexTree( root );
            PopulateRelations();

        }
        public void PopulateRelations()
        {
            PopulateFollows(Root);
            PopulateParent(Root);
            PopulateModifiesAndUses(Root);
        }
        private void PopulateFollows(ASTNode node)
        {
            if (node == null) return;

            foreach (var child in node.Children)
                PopulateFollows(child);

            foreach (var child in node.Children)
            {
                if (child.Follows != null && child.LineNumber.HasValue && child.Follows.LineNumber.HasValue)
                {
                    Follows[child.LineNumber.Value.ToString()] = child.Follows.LineNumber.Value.ToString();
                }
            }
        }
        private void PopulateParent(ASTNode node)
        {
            if (node == null) return;

            foreach (var child in node.Children)
            {
                PopulateParent(child);

                if (node.Type == "if" || node.Type == "while")
                {
                    if (node.LineNumber.HasValue && child.Type == "stmtLst")
                    {
                        foreach (var grandChild in child.Children)
                        {
                            if (grandChild.LineNumber.HasValue)
                            {
                                string parentLine = node.LineNumber.Value.ToString();
                                string childLine = grandChild.LineNumber.Value.ToString();

                                if (!Parent.ContainsKey(parentLine))
                                    Parent[parentLine] = new List<string>();

                                Parent[parentLine].Add(childLine);
                                IsParent.Add((parentLine, childLine));
                            }
                        }
                    }
                }
            }
        }
        private void PopulateModifiesAndUses(ASTNode node)
        {
            if (node == null) return;

            foreach (var child in node.Children)
                PopulateModifiesAndUses(child);

            if (node.LineNumber.HasValue)
            {
                string stmt = node.LineNumber.Value.ToString();

                if (node.Type == "assign")
                {
                    // Modifies: zmienna po lewej stronie
                    string varModified = node.Value;
                    AddModifies(stmt, varModified);

                    // Uses: zmienne po prawej stronie (w wyrażeniu)
                    var varsUsed = GetVariablesFromExpression(node.Children.FirstOrDefault());
                    foreach (var varUsed in varsUsed)
                    {
                        AddUses(stmt, varUsed);
                    }
                }
                else if (node.Type == "while" || node.Type == "if")
                {
                    // Uses: zmienna kontrolna
                    string controlVar = node.Value;
                    AddUses(stmt, controlVar);
                }
                else if (node.Type == "call")
                {
                    string procName = node.Value;
                    // Na późniejszym etapie dodamy Modifies i Uses dla call
                }
            }
        }

        private List<string> GetVariablesFromExpression(ASTNode exprNode)
        {
            List<string> vars = new List<string>();
            if (exprNode == null) return vars;

            if (exprNode.Type == "var")
            {
                vars.Add(exprNode.Value);
            }
            foreach (var child in exprNode.Children)
            {
                vars.AddRange(GetVariablesFromExpression(child));
            }
            return vars;
        }

        private void AddModifies(string stmt, string varName)
        {
            if (!ModifiesStmt.ContainsKey(stmt))
                ModifiesStmt[stmt] = new List<string>();
            if (!ModifiesStmt[stmt].Contains(varName))
                ModifiesStmt[stmt].Add(varName);

            if (!ModifiesVar.ContainsKey(varName))
                ModifiesVar[varName] = new List<string>();
            if (!ModifiesVar[varName].Contains(stmt))
                ModifiesVar[varName].Add(stmt);

            IsModifiesStmtVar.Add((stmt, varName));
        }

        private void AddUses(string stmt, string varName)
        {
            if (!UsesStmt.ContainsKey(stmt))
                UsesStmt[stmt] = new List<string>();
            if (!UsesStmt[stmt].Contains(varName))
                UsesStmt[stmt].Add(varName);

            if (!UsesVar.ContainsKey(varName))
                UsesVar[varName] = new List<string>();
            if (!UsesVar[varName].Contains(stmt))
                UsesVar[varName].Add(stmt);

            IsUsesStmtVar.Add((stmt, varName));
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
