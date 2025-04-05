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
        var statementSubstitutions = new Dictionary<string, int>();

            foreach (var relation in query.Relations)
        {
                foreach (var withClause in query.WithClauses)
                {
                    if (withClause.Left.Attribute == "stmt#" && withClause.Right.IsValue)
                    {
                        if (int.TryParse(withClause.Right.Value, out int lineNumber))
                        {
                            statementSubstitutions[withClause.Left.Reference] = lineNumber;
                        }
                    }
                }

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
                else if (relation.Type.Equals("Uses", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(relation.Arg1, out int lineNumber))
                    {
                        // Format: Uses(n, v) - znajdź zmienne używane w linii n
                        var usedVars = FindVariablesUsedInLine(lineNumber);
                        results.AddRange(usedVars);

                    }
                    else
                    {
                        // Format: Uses(s, "x") - znajdź linie używające zmiennej "x"
                        string varName = relation.Arg2.Trim('"');
                        results.AddRange(FindLinesUsingVariable(varName).Select(x => x.ToString()));
                    }
                }
                else if (relation.Type.Equals("Parent", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Parent(s, n) - dla linijki n w programie znajdź parenta 
                    if (int.TryParse(relation.Arg2, out int childLine))
                    {
                        
                        results.Add(FindDirectParents(childLine).ToString());
                        
                    }
                    else if (int.TryParse(relation.Arg1, out int parentline))
                    {

                        var node = FindNodeByLine(_ast, parentline);
                        //format Paretn(n, s) - dla linijki n w programie znajdź dieci
                        results.AddRange(FindAllChildrenTransistive(node).Select(x => x.ToString())); 

                       
                    }
                }
                else if (relation.Type.Equals("Parent*", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Parent*(s, n) - dla linijki n w programie znajdź wszystkich parentów
                    if (int.TryParse(relation.Arg2, out int childLine))
                    {
                        results.AddRange(FindAllParentsTransitive(childLine).Select(x => x.ToString()));
                    }
                    else if (int.TryParse(relation.Arg1, out int parentline))
                    {
                        var node = FindNodeByLine(_ast, parentline);
                        //format Paretn(n, s) - dla linijki n w programie znajdź dieci
                        results.AddRange(FindAllChildrenTransistive(node).Select(x => x.ToString()));
                    }
                }
                else if (relation.Type.Equals("Follows", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Follows(s, n) - dla linijki n w programie znajdź prawego braciszka
                    if (int.TryParse(relation.Arg2, out int followsLine))
                    {

                        results.Add(FindDirectFollows(followsLine).ToString());
                    }
                    else
                    {
                        var folowsleft = int.Parse(relation.Arg1);
                        results.Add(wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww(folowsleft).ToString());
                        

                    }
                }
                else if (relation.Type.Equals("Follows*", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Follows(s, n) - dla linijki n w programie znajdź wszystkich prawych braciszków braciszka
                    if (int.TryParse(relation.Arg2, out int followsLine))
                    {
                        results.AddRange(FindAllFollowsTransitive(followsLine).Select(x => x.ToString()));
                    }
                    else
                    {
                        var folowsLeft = int.Parse(relation.Arg1);
                        results.AddRange(FindAllFollowsLeftTransitive(followsLine).Select(x => x.ToString()));
                    }
                }
            }
            if (results.Count == 0)
            { results.Add("None"); }
            return results.Distinct().ToList();
        }

        private object FindDirectFollowsLeft(int  line)
        {
            ASTNode followed = FindNodeByLine(_ast, line);
            if (followed != null && followed.FollowedBy != null)
            {
                return followed.FollowedBy.LineNumber.Value;
            }
            return "none";
        }
        private IEnumerable<object> FindAllFollowsLeftTransitive(int followsLine)
        {
            ASTNode followed = FindNodeByLine(_ast, followsLine);
            List<object> results = new List<object>();
            while (followed != null)
            {
                if (followed != null && followed.FollowedBy != null)
                {
                    results.Add(followed.FollowedBy.LineNumber.Value);
                    followed = followed.FollowedBy;
                }
                else
                {
                    followed = null;
                }
            }

            return results;
        }
        private IEnumerable<object> FindAllFollowsTransitive(int followsLine)
        {
            ASTNode followed = FindNodeByLine(_ast, followsLine);
            List<object> results = new List<object>();
            while (followed != null)
            {
                if (followed != null && followed.Follows != null)
                {
                    results.Add(followed.Follows.LineNumber.Value);
                    followed = followed.Follows;
                }
                else
                {
                    followed = null;
                }
            }
            
            return results;
        }

        private object FindDirectFollows(int followsLine)
        {
            ASTNode followed = FindNodeByLine(_ast, followsLine);
            if (followed != null && followed.Follows!= null)
            {
                return followed.Follows.LineNumber.Value;
            }
            return "none";
        }

        private List<string> FindVariablesUsedInLine(int lineNumber)
        {
            var variables = new List<string>();
            FindVariablesUsedInNode(_ast, lineNumber, variables);
            return variables.Distinct().ToList();
        }
        public int FindDirectParents(int childLine)
        {
            var childNode = FindNodeByLine(_ast, childLine);
            if (childNode?.Parent?.Parent != null && (childNode.Parent.Parent.Type == "while" || childNode.Parent.Parent.Type == "if"))
            {
                return childNode.Parent.Parent.LineNumber.Value;
            }
            return -1;
        }
        public List<int> FindAllParentsTransitive(int childLine)
        {
            var parents = new List<int>();
            var childNode = FindNodeByLine(_ast, childLine);

            while (childNode?.Parent.Parent != null && (childNode.Parent.Parent.Type == "while" || childNode.Parent.Parent.Type == "if"))
            {
                parents.Add(childNode.Parent.Parent.LineNumber.Value);
                childNode = childNode.Parent.Parent;
            }

            return parents;
        }
        private void FindVariablesUsedInNode(ASTNode node, int targetLineNumber, List<string> variables)
        {
            if (node.LineNumber.HasValue && node.LineNumber.Value == targetLineNumber)
            {
                if (node.Type == "assign")
                {
                    // Dla przypisania, szukamy zmiennych w wyrażeniu po prawej stronie
                    foreach (var child in node.Children)
                    {
                        CollectVariablesFromExpression(child, variables);
                    }
                }
                else if (node.Type == "while")
                {
                    variables.Add(node.Value);
                }
            }

            foreach (var child in node.Children)
            {
                FindVariablesUsedInNode(child, targetLineNumber, variables);
            }
        }

        private List<int> FindLinesUsingVariable(string varName)
        {
            var lines = new List<int>();
            FindLinesUsingVariableInNode(_ast, varName, lines);
            return lines.Distinct().OrderBy(x => x).ToList();
        }

        private void FindLinesUsingVariableInNode(ASTNode node, string varName, List<int> lines)
        {
            if (DoesUse(node, varName) && node.LineNumber.HasValue)
            {
                lines.Add(node.LineNumber.Value);
            }

            foreach (var child in node.Children)
            {
                FindLinesUsingVariableInNode(child, varName, lines);
            }
        }

        private bool DoesUse(ASTNode node, string varName)
        {
            if (node.Type == "assign")
            {
                // Sprawdzamy wyrażenie po prawej stronie przypisania
                foreach (var child in node.Children)
                {
                    if (ExpressionContainsVariable(child, varName))
                    {
                        return true;
                    }
                }
            }
            else if (node.Type == "while" && node.Value == varName)
            {
                return true;
            }
            return false;
        }

        private bool ExpressionContainsVariable(ASTNode exprNode, string varName)
        {
            if (exprNode == null) return false;

            if (exprNode.Type == "var" && exprNode.Value == varName)
            {
                return true;
            }

            foreach (var child in exprNode.Children)
            {
                if (ExpressionContainsVariable(child, varName))
                {
                    return true;
                }
            }

            return false;
        }

        private void CollectVariablesFromExpression(ASTNode exprNode, List<string> variables)
        {
            if (exprNode == null) return;

            if (exprNode.Type == "var")
            {
                variables.Add(exprNode.Value);
            }

            foreach (var child in exprNode.Children)
            {
                CollectVariablesFromExpression(child, variables);
            }
        }
        private string? FindModifiedVariableInLine(int lineNumber)
        {
            // Każda linia przypisania modyfikuje dokładnie jedną zmienną
            var assignNode = FindAssignNodeAtLine(_ast, lineNumber);
            return assignNode?.Value;
        }

        private ASTNode? FindAssignNodeAtLine(ASTNode node, int targetLineNumber)
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

        private List<int> FindAllChildrenTransistive(ASTNode node)
        {
            var children = new List<int>();
            foreach (var child in node.Children)
            {
                if(child.LineNumber != null)
                {
                    children.Add(child.LineNumber.Value);
                }
       
                children.AddRange(FindAllChildrenTransistive(child));
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

            return null; 
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
