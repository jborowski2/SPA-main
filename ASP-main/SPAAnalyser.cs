using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
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

     

        private List<int> FindLinesModifyingVariable(string varName)
        {
            var lines = new List<int>();
            FindAssignNodesForVariable(_ast, varName, lines);
            return lines;
        }

        private void FindAssignNodesForVariable(ASTNode node, string varName, List<int> lines)
        {
            if (DoesModify( node, varName))
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


        private bool DoesFollow(ASTNode node1, ASTNode model)
        {
            return true;
        }

    private bool isParent()
        {
            return true;

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
