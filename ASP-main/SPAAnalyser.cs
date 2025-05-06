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
    public class SPAAnalyzer
    {
        private readonly PKB _pkb;
        public  SPAAnalyzer(PKB pkb)
        {
            _pkb = pkb;
        }

        public List<string> Analyze(PQLQuery query)
        {
 
            var statementSubstitutions = new Dictionary<string, int>();
            bool wynik = true;
            var finalresults = new HashSet<string>();
            if (query.Selected.Name != "BOOLEAN")
                switch (query.Declarations[query.Selected.Name].Type)
                {
                case "assign":
                    finalresults = _pkb.Assings;
                    break;
                case "stmt":
                        finalresults = _pkb.Stmts;
                    break;
                case "while":
                        finalresults = _pkb.Whiles;
                    break;
                    case "if":
                        finalresults = _pkb.Ifs;
                        break;
                    case "procedure":
                        finalresults = _pkb.Procedures;
                        break;
                    case "prog_line":
                        finalresults = _pkb.Procedures;
                        break;
                    case "variable":
                        finalresults = _pkb.Variables;
                        break;
                    case "constant":
                        finalresults = _pkb.ConstValues;
                        break;

                }



            var updatedRelations = new List<Relation>();
            foreach (var relation in query.Relations)
            {
                var newArg1 = relation.Arg1;
                var newArg2 = relation.Arg2;
                var relationType = relation.Type;

                foreach (var withClause in query.WithClauses)
                {
                    if (withClause.Left.Reference != null && withClause.Right.Reference != null)
                    {
                        if (!query.Declarations.ContainsKey(withClause.Left.Reference))
                            continue;
                        bool test = false;
                        if(withClause.Left.Reference == query.Selected.Name)
                           
                            {
                            var filteredResults = new HashSet<string>(finalresults);

                            // Filtrujemy kopię na podstawie wybranego zbioru z _pkb
                            filteredResults.IntersectWith(query.Declarations[withClause.Right.Reference].Type switch
                            {
                                "assign" => _pkb.Assings,
                                "stmt" => _pkb.Stmts,
                                "while" => _pkb.Whiles,
                                "if" => _pkb.Ifs,
                                "procedure" => _pkb.Procedures,
                                "prog_line" => _pkb.Procedures,
                                "variable" => _pkb.Variables,
                                "constant" => _pkb.ConstValues,
                                _ => filteredResults // przypadek domyślny
                            });

                            // Przypisujemy przefiltrowane wyniki z powrotem do finalresults
                            finalresults = filteredResults;
                        }
                        else
                        {
                            test = false;
                            var testable = new HashSet<String>();
                            switch (query.Declarations[withClause.Left.Reference].Type)
                            {
                                case "assign":
                                    testable = _pkb.Assings;
                                    break;
                                case "stmt":
                                    testable = _pkb.Stmts;
                                    break;
                                case "while":
                                    testable = _pkb.Whiles;
                                    break;
                                case "if":
                                    testable = _pkb.Ifs;
                                    break;
                                case "procedure":
                                    testable = _pkb.Procedures;
                                    break;
                                case "prog_line":
                                    testable = _pkb.Procedures;
                                    break;
                                case "variable":
                                    testable = _pkb.Variables;
                                    break;
                                case "constant":
                                    testable = _pkb.ConstValues;
                                    break;

                            }
                            var sourceCollection = query.Declarations[withClause.Right.Reference].Type switch
                            {
                                "assign" => _pkb.Assings as IEnumerable<string>,
                                "stmt" => _pkb.Stmts as IEnumerable<string>,
                                "while" => _pkb.Whiles as IEnumerable<string>,
                                "if" => _pkb.Ifs as IEnumerable<string>,
                                "procedure" => _pkb.Procedures,
                                "prog_line" => _pkb.Procedures,
                                "variable" => _pkb.Variables,
                                "constant" => _pkb.ConstValues as IEnumerable<string>,
                                _ => testable
                            };

                            testable = testable.Intersect(sourceCollection).ToHashSet();
                            if (testable.Count != 0) test = true;
                            if (test == false) wynik = false;
                        }
                        
                    }
                    else
                    if (withClause.Left.Reference == null && withClause.Right.Reference == null)
                    {
                        if (withClause.Left.Value != withClause.Right.Value)
                            wynik = false;
                    }
                    else
                    {
                        if (!query.Declarations.ContainsKey(withClause.Left.Reference))
                            continue;
                        bool test = false;
                        var decl = query.Declarations[withClause.Left.Reference];
                        switch (decl.Type)
                        {
                            case "assign":
                                if (_pkb.Assings.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "stmt":
                                if (_pkb.Stmts.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "while":
                                if (_pkb.Whiles.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "if":
                                if (_pkb.Ifs.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "procedure":
                                if (_pkb.Procedures.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "prog_line":
                                if (_pkb.Stmts.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "variable":
                                if (_pkb.Variables.Contains(withClause.Right.Value))
                                    test = true;
                                break;
                            case "constant":
                                if (_pkb.ConstValues.Contains(withClause.Right.Value))
                                    test = true;
                                break;

                        }

                        if (withClause.Left.Reference == relation.Arg1 && test == true)
                        {
                            newArg1 = withClause.Right.Value;
                        }
                        else if (withClause.Left.Reference == relation.Arg2 && test == true)
                        {
                            newArg2 = withClause.Right.Value;
                        }

                        if (test == false)
                            wynik = false;
                    }
                }

                // Stwórz nową instancję Relation z zaktualizowanymi argumentami
                updatedRelations.Add(new Relation(
                     relationType,
                     newArg1,
                     newArg2));
                    // Dodaj inne właściwości jeśli są wymagane
                
            }

         
            foreach (var relation in updatedRelations)
            {
                var results = new HashSet<string>();
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
                            if (query.Selected.Name == "BOOLEAN")
                            {

                                if (modifiedVar == relation.Arg2)
                                    results.Add(modifiedVar);
                            }
                            else
                                results.Add(modifiedVar);
                        }
                    }
                    else
                    {
                        // Format: Modifies(s, "x") - znajdź linie modyfikujące zmienną "x"

                        string varName = relation.Arg2.Trim('"');
                        results.UnionWith(FindLinesModifyingVariable(varName).Select(x => x.ToString()));

                    }
                }
                else if (relation.Type.Equals("Uses", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(relation.Arg1, out int lineNumber))
                    {
                        // Format: Uses(n, v) - znajdź zmienne używane w linii n
                        var usedVars = FindVariablesUsedInLine(lineNumber);
                        if (query.Selected.Name == "BOOLEAN")
                        {
                            foreach (string s in usedVars)
                                if (s == relation.Arg1) //tu jak jest arg1 nie przechodzi testów jednostkowych, z arg2 przechodzi
                                    results.Add(s);
                        }
                        else
                            results.UnionWith(usedVars);

                    }
                    else
                    {
                        // Format: Uses(s, "x") - znajdź linie używające zmiennej "x"
                        string varName = relation.Arg2.Trim('"');
                        results.UnionWith(FindLinesUsingVariable(varName).Select(x => x.ToString()));
                    }
                }
                else if (relation.Type.Equals("Parent", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Parent(s, n) – dla linijki n w programie znajdź parenta
                    bool arg1IsInt = int.TryParse(relation.Arg1, out int parentLine);
                    bool arg2IsInt = int.TryParse(relation.Arg2, out int childLine);

                    if (arg1IsInt && arg2IsInt)
                    {
                        string parentFound = FindDirectParents(childLine);
                        if (query.Selected.Name == "BOOLEAN")
                        {
                            if (parentFound == parentLine.ToString())
                                results.Add("true");
                        }
                        else if (parentFound != "none")
                        {
                            results.Add(parentFound);
                        }
                    }
                    else if (arg1IsInt)
                    {
                        // Format: Parent(n, s) – dla linijki n w programie znajdź dzieci
                        var node = _pkb.GetNodeByLine(parentLine);
                        results.UnionWith(FindAllChildrenTransistive(node).Select(x => x.ToString()));
                    }
                    else if (arg2IsInt)
                    {
                        // Format: Parent(s, n) – dla linijki n w programie znajdź parenta
                        string parentFound = FindDirectParents(childLine);
                        if (parentFound != "none")
                            results.Add(parentFound);
                    }
                }
                else if (relation.Type.Equals("Parent*", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Parent*(s, n) – dla linijki n w programie znajdź wszystkich parentów
                    bool arg1IsInt = int.TryParse(relation.Arg1, out int parentLine);
                    bool arg2IsInt = int.TryParse(relation.Arg2, out int childLine);

                    if (arg1IsInt && arg2IsInt)
                    {
                        var parents = FindAllParentsTransitive(childLine);
                        if (query.Selected.Name == "BOOLEAN")
                        {
                            if (parents.Contains(parentLine.ToString()))
                                results.Add("true");
                        }
                        else
                        {
                            results.UnionWith(parents);
                        }
                    }
                    else if (arg1IsInt)
                    {
                        // Format: Parent*(n, s) – dla linijki n w programie znajdź wszystkie dzieci
                        var node = _pkb.GetNodeByLine(parentLine);
                        results.UnionWith(FindAllChildrenTransistive(node).Select(x => x.ToString()));
                    }
                    else if (arg2IsInt)
                    {
                        // Format: Parent*(s, n) – dla linijki n w programie znajdź wszystkich parentów
                        var parents = FindAllParentsTransitive(childLine);
                        results.UnionWith(parents);
                    }
                }
                else if (relation.Type.Equals("Follows", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Follows(s, n) – dla linijki n w programie znajdź lewego brata
                    bool arg1IsInt = int.TryParse(relation.Arg1, out int leftLine);
                    bool arg2IsInt = int.TryParse(relation.Arg2, out int rightLine);

                    if (arg1IsInt && arg2IsInt)
                    {
                        int found = int.Parse(FindDirectFollows(leftLine));
                        if (query.Selected.Name == "BOOLEAN")
                        {
                            if (found == rightLine)
                                results.Add("true");
                        }
                        else
                        {
                            results.Add(found.ToString());
                        }
                    }
                    else if (arg1IsInt)
                    {
                        // Format: Follows(n, s) – dla linijki n w programie znajdź prawego brata
                        string found = FindDirectFollows(leftLine);
                        if (found != null)
                            results.Add(found);
                    }
                    else if (arg2IsInt)
                    {
                        // Format: Follows(s, n) – dla linijki n w programie znajdź lewego brata
                        string found = FindDirectFollowsLeft(rightLine);
                        if (found != null)
                            results.Add(found);
                    }
                }
                else if (relation.Type.Equals("Follows*", StringComparison.OrdinalIgnoreCase))
                {
                    // Format: Follows*(s, n) – dla linijki n w programie znajdź wszystkich lewych braci
                    bool arg1IsInt = int.TryParse(relation.Arg1, out int leftLine);
                    bool arg2IsInt = int.TryParse(relation.Arg2, out int rightLine);

                    if (arg1IsInt && arg2IsInt)
                    {
                        var follows = FindAllFollowsTransitive(leftLine);
                        if (query.Selected.Name == "BOOLEAN")
                        {
                            if (follows.Contains(rightLine.ToString()))
                                results.Add("true");
                        }
                        else
                        {
                            results.UnionWith(follows);
                        }
                    }
                    else if (arg1IsInt)
                    {
                        // Format: Follows*(n, s) – dla linijki n w programie znajdź wszystkich prawych braci
                        var follows = FindAllFollowsTransitive(leftLine);
                        results.UnionWith(follows);
                    }
                    else if (arg2IsInt)
                    {
                        // Format: Follows*(s, n) – dla linijki n w programie znajdź wszystkich lewych braci
                        var follows = FindAllFollowsLeftTransitive(rightLine);
                        results.UnionWith(follows);
                    }
                }
                else if (relation.Type.Equals("Calls", StringComparison.OrdinalIgnoreCase))
                {

                    if ((_pkb.Procedures.Contains(relation.Arg1) && _pkb.Procedures.Contains(relation.Arg2)) ||((_pkb.Stmts.Contains(relation.Arg1) && _pkb.Procedures.Contains(relation.Arg2))))
                    {
                        if (!_pkb.IsCalls.Contains((relation.Arg1, relation.Arg2)))
                        {
                            wynik = false;

                        }
                        else
                        {
                            
                            results.Add(relation.Arg1);
                        }
                    }
                    else
                    if (_pkb.Procedures.Contains(relation.Arg1) || _pkb.Stmts.Contains(relation.Arg1))
                    {
                        results.UnionWith(_pkb.Calls[relation.Arg1]);

                    }
                    else
                    if(_pkb.Procedures.Contains(relation.Arg2) && query.Declarations[relation.Arg1].Type=="stmt")
                    {
                        if(_pkb.Called.Keys.Contains(relation.Arg2))
                        results.Add(relation.Arg2);
                    }
                    else
                    if (_pkb.Procedures.Contains(relation.Arg2))
                    {
                        
                        results.UnionWith(_pkb.Called[relation.Arg2]);

                    }
                    else
                    {
                        if (relation.Arg1 == query.Selected.Name)
                            results.UnionWith(_pkb.Calls.Keys.ToList());
                        else
                            results.UnionWith(_pkb.Called.Keys.ToList());

                    }

                }
                else if (relation.Type.Equals("Calls*", StringComparison.OrdinalIgnoreCase))
                {

                    if ((_pkb.Procedures.Contains(relation.Arg1) && _pkb.Procedures.Contains(relation.Arg2)) || ((_pkb.Stmts.Contains(relation.Arg1) && _pkb.Procedures.Contains(relation.Arg2))))
                    {
                        if (!_pkb.IsCallsStar.Contains((relation.Arg1, relation.Arg2)))
                        {
                            wynik = false;

                        }
                        else
                        {

                            results.Add(relation.Arg1);
                        }
                    }
                    else
                    if (_pkb.Procedures.Contains(relation.Arg1) || _pkb.Stmts.Contains(relation.Arg1))
                    {
                        results.UnionWith(_pkb.CallsStar[relation.Arg1]);

                    }
                    else
                    if (_pkb.Procedures.Contains(relation.Arg2) && query.Declarations[relation.Arg1].Type == "stmt")
                    {
                        if (_pkb.CalledStar.Keys.Contains(relation.Arg2))
                            results.Add(relation.Arg2);
                    }
                    else
                    if (_pkb.Procedures.Contains(relation.Arg2))
                    {

                        results.UnionWith(_pkb.CalledStar[relation.Arg2]);

                    }
                    else
                    {
                        if (relation.Arg1 == query.Selected.Name)
                            results.UnionWith(_pkb.CallsStar.Keys.ToList());
                        else
                            results.UnionWith(_pkb.CalledStar.Keys.ToList());

                    }

                }

                if (results.Count > 0)
                {

                    finalresults = new HashSet<string>(finalresults.Intersect(results));
                }
                else
                { wynik = false; }

            }

    
            if (wynik == true)
            {
              
                if (query.Selected.Name == "BOOLEAN")
                {
                    return new List<string> { "True" };
                }
                else
                {
                    if (finalresults.Count == 0)
                    { finalresults.Add("None"); }
                    return finalresults.ToList();
                }
            }
            else
           
            {
                if (query.Selected.Name == "BOOLEAN")
                {
                    return new List<string> { "False" };
                }
                else
                {
                    return new List<string> { "None" };
                }
            }
          
        }

      

        private string FindDirectFollowsLeft(int line)
        {
            ASTNode followed = _pkb.GetNodeByLine(line);
            if (followed != null && followed.FollowedBy != null)
            {
                return followed.FollowedBy.LineNumber.Value.ToString();
            }
            return null; // DANGER producent powinien obsłużyć wartość null (czyli tutaj) a nie konsument  
        }

        private List<string> FindAllFollowsLeftTransitive(int followsLine)
        {
            ASTNode followed = _pkb.GetNodeByLine(followsLine);
            List<string> results = new List<string>();
            while (followed != null)
            {
                if (followed != null && followed.FollowedBy != null)
                {
                    results.Add(followed.FollowedBy.LineNumber.Value.ToString());
                    followed = followed.FollowedBy;
                }
                else
                {
                    followed = null;
                }
            }

            return results;
        }

        private List<string> FindAllFollowsTransitive(int followsLine)
        {
            ASTNode followed = _pkb.GetNodeByLine(followsLine);
            List<string> results = new List<string>();
            while (followed != null)
            {
                if (followed != null && followed.Follows != null)
                {
                    results.Add(followed.Follows.LineNumber.Value.ToString());
                    followed = followed.Follows;
                }
                else
                {
                    followed = null;
                }
            }

            return results;
        }

        private string FindDirectFollows(int followsLine)
        {
            ASTNode followed = _pkb.GetNodeByLine(followsLine);
            if (followed != null && followed.Follows != null)
            {
                return followed.Follows.LineNumber.Value.ToString();
            }
            return null;// DANGER producent powinien obsłużyć wartość null (czyli tutaj) a nie konsument 
        }

        private List<string> FindVariablesUsedInLine(int lineNumber)
        {
            var variables = new List<string>();
            FindVariablesUsedInNode(_pkb.Root, lineNumber, variables);
            return variables.Distinct().ToList();
        }
        public string FindDirectParents(int childLine)
        {
            var childNode = _pkb.GetNodeByLine(childLine);
            if (childNode?.Parent?.Parent != null && (childNode.Parent.Parent.Type == "while" || childNode.Parent.Parent.Type == "if"))
            {
                return childNode.Parent.Parent.LineNumber.Value.ToString();
            }
            return null;// DANGER producent powinien obsłużyć wartość null (czyli tutaj) a nie konsument 
        }

        public List<string> FindAllParentsTransitive(int childLine)
        {
            var parents = new List<string>();
            var childNode = _pkb.GetNodeByLine(childLine);

            while (childNode?.Parent.Parent != null && (childNode.Parent.Parent.Type == "while" || childNode.Parent.Parent.Type == "if"))
            {
                parents.Add(childNode.Parent.Parent.LineNumber.Value.ToString());
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
            FindLinesUsingVariableInNode(_pkb.Root, varName, lines);
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
            var assignNode = FindAssignNodeAtLine(_pkb.Root, lineNumber);
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
                if (child.LineNumber != null)
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
            FindAssignNodesForVariable(_pkb.Root, varName, lines);
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


        /// <summary>
        /// deprecatred nie używać tego przeszukiwania w _pkb jest słownik klucz(numer lini ) wartośc (ASTNode) 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="varName"></param>
        /// <param name="results"></param>

        //public ASTNode FindNodeByLine(ASTNode node, int targetLineNumber)
        //{
        //    if (node.LineNumber.HasValue && node.LineNumber.Value == targetLineNumber)
        //    {
        //        return node;
        //    }

        //    foreach (var child in node.Children)
        //    {
        //        var foundNode = FindNodeByLine(child, targetLineNumber);
        //        if (foundNode != null)
        //        {
        //            return foundNode;
        //        }
        //    }

        //    return null;
        //}




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