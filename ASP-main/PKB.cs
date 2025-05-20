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
        public HashSet<(string Stmt, string Const)> IsUsesStmtConst { get; private set; }
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

            ModifiesStmt = new Dictionary<string, List<string>>();
            ModifiesVar = new Dictionary<string, List<string>>();
            IsModifiesStmtVar = new HashSet<(string Stmt, string Var)>();
            IsModifiesProcVar = new HashSet<(string Proc, string Var)>();

            UsesStmt = new Dictionary<string, List<string>>();
            UsesVar = new Dictionary<string, List<string>>();
            IsUsesStmtVar = new HashSet<(string Stmt, string Var)>();
            IsUsesProcVar = new HashSet<(string Proc, string Var)>();
            IsUsesStmtConst= new HashSet<(string Stmt, string Const)>();
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
            PopulateNext(Root);
            ComputeNextStar();
            PopulateAssign(Root);
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

            // Najpierw przetwarzamy dzieci
            foreach (var child in node.Children)
                PopulateModifiesAndUses(child);

            if (node.Type == "procedure")
            {
                string procName = node.Value;

                // Krok 1: Zbierz bezpośrednie Modifies/Uses z instrukcji w tej procedurze
                var stmtsInProc = new HashSet<string>();
                CollectStatementsInProcedure(node, stmtsInProc);

                foreach (var stmt in stmtsInProc)
                {
                    // Modifies
                    if (ModifiesStmt.ContainsKey(stmt))
                        foreach (var varName in ModifiesStmt[stmt])
                            IsModifiesProcVar.Add((procName, varName));

                    // Uses
                    if (UsesStmt.ContainsKey(stmt))
                        foreach (var varName in UsesStmt[stmt])
                            IsUsesProcVar.Add((procName, varName));
                }

                // Krok 2: Dodaj Modifies/Uses z wywoływanych procedur (z przechodniością)
                if (Calls.ContainsKey(procName))
                {
                    var processed = new HashSet<string>();
                    var queue = new Queue<string>(Calls[procName]);

                    while (queue.Count > 0)
                    {
                        var callee = queue.Dequeue();
                        if (processed.Contains(callee)) continue;
                        processed.Add(callee);

                        // Dodaj Modifies/Uses wywoływanej procedury
                        foreach (var (p, v) in IsModifiesProcVar)
                            if (p == callee) IsModifiesProcVar.Add((procName, v));

                        foreach (var (p, v) in IsUsesProcVar)
                            if (p == callee) IsUsesProcVar.Add((procName, v));

                        // Dodaj wywołania zagnieżdżone (ale bez rekurencji)
                        if (Calls.ContainsKey(callee))
                            foreach (var newCallee in Calls[callee])
                                if (!processed.Contains(newCallee))
                                    queue.Enqueue(newCallee);
                    }
                }
            }
            else if (node.LineNumber.HasValue)
            {
                string stmt = node.LineNumber.Value.ToString();
               
                if (node.Type == "assign")
                {
                    // Modifies: lewa strona
                    AddModifies(stmt, node.Value);

                    var exprNode = node.Children.FirstOrDefault();
                    // Uses: prawa strona
                    foreach (var varName in GetVariablesFromExpression(exprNode))
                        AddUses(stmt, varName);
                    foreach (var constName in GetConstatntsFromExpression(exprNode))
                        IsUsesStmtConst.Add((stmt, constName));
                }
                else if (node.Type == "while" || node.Type == "if")
                {
                    // Uses: zmienna sterująca
                    AddUses(stmt, node.Value);

                    // Modifies: wszystko w ciele
                    foreach (var varName in GetAllModifiedVariables(node))
                        AddModifies(stmt, varName);
                }
                else if (node.Type == "call")
                {
                    // Modifies/Uses takie jak wywoływana procedura
                    string procName = node.Value;
                    foreach (var (p, v) in IsModifiesProcVar)
                        if (p == procName) AddModifies(stmt, v);

                    foreach (var (p, v) in IsUsesProcVar)
                        if (p == procName) AddUses(stmt, v);
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

                if (ModifiesStmt.ContainsKey(stmt))
                {
                    foreach (var varName in ModifiesStmt[stmt])
                        modifiedVars.Add(varName);
                }
            }

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
