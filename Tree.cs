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
        private void RemoveReverseNotations()
        {
            this.reversePolishNotation = null;
            this.reversePolishNotation = new List<Token>();
        }
        public void UpdateValueInInfixNotation(TreeFunctional tree)
        {
            this.RemoveReverseNotations();
            foreach (var token in this.infixNotation)
            {
                if (token.type == TokenType.VARIABLE)
                {
                    var tokenVar = (token as TokenVariable);
                    tokenVar.data = tree.GetVar(tokenVar.name).data;
                }
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
        public Dictionary<string, TokenVariable> stackVariable;
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
                    token = new TokenIfElse(startPos, endPos, TokenType.IF);
                    break;

                default:
                    token = null;
                    break;
            }

            this.head = token;
            this.next = null;
            this.stackVariable = new Dictionary<string, TokenVariable>();
        }
        public virtual void PutVariableinStack(TokenVariable var)
        {
            stackVariable.Add(var.name, var);
        }
        public virtual TokenVariable GetVar(string name)
        {
            return stackVariable[name];
        }
        public virtual bool VariableExist(string name)
        {
            return stackVariable.ContainsKey(name);
        }
        public virtual void UpdateVar(string name, object data, VariableType varType)
        {
            stackVariable[name].data = data;
            stackVariable[name].varType = varType;
        }
    }
    class TreeIf : TreeFunctional
    {
        protected ArichmetichTree arichmeticStatementTree;
        public Dictionary<string, TokenVariable> stackVariableLocal;

        protected TreeIf()
        {
            this.stackVariableLocal = new Dictionary<string, TokenVariable>();
        }
        public TreeIf(TokenIfElse ifToken, TreeFunctional tree):base()
        {
            if (tree.head.type == TokenType.IF || tree.head.type == TokenType.FOR)
            {
                var tmp = tree as TreeIf;
                this.stackVariable = tmp.stackVariable;
                foreach (var item in tmp.stackVariableLocal)
                {
                    this.stackVariable.Add(item.Key, item.Value);
                }
            }
            else
            {
                this.stackVariable = tree.stackVariable;
            }
            this.head = ifToken;
            this.arichmeticStatementTree = new ArichmetichTree(null);
            this.stackVariableLocal = new Dictionary<string, TokenVariable>();
        }

        /// <summary>
        /// if i < 2 is true expected token with type NUMERIC_CONST and data is 1
        /// </summary>
        /// <param name="token"></param>  
        public void PutTokenInStatement (Token token, Token tokenOfOperation, Token rightOperator , int line)
        {
            double leftData = 0;
            if (token.type == TokenType.VARIABLE || token.type == TokenType.NUMERIC_CONST)
                leftData = (double)(token as TokenNumeric).data;

            switch (token.type)
            {
                case TokenType.ARITHMETIC_BRACKET_OPEN:
                    this.arichmeticStatementTree.PutToken(new Token() { type = TokenType.ARITHMETIC_BRACKET_OPEN });
                    break;
                case TokenType.ARITHMETIC_BRACKET_CLOSE:
                    this.arichmeticStatementTree.PutToken(new Token() { type = TokenType.ARITHMETIC_BRACKET_CLOSE });
                    break;
                case TokenType.VARIABLE:
                case TokenType.NUMERIC_CONST:
                    var tokenCondition = tokenOfOperation;
                    Token nextToken = rightOperator;
                    double rightData = (double)(nextToken as TokenNumeric).data;
                    double resultSmallStation = 0;
                    switch (tokenCondition.type)
                    {
                        case TokenType.MORE:
                            if (leftData > rightData)
                                resultSmallStation = 1;
                            else
                                resultSmallStation = 0;
                            goto putInStatement;

                        case TokenType.LESS:
                            if (leftData < rightData)
                                resultSmallStation = 1;
                            else
                                resultSmallStation = 0;
                            goto putInStatement;


                        case TokenType.EQUAL:
                            if (leftData == rightData)
                                resultSmallStation = 1;
                            else
                                resultSmallStation = 0;
                            goto putInStatement;

                        case TokenType.NOT_EQUAL:
                            if (leftData != rightData)
                                resultSmallStation = 1;
                            else
                                resultSmallStation = 0;
                            goto putInStatement;


                        putInStatement:
                            arichmeticStatementTree.PutToken(new TokenNumeric() { type = TokenType.NUMERIC_CONST, data = resultSmallStation });
                            break;
                    }
                    break;

                case TokenType.OR:
                    arichmeticStatementTree.PutToken(new Token() { type = TokenType.PLUS });
                    break;

                case TokenType.AND:
                    arichmeticStatementTree.PutToken(new Token() { type = TokenType.MULTIPLICATION });
                    break;

                default:
                    throw new Exception("Something went bad in if condition Line:" + line.ToString());
            }
        }

        public virtual bool ProcessStatement()
        {
            double res = (double)arichmeticStatementTree.Process(null);
            double precision = 0.1;
            if (Math.Abs(res) - precision < 0)
                return false;
            return true;
        }

        public override void PutVariableinStack(TokenVariable var)
        {
            stackVariableLocal.Add(var.name, var);
        }
        public override TokenVariable GetVar(string name)
        {
            if (stackVariableLocal.ContainsKey(name))
                return stackVariableLocal[name];

            if (stackVariable.ContainsKey(name))
                return stackVariable[name];

            return null;
        }
        public override bool VariableExist(string name)
        {
            if (stackVariable.ContainsKey(name) || stackVariableLocal.ContainsKey(name))
                return true;
            return false;
        }
        public override void UpdateVar(string name, object data, VariableType varType)
        {
            Dictionary<string, TokenVariable> tmp = null;
            if (stackVariable.ContainsKey(name))
                tmp = stackVariable;
            else if (stackVariableLocal.ContainsKey(name))
                tmp = stackVariableLocal;

            tmp[name].data = data;
            tmp[name].varType = varType;
        }
    }
    class TreeFor : TreeIf
    {
        TokenVariable forVar;
        ArichmetichTree updateForVar;
        public bool initForVar = false;

        public TreeFor(TokenFor forToken, TokenVariable forVar, ArichmetichTree updateForVar, TreeFunctional tree) :base()
        {
            if (tree.head.type == TokenType.IF || tree.head.type == TokenType.FOR)
            {
                var tmp = tree as TreeIf;
                this.stackVariable = tmp.stackVariable;
                foreach (var item in tmp.stackVariableLocal)
                {
                    this.stackVariable.Add(item.Key, item.Value);
                }
            }
            else
            {
                this.stackVariable = tree.stackVariable;
            }
            this.head = forToken;
            this.forVar = forVar;
            this.updateForVar = updateForVar;
        }
        public bool ProcessStatement(List<Token> statCondFor, int line)
        {
            this.arichmeticStatementTree = new ArichmetichTree(null);

            for (int i = 0; i < statCondFor.Count; i++)
            {
                TokenType type = statCondFor[i].type;
                if (type == TokenType.VARIABLE || type == TokenType.NUMERIC_CONST)
                    this.PutTokenInStatement(
                        statCondFor[i].type == TokenType.VARIABLE ? this.GetVar((statCondFor[i] as TokenVariable).name) : statCondFor[i],
                        statCondFor[++i], 
                        statCondFor[++i].type == TokenType.VARIABLE ? this.GetVar((statCondFor[i] as TokenVariable).name) : statCondFor[i], 
                        line);
                else
                    this.PutTokenInStatement(statCondFor[i], null, null, line); 
            }
            return this.ProcessStatement();
        }

        public void ProcessUpdateForVar()
        {
            this.updateForVar.UpdateValueInInfixNotation(this);
            this.updateForVar.Process(this);
        }
    }
}
