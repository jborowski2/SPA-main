using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main
{
    class PQLParser
    {
        private readonly List<Token> _tokens;
        private int _index = 0;

        public PQLParser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token CurrentToken => _index < _tokens.Count ? _tokens[_index] : null;
        private Token NextToken => _index + 1 < _tokens.Count ? _tokens[_index + 1] : null;
        private void Eat(string type)
        {
            if (CurrentToken != null && string.Equals(CurrentToken.Type, type, StringComparison.OrdinalIgnoreCase))
                _index++;
            else
                throw new Exception($"Unexpected token: {CurrentToken}");
        }

        public PQLQuery ParseQuery()
        {
            var query = new PQLQuery();

            // Parse declaration
            while (CurrentToken != null && !string.Equals(CurrentToken.Type, "SELECT", StringComparison.OrdinalIgnoreCase))
            {
                var decl = ParseDeclaration();
                query.Declarations.Add(decl);
            }

            // Parse select
            Eat("SELECT");
            query.Selected = ParseSelected();

            // Parse such that
            while (CurrentToken != null && string.Equals(CurrentToken.Type, "SUCH_THAT", StringComparison.OrdinalIgnoreCase))
            {
                Eat("SUCH_THAT");
                var relation = ParseRelation();
                query.Relations.Add(relation);
            }

            return query;
        }

        private Declaration ParseDeclaration()
        {
            var type = CurrentToken.Value;
            Eat(type.ToUpper()); // e.g., STMT, VARIABLE, etc.
            var name = CurrentToken.Value;
            Eat("NAME");
            Eat("SEMICOLON");
            return new Declaration(type, name);
        }

        private Selected ParseSelected()
        {
            if (CurrentToken.Type == "NAME")
            {
                var name = CurrentToken.Value;
                Eat("NAME");
                return new Selected(name);
            }
            else
            {
                throw new Exception($"Unexpected token in selected: {CurrentToken}");
            }
        }

        private Relation ParseRelation()
        {
            var relationType = CurrentToken.Value;
            Eat(relationType.ToUpper()); // e.g., MODIFIES, USES, etc.
            Eat("LPAREN");

            var arg1 = CurrentToken.Value;
            Eat(CurrentToken.Type); // Could be NAME or NUMBER

            Eat("COMMA");

            var arg2 = CurrentToken.Value;
            if (CurrentToken.Type == "QUOTE")
            {
                Eat("QUOTE");
                arg2 = CurrentToken.Value;
                Eat("NAME");
                Eat("QUOTE");
            }
            else
            {
                Eat(CurrentToken.Type);
            }

            Eat("RPAREN");

            return new Relation(relationType, arg1, arg2);
        }
    }

    // PQL Data Structures
    class PQLQuery
    {
        public List<Declaration> Declarations { get; } = new List<Declaration>();
        public Selected Selected { get; set; }
        public List<Relation> Relations { get; } = new List<Relation>();
    }

    class Declaration
    {
        public string Type { get; }
        public string Name { get; }

        public Declaration(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    class Selected
    {
        public string Name { get; }

        public Selected(string name)
        {
            Name = name;
        }
    }

    class Relation
    {
        public string Type { get; }
        public string Arg1 { get; }
        public string Arg2 { get; }

        public Relation(string type, string arg1, string arg2)
        {
            Type = type;
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }
}
