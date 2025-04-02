using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA_main
{
    class Parser
    {
        private readonly List<Token> _tokens;
        private int _index = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token CurrentToken => _index < _tokens.Count ? _tokens[_index] : null;
        private void Eat(string type)
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
        private ASTNode ParseProcedure()
        {
            Eat("PROCEDURE");
            string procName = CurrentToken.Value;
            Eat("NAME");
            Eat("LBRACE");
            ASTNode stmtLst = ParseStmtLst();
            Eat("RBRACE");
            var procNode = new ASTNode("procedure", procName);
            procNode.AddChild(stmtLst);
            return procNode;
        }

        private ASTNode ParseStmtLst()
        {
            var node = new ASTNode("stmtLst");
            while (CurrentToken != null && CurrentToken.Type != "RBRACE")
                node.AddChild(ParseStmt());
            return node;
        }

        private ASTNode ParseStmt()
        {
            if (CurrentToken.Type == "WHILE")
                return ParseWhile();
            else if (CurrentToken.Type == "NAME")
                return ParseAssign();
            else
                throw new Exception($"Unexpected statement: {CurrentToken}");
        }

        private ASTNode ParseWhile()
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

        private ASTNode ParseAssign()
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

        private ASTNode ParseExpr()
        {
            var node = new ASTNode("expr");

            if (CurrentToken.Type == "NAME")
            {
                string term = CurrentToken.Value;
                Eat("NAME");
                node.AddChild(new ASTNode("var", term));
            }
            else if (CurrentToken.Type == "NUMBER")
            {
                string term = CurrentToken.Value;
                Eat("NUMBER");
                node.AddChild(new ASTNode("const", term));
            }
            else
            {
                throw new Exception($"Unexpected token in expression: {CurrentToken}");
            }

            while (CurrentToken != null && CurrentToken.Type == "PLUS")
            {
                Eat("PLUS");

                if (CurrentToken.Type == "NAME")
                {
                    string term = CurrentToken.Value;
                    Eat("NAME");
                    node.AddChild(new ASTNode("var", term));
                }
                else if (CurrentToken.Type == "NUMBER")
                {
                    string term = CurrentToken.Value;
                    Eat("NUMBER");
                    node.AddChild(new ASTNode("const", term));
                }
                else
                {
                    throw new Exception($"Unexpected token after PLUS: {CurrentToken}");
                }
            }

            return node;
        }
    }
}
