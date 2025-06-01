using SPA_main;
using System.Xml.Linq;

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

        #region Follows Relation
        public Dictionary<string, string> Follows { get; private set; }
        public HashSet<(string, string)> IsFollowsStar { get; private set; }
        #endregion

        #region Parent Relation
        public Dictionary<string, List<string>> Parent { get; private set; }
        public HashSet<(string Parent, string Child)> IsParent { get; private set; }
        public HashSet<(string Parent, string Child)> IsParentStar { get; private set; }
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
        public HashSet<(string Stmt, string Const)> IsUsesStmtConst { get; private set; }
        public HashSet<(string Stmt, string Const)> IsUsesProcConst { get; private set; }
        #endregion

        #region Calls Relation
        public Dictionary<string, List<string>> Calls { get; private set; }
        public HashSet<(string Caller, string Callee)> IsCalls { get; private set; }
        public HashSet<(string Caller, string Callee)> IsCallsStar { get; private set; }
        public Dictionary<string, List<string>> Called { get; private set; }
        public Dictionary<string, List<string>> CallsStar { get; private set; }
        public Dictionary<string, List<string>> CalledStar { get; private set; }

        #endregion

        #region Next Relation
        public Dictionary<string, List<string>> Next { get; private set; }
        public HashSet<(string, string)> IsNext { get; private set; }
        #endregion

        #region NextStar Relation
        public Dictionary<string, List<string>> NextStar { get; private set; }
        public HashSet<(string, string)> IsNextStar { get; private set; }
        #endregion

        #region Assigns
        public Dictionary<string, List<ASTNode>> Assign { get; private set; }
        #endregion

        #region Helpers
        public HashSet<string> ConstValues { get; private set; }


        public HashSet<string> Assings { get; private set; }
        public HashSet<string> Whiles { get; private set; }
        public HashSet<string> Ifs { get; private set; }
        public HashSet<string> Variables { get; private set; }
        public HashSet<string> Procedures { get; private set; }
        public HashSet<string> Stmts { get; private set; }
        public HashSet<string> CallStmts { get; private set; }
        #endregion
        #region Constructor
        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// </summary>
        private PKB() {
            LineToNode = new Dictionary<int, ASTNode>();
            Follows = new Dictionary<string, string>();

            Parent = new Dictionary<string, List<string>>();
            IsParent = new HashSet<(string Parent, string Child)>();
            IsParentStar = new HashSet<(string Parent, string Child)>();
            ModifiesStmt = new Dictionary<string, List<string>>();
            ModifiesVar = new Dictionary<string, List<string>>();
            IsModifiesStmtVar = new HashSet<(string Stmt, string Var)>();
            IsModifiesProcVar = new HashSet<(string Proc, string Var)>();

            UsesStmt = new Dictionary<string, List<string>>();
            UsesVar = new Dictionary<string, List<string>>();
            IsUsesStmtVar = new HashSet<(string Stmt, string Var)>();
            IsUsesProcVar = new HashSet<(string Proc, string Var)>();
            IsUsesStmtConst= new HashSet<(string Stmt, string Const)>();
            IsUsesProcConst = new HashSet<(string Stmt, string Const)>();
            Calls = new Dictionary<string, List<string>>();
            Called = new Dictionary<string, List<string>>();
            IsCalls = new HashSet<(string, string)>();
            IsCallsStar = new HashSet<(string, string)>();
            CallsStar = new Dictionary<string, List<string>>();
            CalledStar = new Dictionary<string, List<string>>();

            Next = new Dictionary<string, List<string>>();
            IsNext = new HashSet<(string, string)>();

            NextStar = new Dictionary<string, List<string>>();
            IsNextStar = new HashSet<(string, string)>();

            Assign = new Dictionary<string, List<ASTNode>>();
            CallStmts = new HashSet<string>();
            Stmts = new HashSet<string>();
            ConstValues = new HashSet<string>();
            Assings = new HashSet<string>();
            Whiles = new HashSet<string>();
            Ifs = new HashSet<string>();
            Variables = new HashSet<string>();
            Procedures = new HashSet<string>();
            Stmts = new HashSet<string>();
        }
        #endregion

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
            IndexTree( Root );
            PopulateRelations();

        }
        public void PopulateRelations()
        {
            PopulateFollows(Root);
            PopulateParent(Root);
            PopulateModifiesAndUses(Root);
            PopulateCalls(Root);
            PropagateModifiesAndUsesOverCalls();
            RecomputeWhileIfModifiesAndUses();
            PopulateNext(Root);
            ComputeNextStar();
            PopulateAssign(Root);
            ComputeFollowsStar();
            ComputeParentStar();
        }
        private void ComputeFollowsStar()
        {
            IsFollowsStar = new HashSet<(string, string)>();

            foreach (var stmt in Follows.Keys)
            {
                string current = stmt;
                while (Follows.ContainsKey(current))
                {
                    var next = Follows[current];
                    IsFollowsStar.Add((stmt, next));
                    current = next;
                }
            }
        }

        public void ComputeParentStar()
        {
            IsParentStar.Clear();

            // Budujemy mapę: rodzic -> lista dzieci
            var parentToChildren = new Dictionary<string, List<string>>();
            foreach (var (parent, child) in IsParent)
            {
                if (!parentToChildren.ContainsKey(parent))
                    parentToChildren[parent] = new List<string>();

                parentToChildren[parent].Add(child);
            }

            // Dla każdego możliwego rodzica uruchamiamy DFS
            foreach (var parent in parentToChildren.Keys)
            {
                var visited = new HashSet<string>();
                DFS(parent, parent, parentToChildren, visited);
            }
        }

        private void DFS(string originalParent, string current, Dictionary<string, List<string>> map, HashSet<string> visited)
        {
            if (!map.ContainsKey(current)) return;

            foreach (var child in map[current])
            {
                if (visited.Contains(child)) continue;

                IsParentStar.Add((originalParent, child));
                visited.Add(child);
                DFS(originalParent, child, map, visited);
            }
        }

        private void PopulateAssign(ASTNode node) {
            if (node == null) return;

            foreach (var child in node.Children) {
                PopulateAssign(child);
            }

            foreach (var child in node.Children) {
                if (child.Type == "assign" && child.LineNumber.HasValue) {
                    string key = child.LineNumber.Value.ToString();

                    if (!Assign.ContainsKey(key)) {
                        Assign[key] = new List<ASTNode>();
                    }

                    Assign[key].Add(child);
                }
            }
        }
        private void RecomputeWhileIfModifiesAndUses()
        {
            // PRZEJDŹ WSZYSTKIE STMTY (w PKB masz je w Stmts)
            foreach (var stmt in Stmts)
            {
                if (LineToNode.TryGetValue(int.Parse(stmt), out var node))
                {
                    if (node.Type == "while" || node.Type == "if")
                    {
                        // ZBIERZ Z CIAŁA
                        var modVars = GetAllModifiedVariables(node);
                        foreach (var varName in modVars)
                            AddModifies(stmt, varName);

                        var useVars = GetAllUsedVariables(node);
                        foreach (var varName in useVars)
                            AddUses(stmt, varName);
                    }
                }
            }
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
                    AddModifies(stmt, node.Value);

                    var exprNode = node.Children.FirstOrDefault();
                    foreach (var varName in GetVariablesFromExpression(exprNode))
                        AddUses(stmt, varName);
                    foreach (var constName in GetConstatntsFromExpression(exprNode))
                        IsUsesStmtConst.Add((stmt, constName));
                }
                else if (node.Type == "while" || node.Type == "if")
                {
                    AddUses(stmt, node.Value);
                    foreach (var varName in GetAllModifiedVariables(node))
                        AddModifies(stmt, varName);
                    foreach (var varName in GetAllUsedVariables(node))
                        AddUses(stmt, varName);
                }
                // call nadal nieobsługiwany
            }

            // --- propagowanie z instrukcji do procedury ---
            if (node.Type == "procedure")
            {
                string procName = node.Value;

                var stmtsInProc = new HashSet<string>();
                CollectStatementsInProcedure(node, stmtsInProc);

                foreach (var stmt in stmtsInProc)
                {
                    if (ModifiesStmt.TryGetValue(stmt, out var modifiedVars))
                    {
                        foreach (var varName in modifiedVars)
                            IsModifiesProcVar.Add((procName, varName));
                    }

                    if (UsesStmt.TryGetValue(stmt, out var usedVars))
                    {
                        foreach (var varName in usedVars)
                            IsUsesProcVar.Add((procName, varName));
                    }

                    foreach (var (s, constName) in IsUsesStmtConst)
                    {
                        if (s == stmt)
                            IsUsesProcConst.Add((procName, constName));
                    }
                }
            }
        }


        private HashSet<string> GetAllUsedVariables(ASTNode node)
        {
            HashSet<string> usedVars = new HashSet<string>();

            if (node == null)
                return usedVars;

            if (node.LineNumber.HasValue)
            {
                string stmt = node.LineNumber.Value.ToString();
                if (UsesStmt.ContainsKey(stmt))
                {
                    foreach (var varName in UsesStmt[stmt])
                        usedVars.Add(varName);
                }
            }

            foreach (var child in node.Children)
            {
                var childVars = GetAllUsedVariables(child);
                foreach (var varName in childVars)
                    usedVars.Add(varName);
            }

            return usedVars;
        }
        private void PropagateModifiesAndUsesOverCalls()
        {
            bool changed;

            do
            {
                changed = false;

                foreach (var (caller, callees) in Calls)
                {
                    foreach (var callee in callees)
                    {
                        // MODIFIES propagation
                        foreach (var (p, v) in IsModifiesProcVar.ToList())
                        {
                            if (p == callee && !IsModifiesProcVar.Contains((caller, v)) && !int.TryParse(caller, out _))
                            {
                                IsModifiesProcVar.Add((caller, v));
                                changed = true;
                            }
                        }

                        // USES propagation (variables)
                        foreach (var (p, v) in IsUsesProcVar.ToList())
                        {
                            if (p == callee && !IsUsesProcVar.Contains((caller, v)) && !int.TryParse(caller, out _))
                            {
                                IsUsesProcVar.Add((caller, v));
                                changed = true;
                            }
                        }

                        // USES propagation (constants)
                        foreach (var (p, c) in IsUsesProcConst.ToList())
                        {
                            if (p == callee && !IsUsesProcConst.Contains((caller, c)) && !int.TryParse(caller, out _))
                            {
                                IsUsesProcConst.Add((caller, c));
                                changed = true;
                            }
                        }

                    }
                }

            } while (changed);
        
            // Dodaj propagację na statementy call (zawsze PO poprzedniej fazie!)
            foreach (var node in LineToNode.Values)
            {
                if (node.Type == "call")
                {
                    string stmt = node.LineNumber.Value.ToString();
                    string procName = node.Value;
                    foreach (var (p, v) in IsModifiesProcVar)
                        if (p == procName)
                            AddModifies(stmt, v);
                    foreach (var (p, v) in IsUsesProcVar)
                        if (p == procName)
                            AddUses(stmt, v);
                }
            }

            // Teraz musisz jeszcze raz zebrać Modifies/Uses dla PROCEDUR,
            // bo nowe statementy call mogły mieć nowe modyfikacje/uses:
            foreach (var procName in Procedures)
            {
                // Znajdź węzeł procedury o tej nazwie
                var procNode = LineToNode.Values.FirstOrDefault(n => n.Type == "procedure" && n.Value == procName);
                if (procNode == null) continue;

                // Zbierz wszystkie statementy w tej procedurze
                var stmtsInProc = new HashSet<string>();
                CollectStatementsInProcedure(procNode, stmtsInProc);

                foreach (var stmt in stmtsInProc)
                {
                    if (ModifiesStmt.TryGetValue(stmt, out var modifiedVars))
                    {
                        foreach (var varName in modifiedVars)
                            IsModifiesProcVar.Add((procName, varName));
                    }

                    if (UsesStmt.TryGetValue(stmt, out var usedVars))
                    {
                        foreach (var varName in usedVars)
                            IsUsesProcVar.Add((procName, varName));
                    }

                    foreach (var (s, constName) in IsUsesStmtConst.Where(p => p.Stmt == stmt))
                    {
                        IsUsesProcConst.Add((procName, constName));
                    }
                }
            }

        }

        private void PopulateNext(ASTNode node)
        {
            if (node == null) return;

            if (node.Type == "procedure")
            {
                var stmtLst = node.Children.FirstOrDefault(c => c.Type == "stmtLst");
                if (stmtLst != null)
                    BuildNextFromStmtLst(stmtLst, null);
            }

            foreach (var child in node.Children)
                PopulateNext(child);
        }

        private void BuildNextFromStmtLst(ASTNode stmtLst, ASTNode followUp)
        {
            for (int i = 0; i < stmtLst.Children.Count; i++)
            {
                var current = stmtLst.Children[i];
                ASTNode next = (i + 1 < stmtLst.Children.Count) ? stmtLst.Children[i + 1] : followUp;

                if (current.Type == "assign" || current.Type == "call")
                {
                    if (next != null)
                        AddNextRelation(current.LineNumber.Value.ToString(), next.LineNumber.Value.ToString());
                }
                else if (current.Type == "while")
                {
                    var whileHeader = current;
                    var whileBody = current.Children.FirstOrDefault(); // stmtLst

                    if (whileBody != null && whileBody.Children.Any())
                    {
                        // 1. while -> first in body
                        AddNextRelation(whileHeader.LineNumber.Value.ToString(),
                            whileBody.Children.First().LineNumber.Value.ToString());

                        // 2. last in body -> while
                        var lastInBody = whileBody.Children.Last();
                        AddNextRelation(lastInBody.LineNumber.Value.ToString(),
                            whileHeader.LineNumber.Value.ToString());

                        // Rekurencja wewnątrz while
                        BuildNextFromStmtLst(whileBody, whileHeader);
                    }

                    // 3. while -> instrukcja po pętli
                    if (next != null)
                    {
                        AddNextRelation(whileHeader.LineNumber.Value.ToString(), next.LineNumber.Value.ToString());
                    }
                }
                else if (current.Type == "if")
                {
                    var ifHeader = current;
                    var thenBlock = ifHeader.Children[0];
                    var elseBlock = ifHeader.Children[1];

                    if (thenBlock.Children.Any())
                        AddNextRelation(ifHeader.LineNumber.Value.ToString(),
                            thenBlock.Children.First().LineNumber.Value.ToString());

                    if (elseBlock.Children.Any())
                        AddNextRelation(ifHeader.LineNumber.Value.ToString(),
                            elseBlock.Children.First().LineNumber.Value.ToString());

                    // Po THEN i ELSE przechodzimy dalej
                    if (next != null)
                    {
                        var lastThen = thenBlock.Children.LastOrDefault();
                        var lastElse = elseBlock.Children.LastOrDefault();

                        if (lastThen != null)
                            AddNextRelation(lastThen.LineNumber.Value.ToString(), next.LineNumber.Value.ToString());

                        if (lastElse != null)
                            AddNextRelation(lastElse.LineNumber.Value.ToString(), next.LineNumber.Value.ToString());
                    }

                    // Rekurencja
                    BuildNextFromStmtLst(thenBlock, next);
                    BuildNextFromStmtLst(elseBlock, next);
                }
            }
        }

        private void AddNextRelation(string from, string to)
        {
            if (!Next.ContainsKey(from))
                Next[from] = new List<string>();

            if (!Next[from].Contains(to))
                Next[from].Add(to);

            IsNext.Add((from, to));
        }

        private void ComputeNextStar()
        {
            foreach (var entry in Next)
            {
                var visited = new HashSet<string>();
                var stack = new Stack<string>();
                stack.Push(entry.Key);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (!visited.Contains(current))
                    {
                        visited.Add(current);

                        if (Next.ContainsKey(current))
                        {
                            foreach (var neighbor in Next[current])
                            {
                                stack.Push(neighbor);

                                // Dodaj relację Next*(entry.Key, neighbor)
                                AddNextStarRelation(entry.Key, neighbor);
                            }
                        }
                    }
                }
            }
        }

        private void AddNextStarRelation(string from, string to)
        {
            if (!NextStar.ContainsKey(from))
                NextStar[from] = new List<string>();

            if (!NextStar[from].Contains(to))
                NextStar[from].Add(to);

            IsNextStar.Add((from, to));
        }

        private IEnumerable<string> GetConstatntsFromExpression(ASTNode exprNode)
        {
            List<string> consts = new List<string>();
            if (exprNode == null) return consts;

            if (exprNode.Type == "const")
            {
                consts.Add(exprNode.Value);
            }

            foreach (var child in exprNode.Children)
            {
                consts.AddRange(GetConstatntsFromExpression(child));
            }

            return consts;

        }

        private void CollectStatementsInProcedure(ASTNode node, HashSet<string> stmts)
        {
            if (node == null) return;

            if (node.LineNumber.HasValue)
            {
                stmts.Add(node.LineNumber.Value.ToString());
            }

            foreach (var child in node.Children)
            {
                CollectStatementsInProcedure(child, stmts);
            }
        }
        private void PopulateCalls(ASTNode node)
        {
            if (node == null) return;

            foreach (var child in node.Children)
                PopulateCalls(child);

            if (node.Type == "procedure")
            {
                string procName = node.Value;
                foreach (var stmt in node.Children)
                {
                    TraverseStatementsForCalls(procName, stmt);
                }
            }
            ComputeCallsStar();
        }
        private void PopulateConst(ASTNode node)
        {
            if (node == null) return;
            
            foreach(var child in node.Children)
            {
                PopulateConst(child);
            }
        }
        private void ComputeCallsStar()
        {
            // First add all direct calls to Calls*
            foreach (var call in IsCalls)
            {
                IsCallsStar.Add(call);
            }
            foreach (var caller in Calls.Keys)
            {
                CallsStar[caller] = new List<string>(Calls[caller]);
            }

            // Then compute the transitive closure using Floyd-Warshall algorithm
            bool changed;
            do
            {
                changed = false;
                

                foreach (var caller in CallsStar.Keys.ToList()) 
                {
                    foreach (var intermediate in CallsStar[caller].ToList())
                    {
                        if (CallsStar.ContainsKey(intermediate))
                        {
                            foreach (var callee in CallsStar[intermediate])
                            {
                                if (!CallsStar[caller].Contains(callee))
                                {
                                    CallsStar[caller].Add(callee);
                                    IsCallsStar.Add((caller, callee));
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            } while (changed);
            foreach (var caller in CallsStar.Keys)
            {
                foreach (var callee in CallsStar[caller])
                {
                    if (!CalledStar.ContainsKey(callee))
                        CalledStar[callee] = new List<string>();

                    if (!CalledStar[callee].Contains(caller))
                        CalledStar[callee].Add(caller);
                }
            }
        }

        private void TraverseStatementsForCalls(string callerProc, ASTNode node)
        {
            if (node == null) return;

            if (node.Type == "call")
            {
                string calleeProc = node.Value;
                AddCallRelation(callerProc, calleeProc);
                AddCallRelation(node.LineNumber.ToString(), calleeProc);
            }

            foreach (var child in node.Children)
            {
                TraverseStatementsForCalls(callerProc, child);
            }
        }

        private void AddCallRelation(string caller, string callee)
        {
            if (!Calls.ContainsKey(caller))
                Calls[caller] = new List<string>();

            if (!Calls[caller].Contains(callee))
                Calls[caller].Add(callee);

            if (!Called.ContainsKey(callee))
                Called[callee] = new List<string>();

            if (!Called[callee].Contains(caller))
                Called[callee].Add(caller);
            IsCalls.Add((caller, callee));
        }



        private HashSet<string> GetAllModifiedVariables(ASTNode node)
        {
            HashSet<string> modifiedVars = new HashSet<string>();

            if (node == null)
                return modifiedVars;

            if (node.LineNumber.HasValue)
            {
                string stmt = node.LineNumber.Value.ToString();

                // Dodaj modyfikowane bezpośrednio przez statement
                if (ModifiesStmt.ContainsKey(stmt))
                {
                    foreach (var varName in ModifiesStmt[stmt])
                        modifiedVars.Add(varName);
                }

                // Jeżeli to jest call, dorzuć Modifies z procedury wywoływanej (zawsze!)
                if (node.Type == "call")
                {
                    string procName = node.Value;
                    foreach (var (p, v) in IsModifiesProcVar)
                        if (p == procName)
                            modifiedVars.Add(v);
                }
            }

            // Rekurencyjnie dzieci
            foreach (var child in node.Children)
            {
                var childVars = GetAllModifiedVariables(child);
                foreach (var varName in childVars)
                    modifiedVars.Add(varName);
            }

            return modifiedVars;
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
        private void IndexTree(ASTNode node)
        {
            if (node == null)
                return;

            if (node.LineNumber.HasValue)
            {
                int lineNum = node.LineNumber.Value;
                

                if (!LineToNode.ContainsKey(lineNum))
                {
                    LineToNode[lineNum] = node;
                    Stmts.Add(lineNum.ToString());
                }
                    


                switch (node.Type)
                {
                    case "assign":
                        Assings.Add(lineNum.ToString());
                        break;
                    case "while":
                        Whiles.Add(lineNum.ToString());
                        break;
                    case "if":
                        Ifs.Add(lineNum.ToString());
                        break;
                    case "call":
                        CallStmts.Add(lineNum.ToString());
                        break;

                }
            }
            switch (node.Type)
            {
                case "var":
                    Variables.Add(node.Value);
                    break;
                case "assign":
                    Variables.Add(node.Value);
                    break;
                case "procedure":
                    Procedures.Add(node.Value);
                    break;
                case "const":
                    ConstValues.Add(node.Value);
                    break;
                case "call":
                  //  CallStmts.Add(node.Value);
                    break;

            }
            foreach (var child in node.Children)
            {
                IndexTree(child);
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
