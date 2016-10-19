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
    
    class ArichmetichTree : Tree
    {
        public ArichmetichTree left;
        public ArichmetichTree right;
        public ArichmetichTree(Token head, TokenVariable token)
        {
            this.head = head;
            if (token != null)
                this.left = new ArichmetichTree(token, null);
            else
                this.left = null;
            right = null;
        }
        public void PutInTree(Token tokenVar, Token TokenOp)
        {
            ArichmetichTree currentTree = this;
            while (currentTree.right != null)
                currentTree = currentTree.right;

            currentTree = new ArichmetichTree(TokenOp, tokenVar as TokenVariable);
        }

        public void PutInTree(Token lastVar)
        {
            ArichmetichTree currentTree = this;

            while (currentTree.right != null)
                currentTree = currentTree.right;

            currentTree.right = new ArichmetichTree(lastVar, null);

        }

        public void ProcessTree(TreeFunction tree) // tree for update var
        {
            TokenType type = this.right.head.type;
            double res;
            if (type == TokenType.NUMERIC_CONST || type == TokenType.VARIABLE)
                res = (double)(this.right.head as TokenNumeric).data;
            else
                res = Calculate(this.right);
            tree.UpdateVar((this.left.head as TokenVariable).name, res);
        }

        private double Calculate(ArichmetichTree tree) // Refactor! 
        {
            if (tree.head.type == TokenType.NUMERIC_CONST || tree.head.type == TokenType.VARIABLE)
            {
                if (tree.head.type == TokenType.VARIABLE)
                    return (double)(tree.left.head as TokenVariable).data;

                if (tree.head.type == TokenType.NUMERIC_CONST)
                    return (double)(tree.left.head as TokenConst).data;
            }

            if (tree.head.type == TokenType.PLUS)
            {
                if (tree.head.type == TokenType.VARIABLE)
                    return (double)(tree.left.head as TokenVariable).data + Calculate(tree.right);

                if (tree.head.type == TokenType.NUMERIC_CONST)
                    return (double)(tree.left.head as TokenConst).data + Calculate(tree.right);

            }

            if (tree.head.type == TokenType.MINUS)
            {
                if (tree.head.type == TokenType.VARIABLE)
                    return (double)(tree.left.head as TokenVariable).data - Calculate(tree.right);

                if (tree.head.type == TokenType.NUMERIC_CONST)
                    return (double)(tree.left.head as TokenConst).data - Calculate(tree.right);
            }
            throw new Exception("Something bad in Calculate arichmetic tree!");
        }
    }

    class TreeFunction : Tree
    {
        public Tree next;
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
