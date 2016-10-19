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
        abstract public void Process(TreeFunction treeFunction);
    }


    class ArichmetichTree : ProcessTree
    {


        //public ArichmetichTree left;
        //public ArichmetichTree right;
        //public ArichmetichTree(Token head, Token token)
        //{
        //    this.head = head;
        //    if (token != null)
        //        this.left = new ArichmetichTree(token, null);
        //    else
        //        this.left = null;
        //    right = null;
        //}
        //public void PutInTree(Token tokenVar, Token TokenOp)
        //{
        //    ArichmetichTree currentTree = this;
        //    while (currentTree.right != null)
        //        currentTree = currentTree.right;

        //    currentTree.right = new ArichmetichTree(TokenOp, tokenVar);
        //}

        //public void PutInTree(Token lastVar)
        //{
        //    ArichmetichTree currentTree = this;

        //    while (currentTree.right != null)
        //        currentTree = currentTree.right;

        //    currentTree.right = new ArichmetichTree(lastVar, null);

        //}

        //public override void Process(TreeFunction tree) // tree for update var
        //{
        //    TokenType type = this.right.head.type;
        //    double res;
        //    if (type == TokenType.NUMERIC_CONST || type == TokenType.VARIABLE)
        //        res = (double)(this.right.head as TokenNumeric).data;
        //    else
        //        res = Calculate(this.right);
        //    tree.UpdateVar((this.left.head as TokenVariable).name, res);
        //}

        //private double Calculate(ArichmetichTree tree) // Refactor! 
        //{
        //    if (tree.head.type == TokenType.NUMERIC_CONST || tree.head.type == TokenType.VARIABLE)
        //    {
        //        if (tree.head.type == TokenType.VARIABLE)
        //            return (double)(tree.head as TokenVariable).data;

        //        if (tree.head.type == TokenType.NUMERIC_CONST)
        //            return (double)(tree.head as TokenConst).data;
        //    }

        //    if (tree.head.type == TokenType.PLUS)
        //    {
        //        TokenType type = tree.left.head.type;
        //        if (type == TokenType.VARIABLE || type == TokenType.NUMERIC_CONST)
        //            return (double)(tree.left.head as TokenNumeric).data + Calculate(tree.right);
        //    }

        //    if (tree.head.type == TokenType.MINUS)
        //    {
        //        TokenType type = tree.left.head.type;
        //        if (type == TokenType.VARIABLE || type == TokenType.NUMERIC_CONST)
        //            return (double)(tree.left.head as TokenNumeric).data - Calculate(tree.right);
        //    }

        //    throw new Exception("Something bad in Calculate arichmetic tree!");
        //}

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
                        this.reversePolishNotation.Add(token);
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
            for (int i = 0; i < stack.Count; i++)
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
        public override void Process(TreeFunction treeFunction)
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
            treeFunction.UpdateVar(tokenToChange.name, (stack.Pop() as TokenNumeric).data);
        }
    }

    class TreeFunction : Tree
    {
        public ProcessTree next;
        public string name;
        private Dictionary<string, TokenVariable> stackVariable;
        public TreeFunction(string name, int startPos, int endPos)
        {
            this.name = name;
            this.head = new TokenFunction(startPos, endPos);
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
        public void UpdateVar(string name, object data)
        {
            stackVariable[name].data = data;
        }
    }
}
