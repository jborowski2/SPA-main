using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ASP_main
{
    class SPAAnalyzer
    {
        private readonly ASTNode _ast;

        public SPAAnalyzer(ASTNode ast)
        {
            _ast = ast;
        }

        public List<string> Analyze(PQLQuery query)
        {
            var results = new List<string>();

            foreach (var relation in query.Relations)
            {
                if (relation.Type.Equals("Modifies", StringComparison.OrdinalIgnoreCase))
                {

                    if (int.TryParse(relation.Arg1, out int lineNumber))
                    {

                        // Format: Modifies(n, v) - znajdź zmienną modyfikowaną w linii n
                        string modifiedVar = FindModifiedVariableInLine(lineNumber);
                        if (modifiedVar != null)
                        {
                            results.Add(modifiedVar);
                        }
                    }
                    else
                    {
                        // Format: Modifies(s, "x") - znajdź linie modyfikujące zmienną "x"

                        string varName = relation.Arg2.Trim('"');
                        results.AddRange(FindLinesModifyingVariable(varName).Select(x => x.ToString()));
                    }
                }
                else if (relation.Type.Equals("Parent", StringComparison.OrdinalIgnoreCase) ||
               relation.Type.Equals("Parent*", StringComparison.OrdinalIgnoreCase))
                {
                    bool isTransitive = relation.Type.EndsWith("*", StringComparison.OrdinalIgnoreCase);

                    // Format: Parent(s, 10) - znajdź s takie że s jest rodzicem 10
                    if (int.TryParse(relation.Arg2, out int childLine))
                    {
                        var childNode = FindNodeByLine(_ast, childLine);
                        if (childNode != null)
                        {
                            if (isTransitive)
                            {
                                // Parent* - wszystkie poziomy rodziców
                                var parents = FindAllParents(childNode.LineNumber.Value);
                                results.AddRange(parents
                                    .Where(p => p.LineNumber.HasValue)
                                    .Select(p => p.LineNumber.ToString()));
                            }
                            else
                            {
                                // Parent - tylko bezpośredni rodzic
                                if (childNode.Parent != null && childNode.Parent.LineNumber.HasValue)
                                {
                                    results.Add(childNode.Parent.LineNumber.ToString());
                                }
                            }
                        }
                    }

                    // Format: Parent(8, s) - znajdź s takie że 8 jest rodzicem s
                    if (int.TryParse(relation.Arg1, out int parentLine))
                    {
                        var parentNode = FindNodeByLine(_ast, parentLine);
                        if (parentNode != null)
                        {
                            if (isTransitive)
                            {
                                // Parent* - wszystkie poziomy dzieci
                                results.AddRange(FindAllChildren(parentNode)
                                    .Where(c => c.LineNumber.HasValue)
                                    .Select(c => c.LineNumber.ToString()));
                            }
                            else
                            {
                                // Parent - tylko bezpośrednie dzieci
                                results.AddRange(parentNode.Children
                                    .Where(c => c.LineNumber.HasValue)
                                    .Select(c => c.LineNumber.ToString()));
                            }
                        }
                    }
                }
            }

            return results.Distinct().ToList();
        }

        private string FindModifiedVariableInLine(int lineNumber)
        {
            // Każda linia przypisania modyfikuje dokładnie jedną zmienną
            var assignNode = FindAssignNodeAtLine(_ast, lineNumber);
            return assignNode?.Value;
        }

        private ASTNode FindAssignNodeAtLine(ASTNode node, int targetLineNumber)
        {
            if (IsModified(node, targetLineNumber))
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                var result = FindAssignNodeAtLine(child, targetLineNumber);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private List<ASTNode> FindAllChildren(ASTNode node)
        {
            var children = new List<ASTNode>();
            foreach (var child in node.Children)
            {
                children.Add(child);
                children.AddRange(FindAllChildren(child));
            }
            return children;
        }

        private List<int> FindLinesModifyingVariable(string varName)
        {
            var lines = new List<int>();
            FindAssignNodesForVariable(_ast, varName, lines);
            return lines;
        }

        private void FindAssignNodesForVariable(ASTNode node, string varName, List<int> lines)
        {
            if (DoesModify(node, varName))
            {
                lines.Add(node.LineNumber.Value);
            }

            foreach (var child in node.Children)
            {
                FindAssignNodesForVariable(child, varName, lines);
            }
        }


        private bool DoesModify(ASTNode node, string varName)
        {
            return (node.Type == "assign" && node.Value == varName && node.LineNumber.HasValue);
        }


        private bool IsModified(ASTNode node, int targetLineNumber)
        {
            return (node.Type == "assign" && node.LineNumber.Value == targetLineNumber);
        }


        private bool DoesFollow(ASTNode node1, ASTNode node2)
        {
            return node1 != null && node1.Follows == node2;
        }
        public bool DoesFollowStar(ASTNode node1, ASTNode node2)
        {
            var current = node1;
            while (current != null)
            {
                if (current.Follows == node2) return true;
                current = current.Follows;
            }
            return false;
        }
        private bool IsParent(ASTNode child, ASTNode parent)
        {
            return child != null && child.Parent == parent;
        }
        public bool IsParentStar(ASTNode child, ASTNode parent)
        {
            var current = parent;
            while (current != null)
            {
                if (current.Parent == child)
                    return true;
                current = current.Parent;
            }
            return false;
        }
        public List<ASTNode> FindAllParents(int lineNumber)
        {
            var node = FindNodeByLine(_ast, lineNumber);
            var parents = new List<ASTNode>();

            while (node?.Parent != null)
            {
                parents.Add(node.Parent);
                node = node.Parent;
            }

            return parents;
        }
        public List<ASTNode> FindAllFollowing(int lineNumber)
        {
            var node = FindNodeByLine(_ast, lineNumber);
            var following = new List<ASTNode>();

            while (node?.Follows != null)
            {
                following.Add(node.Follows);
                node = node.Follows;
            }

            return following;
        }

        public ASTNode FindNodeByLine(ASTNode node, int targetLineNumber)
        {
            if (node.LineNumber.HasValue && node.LineNumber.Value == targetLineNumber)
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                var foundNode = FindNodeByLine(child, targetLineNumber);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return new ASTNode("brak ", "czegokolwiek", -1);
        }

        private bool DoesUse()
        {
            return true;
        }

        private void FindModifiesInNode(ASTNode node, string varName, List<int> results)
        {
            // Console.WriteLine(node.Type);
            // Console.WriteLine(" ");
            // if (node.LineNumber.HasValue) Console.WriteLine(node.LineNumber.Value);
            // Console.WriteLine("\n");

            if (DoesModify(node, varName))
            {
                results.Add(node.LineNumber.Value);

            }

            foreach (var child in node.Children)
            {
                FindModifiesInNode(child, varName, results);
            }
        }
    }
}
