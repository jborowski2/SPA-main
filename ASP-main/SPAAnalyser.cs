using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main
{
    class SPAAnalyzer
{
    private readonly ASTNode _ast;

    public SPAAnalyzer(ASTNode ast)
    {
        _ast = ast;
    }

    public List<int> Analyze(PQLQuery query)
    {
        var results = new List<int>();

        foreach (var relation in query.Relations)
        {
            if (relation.Type.Equals("Modifies", StringComparison.OrdinalIgnoreCase))
            {
                // Usuwamy cudzysłowie z nazwy zmiennej jeśli istnieją
                string varName = relation.Arg2.Trim('"');
                results.AddRange(FindModifies(varName));
            }
        }

        return results.Distinct().ToList(); // Usuwamy duplikaty
    }

    private List<int> FindModifies(string varName)
    {
        var results = new List<int>();
        FindModifiesInNode(_ast, varName, results);
        return results.OrderBy(x => x).ToList(); // Sortujemy wyniki
    }

    private void FindModifiesInNode(ASTNode node, string varName, List<int> results)
    {
           // Console.WriteLine(node.Type);
           // Console.WriteLine(" ");
           // if (node.LineNumber.HasValue) Console.WriteLine(node.LineNumber.Value);
           // Console.WriteLine("\n");
            // Tylko węzły przypisań mogą modyfikować zmienne
            if (node.Type == "assign" && node.Value == varName && node.LineNumber.HasValue)
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
