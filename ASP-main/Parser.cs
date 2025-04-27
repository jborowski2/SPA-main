using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA_main
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _index = 0;
        private string _actualProcName;
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token CurrentToken => _index < _tokens.Count ? _tokens[_index] : null;
        public void Eat(string type)
        {
            if (CurrentToken != null && CurrentToken.Type == type)
                _index++;
            else
                throw new Exception($"Unexpected token: {CurrentToken}");
        }

        public ASTNode ParseProgram()
        {
            var node = new ASTNode("program");
            while (CurrentToken != null)
                node.AddChild(ParseProcedure());
            return node;
        }
        public ASTNode ParseProcedure()
        {
            Eat("PROCEDURE");
            string procName = CurrentToken.Value;
            _actualProcName = procName;
            Eat("NAME");
            Eat("LBRACE");
            ASTNode stmtLst = ParseStmtLst();
            Eat("RBRACE");

            var procNode = new ASTNode("procedure", procName);
            procNode.AddChild(stmtLst);
            return procNode;
        }

        public ASTNode ParseStmtLst()
        {
            var node = new ASTNode("stmtLst");
            node.ProcName = _actualProcName;
            ASTNode previousStmt = null;
            while (CurrentToken != null && CurrentToken.Type != "RBRACE")
            {
                var stmt = ParseStmt();
                node.AddChild(stmt);
                if (previousStmt != null)
                {
                    previousStmt.SetFollows(stmt);
                }
                previousStmt = stmt;
            }
            return node;
        }

        public ASTNode ParseStmt()
        {
            if (CurrentToken.Type == "WHILE")
                return ParseWhile();
            else if (CurrentToken.Type == "IF")
                return ParseIf();
            else if (CurrentToken.Type == "CALL")
                return ParseCall();

            else if (CurrentToken.Type == "NAME")
                return ParseAssign();
            else
                throw new Exception($"Unexpected statement: {CurrentToken}");
        }
        public ASTNode ParseIf()
        {
            int lineNumber = CurrentToken.LineNumber;
            Eat("IF");
            string varName = CurrentToken.Value;
            Eat("NAME");
            Eat("THEN");
            Eat("LBRACE");
            ASTNode thenStmtLst = ParseStmtLst();
            Eat("RBRACE");
            Eat("ELSE");
            Eat("LBRACE");
            ASTNode elseStmtLst = ParseStmtLst();
            Eat("RBRACE");

            var ifNode = new ASTNode("if", varName, lineNumber);
            ifNode.AddChild(thenStmtLst);
            ifNode.AddChild(elseStmtLst);
            return ifNode;
        }
        public ASTNode ParseWhile()
        {
            int lineNumber = CurrentToken.LineNumber;
            Eat("WHILE");
            string varName = CurrentToken.Value;
            Eat("NAME");
            Eat("LBRACE");
            ASTNode stmtLst = ParseStmtLst();
            Eat("RBRACE");
            var whileNode = new ASTNode("while", varName, lineNumber);
            whileNode.AddChild(stmtLst);
            return whileNode;
        }

        public ASTNode ParseAssign()
        {
            int lineNumber = CurrentToken.LineNumber;
            string varName = CurrentToken.Value;
            Eat("NAME");
            Eat("ASSIGN");
            ASTNode exprNode = ParseExpr();
            Eat("SEMICOLON");
            var assignNode = new ASTNode("assign", varName, lineNumber);
            assignNode.AddChild(exprNode);
            return assignNode;
        }

        public ASTNode ParseExpr()
        {
            var node = ParseTerm();

            while (CurrentToken != null && (CurrentToken.Type == "PLUS" || CurrentToken.Type == "MINUS"))
            {
                string op = CurrentToken.Type;
                Eat(op);
                var right = ParseTerm();
                var opNode = new ASTNode(op == "PLUS" ? "plus_op" : "minus_op", op);
                opNode.AddChild(node);
                opNode.AddChild(right);
                node = opNode;
            }

            return node;
        }
        public ASTNode ParseTerm()
        {
            var node = ParseFactor();

            while (CurrentToken != null && CurrentToken.Type == "MULTIPLY")
            {
                Eat("MULTIPLY");
                var right = ParseFactor();
                var opNode = new ASTNode("multiply_op", "*");
                opNode.AddChild(node);
                opNode.AddChild(right);
                node = opNode;
            }

            return node;
        }

        public ASTNode ParseFactor()
        {
            if (CurrentToken.Type == "NAME")
            {
                string term = CurrentToken.Value;
                Eat("NAME");
                return new ASTNode("var", term);
            }
            else if (CurrentToken.Type == "NUMBER")
            {
                string term = CurrentToken.Value;
                Eat("NUMBER");
                return new ASTNode("const", term);
            }
            else if (CurrentToken.Type == "LBRACE")
            {
                Eat("LBRACE");
                var node = ParseExpr();
                Eat("RBRACE");
                return node;
            }
            else
            {
                throw new Exception($"Unexpected token in factor: {CurrentToken}");
            }
        }
        public ASTNode ParseCall()
        {
            int lineNumber = CurrentToken.LineNumber;
            Eat("CALL");
            string procName = CurrentToken.Value;
            Eat("NAME");
            Eat("SEMICOLON");
            return new ASTNode("call", procName, lineNumber);
        }
    }
}
