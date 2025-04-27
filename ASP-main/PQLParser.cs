using SPA_main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main
{
    public class PQLParser
    {
        public List<Token> _tokens;
        public int _index = 0;

        public PQLParser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token CurrentToken => _index < _tokens.Count ? _tokens[_index] : null;
        private Token NextToken => _index + 1 < _tokens.Count ? _tokens[_index + 1] : null;
        public void Eat(string type)
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
                var decls = ParseDeclaration();
                foreach (var decl in decls)
                {
                    query.Declarations.Add(decl.Name, decl);
                }
            }

            // Parse select
            Eat("SELECT");
            query.Selected = ParseSelected();

            // Parse clauses
            while (CurrentToken != null)
            {
                if (string.Equals(CurrentToken.Type, "SUCH_THAT", StringComparison.OrdinalIgnoreCase))
                {
                    Eat("SUCH_THAT");
                    var relation = ParseRelation();
                    query.Relations.Add(relation);
                }
                else if (string.Equals(CurrentToken.Type, "WITH", StringComparison.OrdinalIgnoreCase))
                {
                    Eat("WITH");
                    var withClause = ParseWithClause();
                    query.WithClauses.Add(withClause);           
                }
                else if (string.Equals(CurrentToken.Type, "AND", StringComparison.OrdinalIgnoreCase))
                {
                    Eat("AND");
                    // Obsługa AND - podobna do SUCH_THAT/WITH w zależności od następnego tokenu
                    if ( NextToken?.Type == "DOT")
                    {
                       
                        var withClause = ParseWithClause();
                        query.WithClauses.Add(withClause);
                    }
                    else
                    {
                        var relation = ParseRelation();
                        query.Relations.Add(relation);
                    }
                }
                else
                {
                    break;
                }
            }

            return query;
        }

        public WithClause ParseWithClause()
        {
            var left = ParseWithArgument();
            Eat("EQUALS");
            var right = ParseWithArgument();
            return new WithClause(left, right);
        }

        public List<Declaration> ParseDeclaration()
        {
            var declarations = new List<Declaration>();
            var type = CurrentToken.Value;
            Eat(type.ToUpper()); // np. STMT, VARIABLE, etc.

            // Parsuj pierwszy identyfikator
            var firstName = CurrentToken.Value;
            Eat("NAME");
            declarations.Add(new Declaration(type, firstName));

            // Parsuj kolejne identyfikatory po przecinkach
            while (CurrentToken != null && CurrentToken.Type == "COMMA")
            {
                Eat("COMMA");
                var nextName = CurrentToken.Value;
                Eat("NAME");
                declarations.Add(new Declaration(type, nextName));
            }

            Eat("SEMICOLON");
            return declarations;
        }

        public WithArgument ParseWithArgument()
        {
            // Case 1: Attribute reference (like v.varName)
            if (CurrentToken.Type == "NAME" && NextToken?.Type == "DOT")
            {
                var refName = CurrentToken.Value;
                Eat("NAME");
                Eat("DOT");
                string attrName;
                // Obsługa różnych atrybutów
                if (CurrentToken.Type == "STMT")
                {
                    Eat("STMT");
                    return new WithArgument(refName, "stmt#")
                    {
                        LineNumber = CurrentToken?.LineNumber // Przypisanie numeru linii
                    };
                }
                else
                if (CurrentToken.Type == "VAR_ATTR" || CurrentToken.Value == "varName")
                {
                    Eat(CurrentToken.Type);
                    return new WithArgument(refName, "varName");
                }
                else if (CurrentToken.Type == "QUOTE")
                {
                    Eat("QUOTE");
                    var value = CurrentToken.Value;
                    Eat("NAME");
                    Eat("QUOTE");
                    return new WithArgument(value, isValue: true);
                }
                // Case 3: Simple reference (variable name)
                else if (CurrentToken.Type == "NAME")
                {
                    var value = CurrentToken.Value;
                    Eat("NAME");
                    return new WithArgument(value, isValue: false);
                }
                else if (CurrentToken.Type == "NUMBER")
                {
                    var value = CurrentToken.Value;
                    Eat("NUMBER");
                    return new WithArgument(value, isValue: false);
                }
                else
                {
                    throw new Exception($"Unexpected attribute: {CurrentToken.Value}");
                }

                
            }
            // Case 2: Quoted string value
            else if (CurrentToken.Type == "QUOTE")
            {
                Eat("QUOTE");
                var value = CurrentToken.Value;
                Eat("NAME"); // Albo inny odpowiedni typ tokena dla wartości
                Eat("QUOTE");
                return new WithArgument(value, isValue: true);
            }
            // Case 3: Numeric value
            else if (CurrentToken.Type == "NUMBER")
            {
                var value = CurrentToken.Value;
                Eat("NUMBER");
                return new WithArgument(value, isValue: true);
            }
            // Case 4: Simple reference (variable name)
            else if (CurrentToken.Type == "NAME")
            {
                var value = CurrentToken.Value;
                Eat("NAME");
                return new WithArgument(value, isValue: true);
            }
            else
            {
                throw new Exception($"Unexpected token in with clause: {CurrentToken}");
            }
        }
    

        public Selected ParseSelected()
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

        public Relation ParseRelation()
        {
            var relationType = CurrentToken.Value;

            if (CurrentToken.Type == "PARENT_STAR")
            {
                relationType = "Parent*";
                Eat("PARENT_STAR"); // Zmieniamy to z CurrentToken.Value.ToUpper()
            }
            else if (CurrentToken.Type == "FOLLOWS_STAR")
            {
                relationType = "Follows*";
                Eat("FOLLOWS_STAR");
            }
            else
            {
                relationType = CurrentToken.Value;
                Eat(CurrentToken.Type); // Używamy Type zamiast Value.ToUpper()
            }
          
            
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
    public class PQLQuery
    {
        public Dictionary<string, Declaration> Declarations { get; } = new Dictionary<string, Declaration>();
        public Selected Selected { get; set; }
        public List<Relation> Relations { get; } = new List<Relation>();
        public List<WithClause> WithClauses { get; } = new List<WithClause>();
    }

    public class WithClause
    {
        public WithArgument Left { get; }
        public WithArgument Right { get; }

        public bool IsAttributeComparison => Left.IsAttributeRef && Right.IsAttributeRef;
        public bool IsStmtNumAssignment => Left.Attribute == "stmt#" && Right.IsValue;

        public WithClause(WithArgument left, WithArgument right)
        {
            Left = left;
            Right = right;
        }
    }

    public class WithArgument
    {
        public string Reference { get; }  // np. "s" w "s.stmt#"
        public string Attribute { get; } // np. "stmt#" w "s.stmt#"
        public string Value { get; }     // wartość dla stałych
        public bool IsValue { get; }     // czy to stała wartość
        public bool IsAttributeRef => !string.IsNullOrEmpty(Reference) && !string.IsNullOrEmpty(Attribute);
        public bool IsSimpleRef => !IsAttributeRef && !IsValue;

        public int? LineNumber { get; set; }

        public WithArgument(string reference, string attribute)
        {
            Reference = reference;
            Attribute = attribute;
            IsValue = false;
        }

        public WithArgument(string value, bool isValue)
        {
            if (isValue)
            {
                Value = value;
                IsValue = true;
            }
            else
            {
                Reference = value;
                IsValue = false;
            }
        }
    }

    public class Declaration
    {
        public string Type { get; }  // np. "STMT", "VARIABLE", "PROCEDURE"
        public string Name { get; }  // np. "s", "v", "p"
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        public Declaration(string type, string name)
        {
            Type = type;
            Name = name;

            // Inicjalizacja domyślnych atrybutów w zależności od typu
            switch (type.ToUpper())
            {
                case "VARIABLE":
                    Attributes["varName"] = null;
                    break;
                case "PROCEDURE":
                    Attributes["procName"] = null;
                    break;
                case "ASSIGN":
                case "STMT":
                case "WHILE":
                case "IF":
                    Attributes["stmt#"] = null;
                    break;
                case "CONSTANT":
                    Attributes["value"] = null;
                    break;
            }
        }
    }

    public class Selected
    {
        public string Name { get; }

        public Selected(string name)
        {
            Name = name;
        }
    }

    public class Relation
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
