using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class Tree
    {
        public Token head;
    }
    
    abstract class ProcessTree : Tree
    {
        abstract public object Process(TreeFunctional treeFunction);
    }


    class ArichmetichTree : ProcessTree
    {
        List<Token> infixNotation;
        List<Token> reversePolishNotation;
        Stack<Token> stack;
        TokenVariable tokenToChange;
        public ArichmetichTree(TokenVariable tokenToChange)
        {
            this.tokenToChange = tokenToChange;
            infixNotation = new List<Token>();
            reversePolishNotation = new List<Token>();
            stack = new Stack<Token>();
        }

        public void PutToken(Token token)
        {
            infixNotation.Add(token);
        }
        private void MakeReversePolishNotation()
        {
            foreach (var token in infixNotation)
            {
                switch (token.type)
                {
                    case TokenType.VARIABLE:
                    case TokenType.NUMERIC_CONST:
                        this.reversePolishNotation.Add(token);
                        break;
                    case TokenType.ARITHMETIC_BRACKET_OPEN:
                        //this.reversePolishNotation.Add(token);
                        this.stack.Push(token);
                        break;
                    case TokenType.ARITHMETIC_BRACKET_CLOSE:
                        var stackToken = stack.Pop();
                        while (stackToken.type != TokenType.ARITHMETIC_BRACKET_OPEN)
                        {
                            this.reversePolishNotation.Add(stackToken);
                            stackToken = stack.Pop();
                        }
                        break;
                    default:
                        if (stack.Count != 0 && Priority(token) <= Priority(stack.Peek()))
                        {
                            this.reversePolishNotation.Add(stack.Pop());
                        }
                        stack.Push(token);
                        break;
                }
            }
            int stackCount = stack.Count;            
            for (int i = 0; i < stackCount; i++)
            {
                reversePolishNotation.Add(stack.Pop());
            }
        }
        int Priority(Token token)
        {
            switch (token.type)
            {
                case TokenType.MULTIPLICATION:
                case TokenType.DIVIDE:
                    return 2;
                case TokenType.PLUS:
                case TokenType.MINUS:
                    return 1;
                case TokenType.ARITHMETIC_BRACKET_OPEN:
                case TokenType.ARITHMETIC_BRACKET_CLOSE:
                    return 0;
                default:
                    throw new Exception("something bad in get priority!");
            }
        }
        public override object Process(TreeFunctional treeFunction)
        {
            MakeReversePolishNotation();

            foreach (var token in reversePolishNotation)
            {
                switch (token.type)
                {
                    case TokenType.NUMERIC_CONST:
                    case TokenType.VARIABLE:
                        stack.Push(token);
                        break;
                    case TokenType.PLUS:
                        stack.Push(new TokenNumeric() { data = (double)((stack.Pop() as TokenNumeric).data) + (double)((stack.Pop() as TokenNumeric).data) });
                        break;
                    case TokenType.MINUS:
                        var right = (double)((stack.Pop() as TokenNumeric).data);
                        var left = (double)((stack.Pop() as TokenNumeric).data);
                        stack.Push(new TokenNumeric() { data = left - right });
                        break;
                    case TokenType.MULTIPLICATION:
                        stack.Push(new TokenNumeric() { data = (double)((stack.Pop() as TokenNumeric).data) * (double)((stack.Pop() as TokenNumeric).data) });
                        break;
                    case TokenType.DIVIDE:
                        right = (double)((stack.Pop() as TokenNumeric).data);
                        left = (double)((stack.Pop() as TokenNumeric).data);
                        stack.Push(new TokenNumeric() { data = left / right });
                        break;
                    default:
                        throw new Exception("something bad in get priority!");
                }
            }
            object data = (stack.Pop() as TokenNumeric).data;
            if (tokenToChange != null && treeFunction != null)
               treeFunction.UpdateVar(tokenToChange.name, data, VariableType.NUMERIC);

            return data;
        }
    }

    class StringTree : ProcessTree
    {
        TokenVariable tokenToChange;
        List<TokenVariable> strings;
        public StringTree(TokenVariable tokenToChange)
        {
            if (tokenToChange.varType == VariableType.NUMERIC)
                throw new Exception("Something bad in constructor in String tree!");

            this.tokenToChange = tokenToChange;
            strings = new List<TokenVariable>();
        }

        public void AddToken(TokenVariable token)
        {
            if (token.varType == VariableType.STRING)
            {
                strings.Add(token);
            }
            else
            {
                throw new Exception("Something bad in constructor in String tree!");
            }
        }

        public override object Process(TreeFunctional treeFunction)
        {
            string str = "";
            foreach (var token in strings)
            {
                str += (string)token.data;
            }

            treeFunction.UpdateVar(tokenToChange.name, str, VariableType.STRING);
            return str;
        }
    }

    class TreeFunctional : Tree
    {
        public ProcessTree next;
        public string name;
        private Dictionary<string, TokenVariable> stackVariable;
        public TreeFunctional()
        {
            this.name = null;
            this.head = null;
            this.next = null;
        }
        public TreeFunctional(string name, TokenType type, int startPos, int endPos)
        {
            this.name = name;
            Token token;
            switch (type)
            {
                case TokenType.FUNCTION:
                    token = new TokenFunction(startPos, endPos);
                    break;
                case TokenType.IF:
                    token = new TokenIf(startPos, endPos);
                    break;

                default:
                    token = null;
                    break;
            }

            this.head = token;
            this.next = null;
            this.stackVariable = new Dictionary<string, TokenVariable>();
        }
        public void PutVariableinStack(TokenVariable var)
        {
            stackVariable.Add(var.name, var);
        }
        public TokenVariable GetVar(string name)
        {
            return stackVariable[name];
        }
        public bool VariableExist(string name)
        {
            return stackVariable.ContainsKey(name);
        }
        public void UpdateVar(string name, object data, VariableType varType)
        {
            stackVariable[name].data = data;
            stackVariable[name].varType = varType;
        }
    }
    class TreeIf : TreeFunctional
    {
        private ArichmetichTree arichmeticTree;

        public TreeIf(TokenIf ifToken):base()
        {
            this.head = ifToken;
            this.arichmeticTree = new ArichmetichTree(null);
        }

        /// <summary>
        /// if i < 2 is true expected token with type NUMERIC_CONST and data is 1
        /// </summary>
        /// <param name="token"></param>
        public void PutTokenInStatement (Token token)
        {
            switch (token.type)
            {
                case TokenType.ARITHMETIC_BRACKET_OPEN:
                case TokenType.ARITHMETIC_BRACKET_CLOSE:
                case TokenType.VARIABLE:
                case TokenType.NUMERIC_CONST:
                    arichmeticTree.PutToken(token);
                    break;

                case TokenType.OR:
                    arichmeticTree.PutToken(new Token() { type = TokenType.PLUS });
                    break;

                case TokenType.AND:
                    arichmeticTree.PutToken(new Token() { type = TokenType.MULTIPLICATION });
                    break;

                default:
                    throw new Exception("Something bad in if statement!");
                    break;
            }
        }

        public bool ProcessStatement()
        {
            double res = (double)arichmeticTree.Process(null);
            double precision = 0.1;
            if (Math.Abs(res) - precision < 0)
                return false;
            return true;
        }

    }
}
