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
        bool test = false;
        private readonly PKB _pkb;

        public  SPAAnalyzer(PKB pkb)
        {
            _pkb = pkb;
        }

        public void PrintResults(List<Dictionary<string, string>> resultRows)
        {
            if (!test) return;
            Console.WriteLine("Nowe wykonanie:");
            if (resultRows == null || resultRows.Count == 0)
            {
                Console.WriteLine("No results.");
                return;
            }

            int index = 1;
            foreach (var row in resultRows)
            {
                Console.WriteLine($"--- Result {index} ---");
                foreach (var kvp in row)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
                Console.WriteLine(); // pusta linia między słownikami
                index++;
            }
        }


        public List<string> Analyze(PQLQuery query)
        {
            bool isBoolean = query.Selected[0].Name == "BOOLEAN";
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            var twoSynonyms = new List<Relation>();
            var leftOnlySynonym = new List<Relation>();
            var rightOnlySynonym = new List<Relation>();
            var noSynonyms = new List<Relation>();
            rows.Add(new Dictionary<string, string>());

            foreach (var relation in query.Relations)
            {
                bool isArg1Syn = IsSynonym(relation.Arg1, query);
                bool isArg2Syn = IsSynonym(relation.Arg2, query);

                if (isArg1Syn && isArg2Syn)
                {
                    twoSynonyms.Add(relation);
                }
                else if (isArg1Syn)
                {
                    leftOnlySynonym.Add(relation);
                }
                else if (isArg2Syn)
                {
                    rightOnlySynonym.Add(relation);
                }
                else
                {
                    noSynonyms.Add(relation);
                }
            }


            foreach (var relation in noSynonyms)
            {
                switch (relation.Type.ToUpper())
                {
                    case "FOLLOWS":
                        if (!CheckFollowsRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "PARENT":
                        if (!CheckParentRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "PARENT*":
                        if (!CheckParentStarRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "MODIFIES":
                        if (!CheckModifiesRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "USES":
                        if (!CheckUsesRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "CALLS":
                        if (!CheckCallsRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "CALLS*":
                        if (!CheckCallsStarRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "NEXT":
                        if (!CheckNextRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "NEXT*":
                        if (!CheckNextStarRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    case "FOLLOWS*":
                        if (!CheckFollowsStarRelation(relation.Arg1, relation.Arg2))
                            return ReturnEmpty(query);
                        break;
                    default:
                        throw new NotImplementedException($"Relacja {relation.Type} nieobsługiwana.");
                }

            }

            foreach (var with in query.WithClauses)
            {
                // === [DODANE] Sprawdzenie: oba argumenty to stałe (np. 10 = 5) ===
                if (with.Left.IsValue && with.Right.IsValue)
                {
                    if (with.Left.Value != with.Right.Value)
                    {
                        // Konflikt wartości stałych — nie może być spełnione
                        return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                    }
                    // Są równe — nie wpływa na synonimy, ale nie koliduje
                    continue;
                }

                WithArgument attrSide = null;
                WithArgument valueSide = null;

                if (with.Left.IsAttributeRef && with.Right.IsValue)
                {
                    attrSide = with.Left;
                    valueSide = with.Right;
                }
                else if (with.Right.IsAttributeRef && with.Left.IsValue)
                {
                    attrSide = with.Right;
                    valueSide = with.Left;
                }
                else
                {
                    // Pomijamy porównania dwóch atrybutów lub nazw
                    continue;
                }

                var synonymName = attrSide.Reference;
                var attribute = attrSide.Attribute;
                var constValue = valueSide.Value;

                if (!query.Declarations.ContainsKey(synonymName))
                    continue;

                var declaration = query.Declarations[synonymName];

                // Sprawdź, czy wartość istnieje w programie
                if (IsValidValueForType(declaration.Type, constValue))
                {
                    declaration.Attributes[attribute] = constValue;
                }
                else
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

                var row = rows[0];
                if (!row.ContainsKey(synonymName))
                {
                    row[synonymName] = constValue;
                }
                else if (row[synonymName] != constValue)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }
                PrintResults(rows);
            }




            foreach (var relation in leftOnlySynonym)
            {
                var newRows = new List<Dictionary<string, string>>();
                string left = relation.Arg1;
                string right = relation.Arg2;    //.Replace("\"", ""); // oczyszczona wartość
                string leftType = query.Declarations.ContainsKey(left)
                     ? query.Declarations[left].Type.ToUpper()
                     : "";
                // Pobierz dopasowania z PKB w zależności od typu relacji
                HashSet<string> matchingLefts = relation.Type switch
                {
                    "Parent" => _pkb.IsParent
                                    .Where(p => p.Child == right)
                                    .Select(p => p.Parent)
                                    .ToHashSet(),

                    "Parent*" => _pkb.IsParentStar
                                    .Where(p => p.Child == right)
                                    .Select(p => p.Parent)
                                    .ToHashSet(),

                    "Follows" => _pkb.Follows
                                    .Where(p => p.Value == right)
                                    .Select(p => p.Key)
                                    .ToHashSet(),

                    "Follows*" => _pkb.IsFollowsStar
                                    .Where(p => p.Item2 == right)
                                    .Select(p => p.Item1)
                                    .ToHashSet(),

                    "Modifies" => leftType == "PROCEDURE"
                    ? _pkb.IsModifiesProcVar
                                        .Where(p => p.Var == right)
                                        .Select(p => p.Proc)
                                        .ToHashSet()
                                    : _pkb.IsModifiesStmtVar
                                    .Where(p => p.Var == right)
                                    .Select(p => p.Stmt)
                                    .ToHashSet(),

                               

                    "Uses" => leftType == "PROCEDURE"
                        ? _pkb.IsUsesProcConst
                                    .Where(p => p.Item2 == right)
                                    .Select(p => p.Item1)
                              .ToHashSet()
                            .Union(
                                _pkb.IsUsesProcVar
                                    .Where(p => p.Var == right)
                                    .Select(p => p.Proc)
                            ).ToHashSet()
                        : _pkb.IsUsesStmtConst
                              .Where(p => p.Item2 == right)
                              .Select(p => p.Item1)
                              .ToHashSet()
                            .Union(
                                _pkb.IsUsesStmtVar
                              .Where(p => p.Var == right)
                              .Select(p => p.Stmt)
                            ).ToHashSet(),

                    "Calls" => _pkb.IsCalls
                                .Where(p => p.Callee == right)
                                .Select(p => p.Caller)
                                .ToHashSet(),

                    "Calls*" => _pkb.IsCallsStar
                                .Where(p => p.Callee == right)
                                .Select(p => p.Caller)
                                .ToHashSet(),

                    "Next" => _pkb.IsNext
                                .Where(p => p.Item2 == right)
                                .Select(p => p.Item1)
                                .ToHashSet(),

                    "Next*" => _pkb.IsNextStar
                                .Where(p => p.Item2 == right)
                                .Select(p => p.Item1)
                                .ToHashSet(),

                    _ => new HashSet<string>()
                };


                if (matchingLefts.Count == 0)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

              
                foreach (var row in rows)
                {
                    if (row.ContainsKey(left))
                    {
                        // Synonim ma już przypisaną wartość — sprawdzamy dopasowanie
                        if (matchingLefts.Contains(row[left]))
                        {
                            newRows.Add(row); // pasuje, zostaje
                        }
                        // Jeśli nie pasuje, nie dodajemy go — odpada
                    }
                 
                    else
                    {
                        // Synonim nie ma przypisanej wartości — tworzymy nowe wiersze
                        foreach (var val in matchingLefts)
                        {
                            // Sprawdzenie typu instrukcji, jeśli leftType to konkretny typ stmt
                            if (int.TryParse(val, out var stmtNum) &&
                                (leftType == "ASSIGN" || leftType == "WHILE" || leftType == "IF" || leftType == "STMT" || leftType=="CALL" || leftType=="PROCEDURE"))
                            {
                                var node = _pkb.GetNodeByLine(stmtNum);
                                if (node == null || (node.Type.ToUpper() != leftType && leftType!="STMT"))
                                    continue; // pomiń jeśli niezgodny typ
                            }

                            var newRow = new Dictionary<string, string>(row);
                            newRow[left] = val;
                            newRows.Add(newRow);
                        }
                    }

                }

                // Jeśli nie ma żadnych pasujących wierszy — kończymy
                if (newRows.Count == 0)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

                // Zamieniamy stare wiersze na nowe
                rows = newRows;
                PrintResults(rows);
            }

        //   PrintResults(rows);

            foreach (var relation in rightOnlySynonym)
            {
                var newRows = new List<Dictionary<string, string>>();
                string left = relation.Arg1.Replace("\"", ""); // oczyszczona wartość (stała)
                string right = relation.Arg2;

                string rightType = query.Declarations.ContainsKey(right)
     ? query.Declarations[right].Type.ToUpper()
     : "";

                HashSet<string> matchingRights = relation.Type switch
                {
                    "Parent" => _pkb.IsParent
                                    .Where(p => p.Parent == left)
                                    .Select(p => p.Child)
                                    .ToHashSet(),

                    "Parent*" => _pkb.IsParentStar
                                    .Where(p => p.Parent == left)
                                    .Select(p => p.Child)
                                    .ToHashSet(),

                    "Follows" => _pkb.Follows
                                    .Where(p => p.Key == left)
                                    .Select(p => p.Value)
                                    .ToHashSet(),

                    "Follows*" => _pkb.IsFollowsStar
                                    .Where(p => p.Item1 == left)
                                    .Select(p => p.Item2)
                                    .ToHashSet(),

                    "Modifies" => _pkb.IsModifiesStmtVar
                    .Where(p => p.Stmt == left)
                    .Select(p => p.Var)
                    .ToHashSet()
                .Union(
                    _pkb.IsModifiesProcVar
                        .Where(p => p.Proc == left)
                        .Select(p => p.Var)
                )
                .ToHashSet(),

                    "Uses" =>
     rightType == "VARIABLE"
         ? _pkb.IsUsesStmtVar
             .Where(p => p.Stmt == left)
             .Select(p => p.Var)
             .ToHashSet()
             .Union(
                 _pkb.IsUsesProcVar
                     .Where(p => p.Proc == left)
                     .Select(p => p.Var)
             ).ToHashSet()
         : _pkb.IsUsesStmtConst
             .Where(p => p.Item1 == left)
             .Select(p => p.Item2)
             .ToHashSet()
             .Union(
                 _pkb.IsUsesProcConst
                     .Where(p => p.Item1 == left)
                     .Select(p => p.Item2)
             ).ToHashSet(),



                    "Calls" => _pkb.IsCalls
                                    .Where(p => p.Caller == left)
                                    .Select(p => p.Callee)
                                    .ToHashSet(),

                    "Calls*" => _pkb.IsCallsStar
                                    .Where(p => p.Caller == left)
                                    .Select(p => p.Callee)
                                    .ToHashSet(),

                    "Next" => _pkb.IsNext
                                    .Where(p => p.Item1 == left)
                                    .Select(p => p.Item2)
                                    .ToHashSet(),

                    "Next*" => _pkb.IsNextStar
                                    .Where(p => p.Item1 == left)
                                    .Select(p => p.Item2)
                                    .ToHashSet(),

                    _ => new HashSet<string>()
                };

                if (matchingRights.Count == 0)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

                foreach (var row in rows)
                {
                    foreach (var val in matchingRights)
                    {
                        // Jeśli synonim dotyczy typu instrukcji, sprawdź jego zgodność z typem z PKB
                        if (int.TryParse(val, out var lineNum))
                        {
                            var node = _pkb.GetNodeByLine(lineNum);
                            if (node == null)
                                continue;

                            var nodeType = node.Type.ToUpper();

                            if (rightType == "ASSIGN" && nodeType != "ASSIGN")
                                continue;
                            if (rightType == "WHILE" && nodeType != "WHILE")
                                continue;
                            if (rightType == "IF" && nodeType != "IF")
                                continue;
                            if (rightType == "CALL" && nodeType != "CALL")
                                continue;
                            if (rightType == "STMT" &&
                                nodeType != "ASSIGN" &&
                                nodeType != "WHILE" &&
                                nodeType != "IF" &&
                                nodeType != "CALL")
                                continue;
                            // dla PROG_LINE nie trzeba sprawdzać typu
                        }

                        // Jeśli wiersz już ma przypisaną wartość dla synonimu — sprawdź zgodność
                        if (row.ContainsKey(right))
                        {
                            if (row[right] != val)
                                continue; // niezgodność, pomiń
                            else
                                newRows.Add(row); // zgodność, dodaj oryginalny wiersz
                        }
                        else
                        {
                            var newRow = new Dictionary<string, string>(row);
                            newRow[right] = val;
                            newRows.Add(newRow);
                        }
                    }
                }



                if (newRows.Count == 0)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

                rows = newRows;
                PrintResults(rows);
            }



             //PrintResults(rows);

            foreach (var relation in twoSynonyms)
            {
                var newRows = new List<Dictionary<string, string>>();
                string left = relation.Arg1;
                string right = relation.Arg2;
                string leftType = query.Declarations.ContainsKey(left)
        ? query.Declarations[left].Type.ToUpper()
        : "";
                string rightType = query.Declarations.ContainsKey(right)
                    ? query.Declarations[right].Type.ToUpper()
                    : "";
                // Pobierz wszystkie możliwe pary (lewy, prawy) z PKB
                HashSet<(string, string)> matchingPairs = relation.Type switch
                {
                    "Parent" => _pkb.IsParent,
                    "Parent*" => _pkb.IsParentStar,
                    "Follows" => _pkb.Follows.Select(kvp => (kvp.Key, kvp.Value)).ToHashSet(),
                    "Follows*" => _pkb.IsFollowsStar,
                    "Uses" => (leftType, rightType) switch
                    {
                        ("PROCEDURE", "CONSTANT") => _pkb.IsUsesProcConst,
                        ("STMT", "CONSTANT") => _pkb.IsUsesStmtConst,
                        ("PROCEDURE", "VARIABLE") => _pkb.IsUsesProcVar,
                        ("STMT", "VARIABLE") => _pkb.IsUsesStmtVar,
                        ("IF", "CONSTANT") => _pkb.IsUsesStmtConst,
                        ("IF", "VARIABLE") => _pkb.IsUsesStmtVar,
                        ("WHILE", "CONSTANT") => _pkb.IsUsesStmtConst,
                        ("WHILE", "VARIABLE") => _pkb.IsUsesStmtVar,
                        _ => new HashSet<(string, string)>()
                    },

                    "Modifies" => (leftType, rightType) switch
                    {
                        ("PROCEDURE", "VARIABLE") => _pkb.IsModifiesProcVar,
                        ("STMT", "VARIABLE") => _pkb.IsModifiesStmtVar,
                        ("WHILE", "VARIABLE") => _pkb.IsModifiesStmtVar,
                        ("IF", "VARIABLE") => _pkb.IsModifiesStmtVar,
                        _ => new HashSet<(string, string)>()
                    },

                    "Calls" => _pkb.IsCalls
             .Where(p =>
                 ((leftType == "PROCEDURE" && !int.TryParse(p.Caller, out _)) ||
                  (leftType == "STMT" && int.TryParse(p.Caller, out _))||
                   
                  (leftType == "CALL" && int.TryParse(p.Caller, out _))) &&
                 (rightType == "PROCEDURE")) // Callee zawsze musi być procedurą
             .ToHashSet(),

                    "Calls*" => _pkb.IsCallsStar
                        .Where(p =>
                            ((leftType == "PROCEDURE" && !int.TryParse(p.Caller, out _)) ||
                             (leftType == "STMT" && int.TryParse(p.Caller, out _)) ||
                             (leftType == "CALL" && int.TryParse(p.Caller, out _))) &&
                            (rightType == "PROCEDURE"))
                        .ToHashSet(),
                    "Next" => _pkb.IsNext,
                    "Next*" => _pkb.IsNextStar,
                    _ => new HashSet<(string, string)>()
                };

                if (matchingPairs.Count == 0)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

                foreach (var row in rows)
                {
                    bool hasLeft = row.ContainsKey(left);
                    bool hasRight = row.ContainsKey(right);

                    if (hasLeft && hasRight)
                    {
                        // Oba synonimy mają przypisane wartości — sprawdzamy czy istnieje relacja
                        if (matchingPairs.Contains((row[left], row[right])))
                        {
                            newRows.Add(row); // pasuje, zostaje
                        }
                        // W przeciwnym wypadku nie dodajemy wiersza
                    }
                    else if (hasLeft)
                    {
                        var compatibleRights = matchingPairs
                            .Where(p => p.Item1 == row[left])
                            .Select(p => p.Item2)
                            .ToHashSet();

                        foreach (var r in compatibleRights)
                        {
                            if (!IsValueCompatibleWithType(query, right, r))
                                continue;

                            var newRow = new Dictionary<string, string>(row);
                            newRow[right] = r;
                            newRows.Add(newRow);
                        }
                    }
                    else if (hasRight)
                    {
                        var compatibleLefts = matchingPairs
                            .Where(p => p.Item2 == row[right])
                            .Select(p => p.Item1)
                            .ToHashSet();

                        foreach (var l in compatibleLefts)
                        {
                            if (!IsValueCompatibleWithType(query, left, l))
                                continue;

                            var newRow = new Dictionary<string, string>(row);
                            newRow[left] = l;
                            newRows.Add(newRow);
                        }
                    }

                    else
                    {
                        foreach (var (l, r) in matchingPairs)
                        {
                            bool leftValid = true, rightValid = true;

                            // Jeśli lewy to numer, sprawdź typ instrukcji
                            if (int.TryParse(l, out var lNum))
                            {
                                var node = _pkb.GetNodeByLine(lNum);
                                leftValid = node != null && IsNodeTypeMatch(leftType, node.Type);
                            }

                            // Jeśli prawy to numer, sprawdź typ instrukcji
                            if (int.TryParse(r, out var rNum))
                            {
                                var node = _pkb.GetNodeByLine(rNum);
                                rightValid = node != null && IsNodeTypeMatch(rightType, node.Type);
                            }

                            if (!leftValid || !rightValid)
                                continue;

                            var newRow = new Dictionary<string, string>(row);
                            newRow[left] = l;
                            newRow[right] = r;
                            newRows.Add(newRow);
                        }
                    }

                }

                if (newRows.Count == 0)
                {
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };
                }

                rows = newRows;
                PrintResults(rows);
            }


            // PrintResults(rows);


            foreach (var pattern in query.PatternClauses)
            {
                var newRows = new List<Dictionary<string, string>>();
                var assignSyn = pattern.AsignSynonym;

                if (!query.Declarations.TryGetValue(assignSyn, out var declaration))
                    continue;

                var type = declaration.Type.ToUpper();
                HashSet<string> candidateSet = type switch
                {
                    "ASSIGN" => _pkb.Assings,
                    "WHILE" => _pkb.Whiles,
                    "IF" => _pkb.Ifs,
                    _ => new HashSet<string>() // Nieobsługiwany typ
                };

                foreach (var row in rows)
                {
                    var candidateLines = row.ContainsKey(assignSyn)
                        ? new List<string> { row[assignSyn] }
                        : candidateSet.ToList();

                    foreach (var line in candidateLines)
                    {
                        var node = _pkb.GetNodeByLine(int.Parse(line));
                        if (node == null || node.Type.ToUpper() != type)
                            continue;

                        var leftNode = pattern.AssignAST; // lewa strona przypisania
                        string matchedLeftValue = null;

                        bool leftOk = false;

                        if (query.Declarations.ContainsKey(leftNode.Value))
                        {
                            // Jeśli już podstawiony — podstaw i sprawdź
                            if (row.ContainsKey(leftNode.Value))
                            {
                                var substituted = new ASTNode("var", row[leftNode.Value]);
                                if (FindLeftPattern(node, substituted))
                                {
                                    leftOk = true;
                                    matchedLeftValue = row[leftNode.Value];
                                }
                            }
                            else
                            {
                                // Jeśli nie — przetestuj wszystkie możliwe wartości z domeny synonimu
                                var domain = query.Declarations[leftNode.Value].Type.ToUpper() switch
                                {
                                    "VARIABLE" => _pkb.Variables,
                                    "CONSTANT" => _pkb.ConstValues,
                                    _ => new HashSet<string>()
                                };

                                foreach (var candidate in domain)
                                {
                                    var candidateNode = new ASTNode("var", candidate);
                                    if (FindLeftPattern(node, candidateNode))
                                    {
                                        leftOk = true;
                                        matchedLeftValue = candidate;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Literalna zmienna lub "_"
                            leftOk = FindLeftPattern(node, leftNode);
                        }

                        if (!leftOk) continue;

                        var rhs = node.Children.FirstOrDefault(); // prawe wyrażenie (jeśli dotyczy)
                        var rightNode = pattern.AssignAST.Children[0];

                        bool rightOk = pattern.RightClose
                            ? FindRightPatternStrict(rhs, rightNode)
                            : FindRightPattern(rhs, rightNode);

                        if (!rightOk) continue;

                        var newRow = new Dictionary<string, string>(row);
                        newRow[assignSyn] = line;

                        // Dodaj również wartość z lewej strony jeśli to był synonim
                        if (query.Declarations.ContainsKey(leftNode.Value) && !newRow.ContainsKey(leftNode.Value))
                        {
                            newRow[leftNode.Value] = matchedLeftValue;
                        }

                        newRows.Add(newRow);
                    }

                }

                if (newRows.Count == 0)
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };

                rows = newRows;
                PrintResults(rows);
            }

         //   PrintResults(rows);


            foreach (var with in query.WithClauses)
            {
                // Interesują nas tylko porównania dwóch atrybutów
                if (!(with.Left.IsAttributeRef && with.Right.IsAttributeRef))
                    continue;

                var leftSyn = with.Left.Reference;
                var leftAttr = with.Left.Attribute;
                var rightSyn = with.Right.Reference;
                var rightAttr = with.Right.Attribute;

                var newRows = new List<Dictionary<string, string>>();

                foreach (var row in rows)
                {
                    var leftHas = row.TryGetValue(leftSyn, out var leftVal);
                    var rightHas = row.TryGetValue(rightSyn, out var rightVal);

                    if (leftHas && rightHas)
                    {
                        // Sprawdź czy wartości są równe
                        if (leftVal == rightVal)
                            newRows.Add(row);
                    }
                    else if (leftHas && !rightHas)
                    {
                        // Uzupełniamy brakujący prawy, ale tylko jeśli wartość pasuje do typu
                        if (IsValidValueForAttribute(query.Declarations[rightSyn], rightAttr, leftVal))
                        {
                            var newRow = new Dictionary<string, string>(row);
                            newRow[rightSyn] = leftVal;
                            newRows.Add(newRow);
                        }
                    }
                    else if (!leftHas && rightHas)
                    {
                        // Uzupełniamy brakujący lewy, ale tylko jeśli wartość pasuje do typu
                        if (IsValidValueForAttribute(query.Declarations[leftSyn], leftAttr, rightVal))
                        {
                            var newRow = new Dictionary<string, string>(row);
                            newRow[leftSyn] = rightVal;
                            newRows.Add(newRow);
                        }
                    }
                    else if (!leftHas && !rightHas)
                    {
                        var leftDomain = GetDomainForAttribute(query, leftSyn, leftAttr);
                        var rightDomain = GetDomainForAttribute(query, rightSyn, rightAttr);

                        var commonValues = leftDomain.Intersect(rightDomain).ToList();

                        foreach (var val in commonValues)
                        {
                            var newRow = new Dictionary<string, string>(row);
                            newRow[leftSyn] = val;
                            newRow[rightSyn] = val;
                            newRows.Add(newRow);
                        }
                    }

                }

                if (newRows.Count == 0)
                    return isBoolean ? new List<string> { "False" } : new List<string> { "None" };

                rows = newRows;
                PrintResults(rows);
            }




         //  PrintResults(rows);

            if (isBoolean)
            {
                return new List<string> { "True" };
            }

            foreach (var sel in query.Selected)
            {
                string synName = sel.Name;

                if (!query.Declarations.TryGetValue(synName, out var decl))
                    return new List<string> { "None" };

                var domain = decl.Type.ToUpper() switch
                {
                    "STMT" => _pkb.Stmts,
                    "ASSIGN" => _pkb.Assings,
                    "WHILE" => _pkb.Whiles,
                    "IF" => _pkb.Ifs,
                    "VARIABLE" => _pkb.Variables,
                    "PROCEDURE" => _pkb.Procedures,
                    "CONSTANT" => _pkb.ConstValues,
                    "PROG_LINE" => _pkb.Stmts,
                    "CALL" => _pkb.CallStmts,
                    _ => new HashSet<string>()
                };

                if (domain.Count == 0)
                    return new List<string> { "None" };

                // Uzupełnianie rows o brakujące wartości dla synonimu
                if (!rows.All(r => r.ContainsKey(synName)))
                {
                    var newRows = new List<Dictionary<string, string>>();

                    foreach (var row in rows)
                    {
                        if (row.ContainsKey(synName))
                        {
                            newRows.Add(row); // już ma wartość, zostaje
                        }
                        else
                        {
                            foreach (var val in domain)
                            {
                                var newRow = new Dictionary<string, string>(row);
                                newRow[synName] = val;
                                newRows.Add(newRow);
                            }
                        }
                    }

                    rows = newRows;
                }
            }



            return query.Selected.Count == 1
     ? rows.Select(r => r[query.Selected[0].Name]).Distinct().OrderBy(x => x).ToList()
     : rows.Select(r => $"<{string.Join(", ", query.Selected.Select(s => r[s.Name]))}>")
           .Distinct()
           .OrderBy(x => x)
           .ToList();




        }

        private bool IsNodeTypeMatch(string expectedType, string actualType)
        {
            expectedType = expectedType.ToUpper();
            actualType = actualType.ToUpper();

            return expectedType switch
            {
                "ASSIGN" => actualType == "ASSIGN",
                "IF" => actualType == "IF",
                "WHILE" => actualType == "WHILE",
                "CALL" => actualType == "CALL",
                "STMT" => actualType is "ASSIGN" or "IF" or "WHILE" or "CALL" or "PRINT" or "READ",
                "PROG_LINE" => int.TryParse(actualType, out _), // lub true, bo to numer linii
                _ => true // inne typy jak VARIABLE, PROCEDURE nie muszą być sprawdzane tutaj
            };
        }


        private HashSet<string> GetDomainForAttribute(PQLQuery query, string synonym, string attribute)
        {
            if (!query.Declarations.TryGetValue(synonym, out var decl))
                return new HashSet<string>();

            return (attribute, decl.Type.ToUpper()) switch
            {
                // stmt# zależnie od typu synonimu
                ("stmt#", "STMT") => _pkb.Stmts,
                ("stmt#", "ASSIGN") => _pkb.Assings,
                ("stmt#", "WHILE") => _pkb.Whiles,
                ("stmt#", "IF") => _pkb.Ifs,
                ("stmt#", "CALL") => _pkb.CallStmts,

                // value lub varName dla CONSTANT
                ("value", "CONSTANT") or ("varName", "CONSTANT") => _pkb.ConstValues,

                // varName dla VARIABLE
                ("varName", "VARIABLE") => _pkb.Variables,

                // procName dla PROCEDURE
                ("procName", "PROCEDURE") => _pkb.Procedures,

                _ => new HashSet<string>()
            };
        }



        public static List<List<string>> CartesianProduct(List<List<string>> sets)
        {
            var result = new List<List<string>> { new List<string>() };

            foreach (var set in sets)
            {
                var temp = new List<List<string>>();
                foreach (var item in result)
                {
                    foreach (var element in set)
                    {
                        var newList = new List<string>(item) { element };
                        temp.Add(newList);
                    }
                }
                result = temp;
            }

            return result;
        }


        private bool IsValidValueForAttribute(Declaration decl, string attr, string value)
        {
            return decl.Type.ToUpper() switch
            {
                // stmt# dla różnych typów instrukcji
                "STMT" when attr == "stmt#" => _pkb.Stmts.Contains(value),
                "ASSIGN" when attr == "stmt#" => _pkb.Assings.Contains(value),
                "WHILE" when attr == "stmt#" => _pkb.Whiles.Contains(value),
                "IF" when attr == "stmt#" => _pkb.Ifs.Contains(value),
                "CALL" when attr == "stmt#" => _pkb.CallStmts.Contains(value),

                // varName – tylko dla VARIABLE
                "VARIABLE" when attr == "varName" => _pkb.Variables.Contains(value),

                // value – tylko dla CONSTANT
                "CONSTANT" when attr == "value" || attr == "varName" => _pkb.ConstValues.Contains(value),

                // procName – tylko dla PROCEDURE
                "PROCEDURE" when attr == "procName" => _pkb.Procedures.Contains(value),

                _ => false
            };

        }


        private bool IsValueCompatibleWithType(PQLQuery query, string synonym, string value)
        {
            if (!query.Declarations.TryGetValue(synonym, out var decl))
                return false;

            var type = decl.Type.ToUpper();

            // dla instrukcji numerycznych
            if (int.TryParse(value, out var lineNum))
            {
                var node = _pkb.GetNodeByLine(lineNum);
                if (node == null)
                    return false;

                var nodeType = node.Type.ToUpper();

                return type switch
                {
                    "ASSIGN" => nodeType == "ASSIGN",
                    "WHILE" => nodeType == "WHILE",
                    "IF" => nodeType == "IF",
                    "CALL" => nodeType == "CALL",
                    "STMT" => nodeType is "ASSIGN" or "WHILE" or "IF" or "CALL",
                    "PROG_LINE" => true, // dowolna instrukcja
                    _ => false
                };
            }

            // dla nazw
            return type switch
            {
                "VARIABLE" => _pkb.Variables.Contains(value),
                "CONSTANT" => _pkb.ConstValues.Contains(value),
                "PROCEDURE" => _pkb.Procedures.Contains(value),
                _ => false
            };
        }


        private bool CheckParentRelation(string arg1, string arg2)
        {
            return _pkb.IsParent.Contains((arg1, arg2));
        }

        private bool CheckParentStarRelation(string arg1, string arg2)
        {
            return _pkb.IsParentStar.Contains((arg1, arg2));
        }

        private bool CheckFollowsRelation(string arg1, string arg2)
        {
            return _pkb.Follows.TryGetValue(arg1, out var actualBefore) && actualBefore == arg2;
        }

        private bool CheckNextRelation(string arg1, string arg2)
        {
            return _pkb.IsNext.Contains((arg1, arg2));
        }

        private bool CheckNextStarRelation(string arg1, string arg2)
        {
            return _pkb.IsNextStar.Contains((arg1, arg2));
        }

        private bool CheckModifiesRelation(string arg1, string arg2)
        {
            // Sprawdzenie: instrukcja modyfikuje zmienną
            return _pkb.IsModifiesStmtVar.Contains((arg1, arg2)) ||
                   _pkb.IsModifiesProcVar.Contains((arg1, arg2));
                   
        }

        private bool CheckUsesRelation(string arg1, string arg2)
        {
            // Sprawdzenie: instrukcja używa zmiennej
            return _pkb.IsUsesStmtVar.Contains((arg1, arg2)) ||
                   _pkb.IsUsesProcVar.Contains((arg1, arg2)) ||
                   _pkb.IsUsesStmtConst.Contains((arg1, arg2))||
                   _pkb.IsUsesProcConst.Contains((arg1, arg2));
            
        }

        private bool CheckCallsRelation(string arg1, string arg2)
        {
            return _pkb.IsCalls.Contains((arg1.Trim('"'), arg2.Trim('"')));
        }

        private bool CheckCallsStarRelation(string arg1, string arg2)
        {
            return _pkb.IsCallsStar.Contains((arg1.Trim('"'), arg2.Trim('"')));
        }

        private bool CheckFollowsStarRelation(string arg1, string arg2)
        {
            return _pkb.IsFollowsStar.Contains((arg1, arg2));
        }


        private List<string> ReturnEmpty(PQLQuery query)
        {
            if (query.Selected.Count == 1 && query.Selected[0].Name == "BOOLEAN")
            {
                return new List<string> { "False" };
            }
            else
            {
                return new List<string> { "None" };
            }
        }

        // Metoda pomocnicza do generowania wszystkich możliwych krotek
        private void GenerateTuples(int depth, List<string> names, List<HashSet<string>> allValues,
                          string[] current, List<string> results)
        {
            if (depth == names.Count)
            {
                results.Add($"<{string.Join(", ", current)}>");
                return;
            }

            foreach (var value in allValues[depth])
            {
                current[depth] = value;
                GenerateTuples(depth + 1, names, allValues, current, results);
            }
        }


        
        private bool FindLeftPattern(ASTNode pkbAssign, ASTNode queryAssign) {
            if (queryAssign.Value == "_") {
                return true;
            } else {
                if (pkbAssign.Value == queryAssign.Value) {
                    return true;
                } else {
                    return false;
                }
            }
            
        }

        private bool IsValidValueForType(string type, string value)
        {
            var pkb = PKB.GetInstance();
            switch (type.ToUpper())
            {
                case "STMT":
                case "PROG_LINE":
                    return pkb.Stmts.Contains(value);
                case "ASSIGN":
                    return pkb.Assings.Contains(value);
                case "WHILE":
                    return pkb.Whiles.Contains(value);
                case "IF":
                    return pkb.Ifs.Contains(value);
                case "VARIABLE":
                    return pkb.Variables.Contains(value);
                case "PROCEDURE":
                    return pkb.Procedures.Contains(value);
                case "CONSTANT":
                    return pkb.ConstValues.Contains(value);
                default:
                    return false;
            }
        }

        private bool IsSynonym(string arg, PQLQuery query)
        {
            return !arg.StartsWith("\"") && arg != "_" && query.Declarations.ContainsKey(arg);
        }

        private bool FindRightPatternStrict(ASTNode? pkbAssign, ASTNode? queryAssign) {
            // jeśli oba są nullami to są równe
            //queryAssign = queryAssign.Children[0];
            if (queryAssign.Value == "_")
                return true;

            if (pkbAssign == null && queryAssign == null)
                return true;

            // jeśli któryś jest nullem to nie są równe
            if (pkbAssign == null || queryAssign == null)
                return false;

            // jeśli wartość lub typ się nie zgadza to nie są równe
            if (pkbAssign.Value != queryAssign.Value || pkbAssign.Type != queryAssign.Type)
                return false;

            // pobieramy liczbe kolejnych węzłów 
            int pkbChildrenCount = pkbAssign.Children.Count;
            int queryChildrenCount = queryAssign.Children.Count;

            // jeśli liczba kolejnych węzłów nie jest równa to nie są równe assigny
            if (pkbChildrenCount != queryChildrenCount)
                return false;

            // jeśli nie mają dzieci to są równe
            if (pkbChildrenCount == 0 && queryChildrenCount == 0)
                return true;

            // jeśli mają dziecko to analizujemy dalsze (wchodzimy do węzłów rekursywnie)
            if (pkbChildrenCount == 1 && queryChildrenCount == 1)
                return FindRightPatternStrict(pkbAssign.Children[0], queryAssign.Children[0]);

            // jeśli mają dzieci to analizujemy w lewym i prawym węźle
            if (pkbChildrenCount == 2 && queryChildrenCount == 2)
                return FindRightPatternStrict(pkbAssign.Children[0], queryAssign.Children[0]) &&
                       FindRightPatternStrict(pkbAssign.Children[1], queryAssign.Children[1]);

            return false;
        }

        private bool FindRightPattern(ASTNode pkbAssign, ASTNode queryAssign) {
            if (FindRightPatternStrict(pkbAssign, queryAssign))
                return true;

            // Przeszukaj dzieci rekurencyjnie
            foreach (var child in pkbAssign.Children)
            {
                if (FindRightPattern(child, queryAssign))
                    return true;
            }

            return false;
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