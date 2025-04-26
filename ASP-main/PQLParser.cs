﻿using SPA_main;
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
                query.Declarations.AddRange(decls);
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
                    if (withClause.Left.Attribute == "stmt#" && withClause.Right.IsValue)
                    {
                        // Znajdź wszystkie relacje zawierające tę zmienną i zaktualizuj je
                        for (int i = 0; i < query.Relations.Count; i++)
                        {
                            var relation = query.Relations[i];

                            // Utwórz nową relację z podstawioną wartością
                            var newRelation = new Relation(
                                relation.Type,
                                relation.Arg1 == withClause.Left.Reference ? withClause.Right.Value : relation.Arg1,
                                relation.Arg2 == withClause.Left.Reference ? withClause.Right.Value : relation.Arg2
                            );

                            query.Relations[i] = newRelation;
                        }
                    }

                    else if (withClause.Left.Attribute == "varName")
                    {
                        string value = withClause.Right.IsValue
                            ? withClause.Right.Value
                            : withClause.Right.Value; // W rzeczywistości powinno się sprawdzić wartość otherVar

                        for (int i = 0; i < query.Relations.Count; i++)
                        {
                            var relation = query.Relations[i];
                            var newRelation = new Relation(
                                relation.Type,
                                relation.Arg1 == withClause.Left.Reference ? value : relation.Arg1,
                                relation.Arg2 == withClause.Left.Reference ? value : relation.Arg2
                            );
                            query.Relations[i] = newRelation;
                        }
                    }







                    query.WithClauses.Add(withClause);
                }
                else if (string.Equals(CurrentToken.Type, "AND", StringComparison.OrdinalIgnoreCase))
                {
                    Eat("AND");
                    // Obsługa AND - podobna do SUCH_THAT/WITH w zależności od następnego tokenu
                    if (CurrentToken.Type == "WITH" || NextToken?.Type == "EQUALS")
                    {
                        Eat("WITH");
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
                return new WithArgument(value, isValue: false);
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
        public List<Declaration> Declarations { get; } = new List<Declaration>();
        public Selected Selected { get; set; }
        public List<Relation> Relations { get; } = new List<Relation>();
        public List<WithClause> WithClauses { get; } = new List<WithClause>();
    }

    public class WithClause
    {
        public WithArgument Left { get; }
        public WithArgument Right { get; }

        public WithClause(WithArgument left, WithArgument right)
        {
            Left = left;
            Right = right;
        }
    }

    public class WithArgument
    {
        public string Reference { get; }
        public string Attribute { get; }
        public string Value { get; }
        public bool IsValue { get; }
        public int? LineNumber { get; set; }

        public WithArgument(string reference, string attribute)
        {
            Reference = reference;
            Attribute = attribute;
            IsValue = false;
        }

        public WithArgument(string value, bool isValue)
        {
            Value = value;
            IsValue = true;
        }
    }

    public class Declaration
    {
        public string Type { get; }
        public string Name { get; }

        public Declaration(string type, string name)
        {
            Type = type;
            Name = name;
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
