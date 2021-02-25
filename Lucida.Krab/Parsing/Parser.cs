using Lucida.Krab.Characters;
using Lucida.Krab.CodeDocument;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucida.Krab.Parsing
{
    public static class Parser
    {
        public const string InternalSource = "&<internal>";

        private const string EmptyKey = "";
        private const string InheritsKey = "&<inht>";
        private const string BodyKey = "&<body>";
        private const string ExpressionKey = "&<expr>";
        private const string TerminalKey = "&<term>";
        private const string LiteralKey = "&<litr>";
        private const string CallKey = "&<call>";

        private static readonly ICharacterSource Empty = new StringSource(InternalSource, EmptyKey);
        private static readonly ICharacterSource Inherits = new StringSource(InternalSource, InheritsKey);
        private static readonly ICharacterSource Body = new StringSource(InternalSource, BodyKey);
        private static readonly ICharacterSource Expression = new StringSource(InternalSource, ExpressionKey);
        private static readonly ICharacterSource Terminal = new StringSource(InternalSource, TerminalKey);
        private static readonly ICharacterSource Literal = new StringSource(InternalSource, LiteralKey);
        private static readonly ICharacterSource Call = new StringSource(InternalSource, CallKey);

        private static readonly Dictionary<string, Operator> Operators = new Dictionary<string, Operator>
        {
            { "?", new Operator(1, 2, false) },
            { ":", new Operator(1, 2, false) },
            { "||", new Operator(2, 2, true) },
            { "&&", new Operator(3, 2, true) },
            { "|", new Operator(4, 2, true) },
            { "^", new Operator(5, 2, true) },
            { "&", new Operator(6, 2, true) },
            { "==", new Operator(7, 2, true) },
            { "!=", new Operator(7, 2, true) },
            { ">=", new Operator(8, 2, true) },
            { ">", new Operator(8, 2, true) },
            { "<=", new Operator(8, 2, true) },
            { "<", new Operator(8, 2, true) },
            { ">>", new Operator(9, 2, true) },
            { "<<", new Operator(9, 2, true) },
            { "+", new Operator(10, 2, true) },
            { "-", new Operator(10, 2, true) },
            { "*", new Operator(11, 2, true) },
            { "/", new Operator(11, 2, true) },
            { "%", new Operator(11, 2, true) },
            { "!", new Operator(12, 1, false) },
            { "~", new Operator(12, 1, false) }
        };

        public static Document Parse(ICharacterSource source)
        {
            return BuildDocument(ParseTokens(GetTokens(source)));
        }

        private static Document BuildDocument(Node[] nodes)
        {
            var result = new Document();

            foreach (var node in nodes)
            {
                result.Members.Add(BuildMember(node));
            }

            return result;
        }

        private static Member BuildMember(Node node)
        {
            switch (node.Type.Build())
            {
                case "namespace":
                    {
                        var ns = new Namespace(node.Name) { AccessModifiers = node.AccessModifiers };
                        var source = node.Children;
                        if (node.Children.Count == 1 && node.Children[0].Type.Build() == BodyKey) source = node.Children[0].Children;
                        foreach (var child in source) ns.Members.Add(BuildMember(child));
                        return ns;
                    }
                case "struct":
                    {
                        var s = new Structure(node.Name) { AccessModifiers = node.AccessModifiers };
                        var source = node.Children;
                        if (node.Children.Count == 1 && node.Children[0].Type.Build() == BodyKey) source = node.Children[0].Children;
                        foreach (var child in source) s.Members.Add(BuildMember(child));
                        return s;
                    }
                default:
                    if (node.NodeType == NodeType.Method)
                    {
                        var method = new Method(node.Type, node.Name) { AccessModifiers = node.AccessModifiers };

                        foreach (var child in node.Children)
                        {
                            if (method.Body.Count == 0 && child.Type.Build() == BodyKey)
                            {
                                foreach (var bodyChild in child.Children) method.Body.Add(BuildMember(bodyChild));
                            }
                            else
                            {
                                var m = BuildMember(child);

                                if (m is Field param)
                                {
                                    method.Parameters.Add(param);
                                }
                                else
                                {
                                    throw new SourceError(m.Name, "Parameter expected.");
                                }
                            }
                        }

                        return method;
                    }
                    else
                    {
                        if (node.Type.Build() == ExpressionKey)
                        {
                            return new Statement(Empty, BuildExpression(node)) { AccessModifiers = node.AccessModifiers };
                        }
                        else if (node.Children.Count == 0 || (node.Children.Count == 1 && node.Children[0].Type.Build() == ExpressionKey))
                        {
                            var field = new Field(node.Type, node.Name) { AccessModifiers = node.AccessModifiers };

                            if (node.Children.Count == 1)
                            {
                                field.InitialValue = BuildExpression(node.Children[0]);
                            }

                            return field;
                        }
                        else
                        {
                            throw new SourceError(node, "Assignment expression expected.");
                        }
                    }
            }
        }

        private static Expression BuildExpression(Node node)
        {
            switch (node.Type.Build())
            {
                case ExpressionKey:
                    if (node.Children.Count != 1) throw new SourceError(node, "Expected operation.");
                    return BuildExpression(node.Children[0]);
                case TerminalKey:
                    return new ReferenceExpression(node.Name);
                case LiteralKey:
                    return new LiteralExpression(node.Name);
                case CallKey:
                    var result = new CallExpression(node.Name);
                    foreach (var child in node.Children) result.Arguments.Add(BuildExpression(child));
                    return result;
                default:
                    throw new SourceError(node, "Expected expression.");
            }
        }

        private static Node[] ParseTokens(Token[] tokens)
        {
            var state = ParserState.FieldTypeOrKeyword;

            var fields = new List<Node>();
            var field = new Node();

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                switch (state)
                {
                    case ParserState.FieldTypeOrKeyword:
                        switch (token.Type)
                        {
                            case TokenType.Name:
                                field.Type = token;
                                field.Name = Empty;

                                switch (token.Build())
                                {
                                    case "public":
                                        field.AccessModifiers |= AccessModifiers.Public;
                                        break;
                                    case "private":
                                        field.AccessModifiers |= AccessModifiers.Private;
                                        break;
                                    case "protected":
                                        field.AccessModifiers &= ~(AccessModifiers.Public | AccessModifiers.Private);
                                        break;
                                    case "static":
                                        field.AccessModifiers |= AccessModifiers.Static;
                                        break;
                                    case "readonly":
                                        field.AccessModifiers |= AccessModifiers.Readonly;
                                        break;
                                    case "return":
                                        state = ParserState.Expression;
                                        break;
                                    case "if":
                                    case "elif":
                                        state = ParserState.SubExpression;
                                        break;
                                    case "else":
                                        state = ParserState.Body;
                                        break;
                                    case "while":
                                        state = ParserState.SubExpression;
                                        break;
                                    default:
                                        state = ParserState.FieldName;
                                        break;
                                }
                                break;
                            default:
                                field.Type = Expression;
                                field.Name = Empty;
                                state = ParserState.Expression;
                                i--;
                                break;
                        }
                        break;
                    case ParserState.FieldName:
                        switch (token.Type)
                        {
                            case TokenType.Name:
                                field.Name = token;
                                state = ParserState.EndOfFieldOrAdditional;
                                break;
                            default:
                                field.Type = Expression;
                                field.Name = Empty;
                                state = ParserState.Expression;
                                i -= 2;
                                break;
                        }
                        break;
                    case ParserState.EndOfFieldOrAdditional:
                        switch (token.Type)
                        {
                            case TokenType.Symbol:
                                switch (token.Build())
                                {
                                    case ";":
                                        field.NodeType = NodeType.Variable;
                                        state = ParserState.Finished;
                                        break;
                                    case ":":
                                        field.NodeType = NodeType.Variable;
                                        state = ParserState.Inherit;
                                        break;
                                    case "=":
                                        state = ParserState.Expression;
                                        break;
                                    case "{":
                                        field.NodeType = NodeType.Variable;
                                        state = ParserState.Body;
                                        i--;
                                        break;
                                    case "(":
                                        field.NodeType = NodeType.Method;
                                        state = ParserState.MethodParameterType;
                                        break;
                                    default:
                                        throw new SourceError(token, "Unexpected operator.");
                                }
                                break;
                            default:
                                throw new SourceError(token, "End of statement expected.");
                        }
                        break;
                    case ParserState.MethodParameterType:
                        switch (token.Type)
                        {
                            case TokenType.Name:
                                field.Children.Add(new Node
                                {
                                    NodeType = NodeType.Variable,
                                    Type = token
                                });
                                state = ParserState.MethodParameterName;
                                break;
                            case TokenType.Symbol:
                                switch (token.Build())
                                {
                                    case ")":
                                        state = ParserState.Body;
                                        break;
                                    default:
                                        throw new SourceError(token, "Parameter type expected.");
                                }
                                break;
                            default:
                                throw new SourceError(token, "Parameter type expected.");
                        }
                        break;
                    case ParserState.MethodParameterName:
                        switch (token.Type)
                        {
                            case TokenType.Name:
                                field.Children[field.Children.Count - 1].Name = token;
                                state = ParserState.MethodSeparatorOrEndBracket;
                                break;
                            default:
                                throw new SourceError(token, "Parameter name expected.");
                        }
                        break;
                    case ParserState.MethodSeparatorOrEndBracket:
                        switch (token.Type)
                        {
                            case TokenType.Symbol:
                                switch (token.Build())
                                {
                                    case ")":
                                        state = ParserState.Body;
                                        break;
                                    case ",":
                                        state = ParserState.MethodParameterType;
                                        break;
                                    default:
                                        throw new SourceError(token, ") expected.");
                                }
                                break;
                            default:
                                throw new SourceError(token, ") expected.");
                        }
                        break;
                    case ParserState.Body:
                        {
                            if (token.Type != TokenType.Symbol)
                            {
                                throw new SourceError(token, "End of statement expected.");
                            }

                            switch (token.Build())
                            {
                                case ";":
                                    state = ParserState.Finished;
                                    break;
                                case "{":
                                    var depth = 0;

                                    for (int j = i; j < tokens.Length; j++)
                                    {
                                        if (tokens[j].Type == TokenType.Symbol)
                                        {
                                            switch (tokens[j].Build())
                                            {
                                                case "{":
                                                    depth++;
                                                    break;
                                                case "}":
                                                    depth--;
                                                    break;
                                            }

                                            if (depth == 0)
                                            {
                                                var length = (j - i) - 1;

                                                if (length < 0) break;

                                                i += 1;

                                                if (length == 0) break;

                                                var childTokens = new Token[length];

                                                Array.Copy(tokens, i, childTokens, 0, length);

                                                field.Children.Add(new Node
                                                {
                                                    NodeType = NodeType.Variable,
                                                    Name = Empty,
                                                    Type = Body,
                                                    Children = new List<Node>(ParseTokens(childTokens))
                                                });

                                                i += length;

                                                break;
                                            }
                                        }
                                    }

                                    if (depth != 0) throw new SourceError(token, "End of block expected.");

                                    state = ParserState.Finished;
                                    break;
                                default:
                                    throw new SourceError(token, "End of statement expected.");
                            }
                        }
                        break;
                    case ParserState.Inherit:
                        switch (token.Type)
                        {
                            case TokenType.Name:
                                field.Children.Add(new Node
                                {
                                    NodeType = NodeType.Variable,
                                    Type = Inherits,
                                    Name = token
                                });
                                state = ParserState.InheritSeparator;
                                break;
                            default:
                                throw new SourceError(token, "Inherited type expected.");
                        }
                        break;
                    case ParserState.InheritSeparator:
                        if (token.Build() == ",")
                        {
                            state = ParserState.Inherit;
                        }
                        else
                        {
                            state = ParserState.Body;
                            i--;
                        }
                        break;
                    case ParserState.Expression:
                        {
                            var length = -1;

                            for (int j = i; j < tokens.Length; j++)
                            {
                                if (tokens[j].Type == TokenType.Symbol && tokens[j].Build() == ";")
                                {
                                    length = j - i;
                                    break;
                                }
                            }

                            if (length < 0) throw new SourceError(token, "End of statement expected.");

                            if (length != 0)
                            {
                                var childTokens = new Token[length];

                                Array.Copy(tokens, i, childTokens, 0, length);

                                field.Children.Add(new Node
                                {
                                    NodeType = NodeType.Variable,
                                    Type = Expression,
                                    Name = Empty,
                                    Children = new List<Node> { ParseExpression(childTokens) }
                                });
                            }

                            i += length;
                            field.NodeType = NodeType.Variable;
                            state = ParserState.Finished;
                        }
                        break;
                    case ParserState.SubExpression:
                        if (token.Build() == "(")
                        {
                            var depth = 0;

                            for (int j = i; j < tokens.Length; j++)
                            {
                                if (tokens[j].Type == TokenType.Symbol)
                                {
                                    switch (tokens[j].Build())
                                    {
                                        case "(":
                                            depth++;
                                            break;
                                        case ")":
                                            depth--;
                                            break;
                                    }

                                    if (depth == 0)
                                    {
                                        var length = (j - i) - 1;

                                        if (length <= 0) break;

                                        var childTokens = new Token[length];

                                        Array.Copy(tokens, i + 1, childTokens, 0, length);

                                        field.Children.Add(new Node
                                        {
                                            NodeType = NodeType.Variable,
                                            Name = Empty,
                                            Type = Expression,
                                            Children = new List<Node> { ParseExpression(childTokens) }
                                        });

                                        i += length + 1;

                                        break;
                                    }
                                }
                            }

                            if (depth != 0) throw new SourceError(token, "End of block expected.");

                            state = ParserState.Body;
                        }
                        else
                        {
                            throw new SourceError(token, "( expected.");
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if (state == ParserState.Finished)
                {
                    if (field.NodeType == NodeType.Undefined) throw new SourceError(token, "Invalid syntax.");

                    fields.Add(field);
                    field = new Node();
                    state = ParserState.FieldTypeOrKeyword;
                }
            }

            if (state != ParserState.FieldTypeOrKeyword)
            {
                throw new SourceError(tokens.Last(), "Unexpected end of source.");
            }

            return fields.ToArray();
        }

        private static Node ParseExpression(Token[] tokens)
        {
            var shuntingYard = new LinkedList<ExpressionToken>();
            var functionStack = new LinkedList<ExpressionToken>();
            var postfix = new Queue<ExpressionToken>();

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                switch (token.Type)
                {
                    case TokenType.Symbol:
                        switch (token.Build())
                        {
                            case "(":
                                shuntingYard.Push(new ExpressionToken(token, true, i));
                                break;
                            case ")":
                                {
                                    while (true)
                                    {
                                        if (shuntingYard.Count == 0)
                                        {
                                            throw new SourceError(token, "( expected.");
                                        }

                                        var op = shuntingYard.Pop();

                                        if (op.Token.Build() == "(") break;

                                        if (functionStack.Count > 0 && functionStack.Peek() == op) op = functionStack.Pop();

                                        postfix.Enqueue(op);
                                    }

                                    if (shuntingYard.Count > 0 && shuntingYard.Peek().Token.Type == TokenType.Name)
                                    {
                                        var op = shuntingYard.Pop();

                                        if (functionStack.Peek() == op) op = functionStack.Pop();

                                        op.Arguments += (i - op.Index) == 2 ? -1 : 0;

                                        postfix.Enqueue(op);
                                    }
                                }
                                break;
                            case ",":
                                if (functionStack.Count > 0)
                                {
                                    while (true)
                                    {
                                        if (shuntingYard.Count == 0) throw new SourceError(token, "( expected.");

                                        if (shuntingYard.Peek().Token.Build() == "(") break;

                                        var op = shuntingYard.Pop();

                                        if (functionStack.Peek() == op) op = functionStack.Pop();

                                        postfix.Enqueue(op);
                                    }

                                    {
                                        var op = functionStack.Pop();
                                        op.Arguments += 1;
                                        functionStack.Push(op);
                                    }
                                }
                                else
                                {
                                    throw new SourceError(token, "Unexpected separator.");
                                }
                                break;
                            default:
                                while (shuntingYard.Count > 0 && shuntingYard.Peek().Token.Build() != "(" && OperatorHasPrecedence(shuntingYard.Peek().Token, token))
                                {
                                    postfix.Enqueue(shuntingYard.Pop());
                                }
                                shuntingYard.Push(new ExpressionToken(token, true, i));
                                break;
                        }
                        break;
                    case TokenType.Name:
                        if (i < tokens.Length - 1 && tokens[i + 1].Build() == "(")
                        {
                            var op = new ExpressionToken(token, true, i);
                            shuntingYard.Push(op);
                            functionStack.Push(op);
                        }
                        else
                        {
                            postfix.Enqueue(new ExpressionToken(token, false, i));
                        }
                        break;
                    case TokenType.Value:
                        postfix.Enqueue(new ExpressionToken(token, false, i));
                        break;
                }
            }

            while (shuntingYard.Count > 0)
            {
                postfix.Enqueue(shuntingYard.Pop());
            }

            var evalStack = new Stack<Node>();

            while (postfix.Count > 0)
            {
                var op = postfix.Dequeue();

                if (op.IsFunction)
                {
                    var argCount = op.Arguments + (op.Token.Type == TokenType.Name ? 1 : 0);
                    var args = new List<Node>(argCount);

                    for (int i = 0; i < argCount && evalStack.Count > 0; i++)
                    {
                        args.Add(evalStack.Pop());
                    }

                    if (args.Count != argCount) throw new SourceError(op.Token, "Missing operands.");

                    args.Reverse();

                    evalStack.Push(new Node
                    {
                        Type = Call,
                        Name = op.Token,
                        Children = args
                    });
                }
                else
                {
                    evalStack.Push(new Node
                    {
                        Type = op.Token.Type == TokenType.Value ? Literal : Terminal,
                        Name = op.Token
                    });
                }
            }

            if (evalStack.Count == 0) throw new SourceError(tokens.FirstOrDefault(), "Expression is empty.");

            if (evalStack.Count > 1)
            {
                evalStack.Pop();
                throw new SourceError(evalStack.Pop(), "End of expression expected.");
            }

            return evalStack.Pop();
        }

        private static bool OperatorHasPrecedence(Token a, Token b)
        {
            return GetPrecedence(a, false) > GetPrecedence(b, true);
        }

        private static int GetPrecedence(Token token, bool applyAssociativeRule = false)
        {
            if (token.Type == TokenType.Name) return int.MaxValue;

            if (Operators.TryGetValue(token.Build(), out Operator op))
            {
                return op.Precedence - (applyAssociativeRule && op.LeftAssociative ? 1 : 0);
            }

            return -1;
        }

        private static int StackArgs(ExpressionToken op)
        {
            if (Operators.TryGetValue(op.Token.Build(), out Operator result))
            {
                return result.Arguments;
            }
            else
            {
                return 0;
            }
        }

        private static Token[] GetTokens(ICharacterSource source)
        {
            var result = new List<Token>();

            for (long i = 0; i < source.Length; i++)
            {
                if (IsNameChar(source[i], true))
                {
                    var length = 1L;

                    for (long j = i + 1; j < source.Length && IsNameChar(source[j], false); j++, length++) { }

                    result.Add(new Token
                    {
                        Type = TokenType.Name,
                        Value = source.Substring(i, length)
                    });

                    i += length - 1;
                }
                else if (IsValueChar(source[i], true))
                {
                    var length = 1L;

                    for (long j = i + 1; j < source.Length && IsValueChar(source[j], false); j++, length++) { }

                    if (length == 1 && source[i] == '-')
                    {
                        result.Add(new Token
                        {
                            Type = TokenType.Symbol,
                            Value = source.Substring(i, length)
                        });
                    }
                    else
                    {
                        result.Add(new Token
                        {
                            Type = TokenType.Value,
                            Value = source.Substring(i, length)
                        });
                    }

                    i += length - 1;
                }
                else if (IsSymbolChar(source[i], true))
                {
                    var length = 1L;

                    for (long j = i + 1; j < source.Length && IsSymbolChar(source[j], false); j++, length++) { }

                    result.Add(new Token
                    {
                        Type = TokenType.Symbol,
                        Value = source.Substring(i, length)
                    });

                    i += length - 1;
                }
            }

            return result.ToArray();
        }

        private static bool IsNameChar(char c, bool first)
        {
            return char.IsLetter(c) || c == '_' || (!first && (char.IsDigit(c) || c == '.'));
        }

        private static bool IsValueChar(char c, bool first)
        {
            return char.IsDigit(c) || c == '\'' || (!first && (char.IsLetter(c) || c == '\\')) || (first && c == '-');
        }

        private static bool IsSymbolChar(char c, bool first)
        {
            return !(char.IsWhiteSpace(c) || char.IsControl(c) || IsNameChar(c, true) || IsValueChar(c, true) || (!first && (c == ';' || c == ',' || c == '(' || c == ')' || c == '{' || c == '}')));
        }

        private static void Push<T>(this LinkedList<T> list, T value)
        {
            list.AddLast(value);
        }

        private static T Peek<T>(this LinkedList<T> list)
        {
            return list.Last.Value;
        }

        private static T Pop<T>(this LinkedList<T> list)
        {
            var result = list.Last.Value;
            list.RemoveLast();
            return result;
        }

        private struct ExpressionToken
        {
            public Token Token;
            public int Arguments;
            public int Index;
            public bool IsFunction;

            public ExpressionToken(Token token, bool isFunction, int index)
            {
                Token = token;
                IsFunction = isFunction;
                Index = index;
                Arguments = 0;
                Arguments = StackArgs(this);
            }

            public static bool operator ==(ExpressionToken a, ExpressionToken b)
            {
                return a.Token.Type == b.Token.Type && a.Token.Value == b.Token.Value;
            }

            public static bool operator !=(ExpressionToken a, ExpressionToken b)
            {
                return !(a == b);
            }

            public override string ToString()
            {
                return Token.ToString();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private struct Operator
        {
            public int Precedence;
            public int Arguments;
            public bool LeftAssociative;

            public Operator(int precedence, int arguments, bool leftAssociative)
            {
                Precedence = precedence;
                Arguments = arguments;
                LeftAssociative = leftAssociative;
            }
        }

        private enum ParserState
        {
            FieldTypeOrKeyword,
            FieldName,
            EndOfFieldOrAdditional,
            Expression,
            SubExpression,
            MethodParameterType,
            MethodParameterName,
            MethodSeparatorOrEndBracket,
            Inherit,
            InheritSeparator,
            Body,
            Finished
        }
    }
}
