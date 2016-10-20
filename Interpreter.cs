using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interpreter
{
    public interface IInterpretator
    {
        object[] Execute(string programText, object[] args);
    }

    class Interpreter : IInterpretator
    {
        private string text;

        private RichTextBox richTextBoxResult;
        //private Dictionary<string, List<Token>> tokensFunstions;
        private TreeFunctional mainTree;
        public Interpreter(RichTextBox richTextBoxResult)
        {
          //  tokensFunstions.Add("main", new List<Token>());
            this.richTextBoxResult = richTextBoxResult;
        }

        public object[] Execute(string programText, object[] args)
        {
            this.text = programText;
            FindMain();
            Process(mainTree); 
            return null;
        }
        
        void FindMain()
        {
            string nameMain = "main()";
            int start = this.text.IndexOf(nameMain) + nameMain.Length;
            int last = this.text.LastIndexOf(nameMain);
            if (last + nameMain.Length != start)
                throw new Exception("Multiplied definition of main");
            while (this.text[start] == ' ' || this.text[start] == '\n')
                ++start;
            int end = EndBracket(start - 1, "main");
            mainTree = new TreeFunctional("main", TokenType.FUNCTION, start, end);
        }

        int EndBracket(int start, string nameFunction)
        {
            int end = -1;
            bool firstZero = true;
            int countBracket = 0;
            for (int i = start; i < this.text.Length; i++)
            {
                if (this.text[i] == '{')
                    countBracket++;
                else
                if (this.text[i] == '}')
                    countBracket--;

                if (countBracket == 0 && !firstZero)
                {
                    end = i;
                    break;
                }
                if (countBracket != 0)
                    firstZero = false;
            }
            if (countBracket != 0)
            {
                throw new Exception("Not equal number of bracket in " + nameFunction);
            }
            return end;
        }
        
        void Process(TreeFunctional tree)
        {
            TokenLogic tokenHead = tree.head as TokenLogic; 
            for (int i = tokenHead.startPos + 1; i < tokenHead.endPos;)
            {
                Token currentToken = GetNextToken(ref i, tree);
                if (currentToken == null)
                    break;
                switch (currentToken.type)
                {
                    case TokenType.VARIABLE:
                        ProcessVariable(tree, ref i, ref currentToken);
                        break;

                    case TokenType.IF:
                        ProcessIfElseStatement(tree, ref i, ref currentToken);
                        break;

                    case TokenType.FOR:
                        ProcessFor(tree, ref i, ref currentToken);
                        break;
                }

                tree.next.Process(tree);
                tree.next = null;
            }
        }

        private void ProcessFor(TreeFunctional tree, ref int i, ref Token currentToken)
        {
            TokenFor forToken = currentToken as TokenFor;
            currentToken = GetNextToken(ref i, tree);
            if (currentToken.type != TokenType.ARITHMETIC_BRACKET_OPEN)
                throw new Exception("Expected open bracket after for, Line: " + i.ToString());

            currentToken = GetNextToken(ref i, tree);
            TokenVariable forVar = null;
            if (currentToken.type == TokenType.VARIABLE)
                forVar = currentToken as TokenVariable;

            int endPos = i;
            if (forVar != null)
            {
                while (this.text[++endPos] != ';') ;
                TreeFunctional treeForInitForVar = new TreeFunctional(null, TokenType.FUNCTION, i - forVar.name.Length - 1, endPos);
                Process(treeForInitForVar);
                forVar.data = treeForInitForVar.stackVariable[forVar.name].data;
                i = endPos + 1;
            }

            List<Token> forStatement = new List<Token>();
            currentToken = GetNextToken(ref i, tree);
            while (currentToken.type != TokenType.END_OP)
            {
                forStatement.Add(currentToken);
                currentToken = GetNextToken(ref i, tree);
            }

            currentToken = GetNextToken(ref i, tree);
            ArichmetichTree updateVarTree = null;
            if (currentToken.type != TokenType.ARITHMETIC_BRACKET_CLOSE)
            {
                if (currentToken.type != TokenType.VARIABLE)
                    throw new Exception("Expect var in update var posution in for, Line: " + i.ToString());
                updateVarTree = new ArichmetichTree(currentToken as TokenVariable);
                currentToken = GetNextToken(ref i, tree);
                if (currentToken.type != TokenType.ASSIGN)
                    throw new Exception("Expect assign in update pos in for, Line: " + i.ToString());

                currentToken = GetNextToken(ref i, tree);
                while (currentToken.type != TokenType.ARITHMETIC_BRACKET_CLOSE)
                {
                    updateVarTree.PutToken(currentToken);
                    currentToken = GetNextToken(ref i, tree);
                }
            }

            TreeFor forTree = new TreeFor(forToken, forVar, updateVarTree, tree);
            while (forTree.ProcessStatement(forStatement, i))
            {
                Process(forTree);
                forTree.ProcessUpdateForVar();
            }
            i = (forTree.head as TokenLogic).endPos + 1;
        }

        private void ProcessIfElseStatement(TreeFunctional tree, ref int i, ref Token currentToken)
        {
            TreeIf ifTree = new TreeIf(currentToken as TokenIfElse, tree);
            // process statement

            currentToken = GetNextToken(ref i, tree);
            int deep;
            if (currentToken.type == TokenType.ARITHMETIC_BRACKET_OPEN)
            {
                deep = 1;
                ifTree.PutTokenInStatement(currentToken, null, null, i);
            }
            else
                throw new Exception("Expected open bracket in if statement line: " + i.ToString());

            while (deep != 0)
            {
                currentToken = GetNextToken(ref i, tree);
                if (currentToken.type == TokenType.ARITHMETIC_BRACKET_OPEN)
                    deep++;
                else
                if (currentToken.type == TokenType.ARITHMETIC_BRACKET_CLOSE)
                    deep--;
                if (currentToken.type == TokenType.VARIABLE || currentToken.type == TokenType.NUMERIC_CONST)
                {
                    var tokenOperation = GetNextToken(ref i, tree);
                    var rightVariable = GetNextToken(ref i, tree);
                    ifTree.PutTokenInStatement(currentToken, tokenOperation, rightVariable, i);
                }
                else
                {
                    ifTree.PutTokenInStatement(currentToken, null, null, i);
                }
            }
            (ifTree.head as TokenIfElse).startPos = i;
            i = (ifTree.head as TokenIfElse).endPos + 1;

            if (ifTree.ProcessStatement())
            {
                Process(ifTree);
            }
            else
            {
                currentToken = GetNextToken(ref i, tree);
                if (currentToken.type == TokenType.ELSE)
                {
                    ifTree.head = currentToken;
                    Process(ifTree);
                    i = (ifTree.head as TokenIfElse).endPos + 1;
                }
            }
        }

        private void ProcessVariable(TreeFunctional tree, ref int i, ref Token currentToken)
        {
            Token nextToken = GetNextToken(ref i, tree);
            if (nextToken.type == TokenType.ASSIGN)
            {
                nextToken = GetNextToken(ref i, tree);

                var type = nextToken.type;

                if (type == TokenType.NUMERIC_CONST || type == TokenType.ARITHMETIC_BRACKET_OPEN ||
                    (type == TokenType.VARIABLE && (nextToken as TokenVariable).varType != VariableType.STRING))
                {
                    ArichmetichTree arTree = new ArichmetichTree(currentToken as TokenVariable);
                    tree.next = arTree;
                    while (nextToken.type != TokenType.END_OP)
                    {
                        arTree.PutToken(nextToken);
                        nextToken = GetNextToken(ref i, tree);
                        type = nextToken.type;
                    }
                }
                else
                if (type == TokenType.VARIABLE && (type == TokenType.VARIABLE && (nextToken as TokenVariable).varType != VariableType.NUMERIC))
                {
                    StringTree strTree = new StringTree(currentToken as TokenVariable);
                    tree.next = strTree;
                    currentToken = nextToken;
                    while (currentToken.type != TokenType.END_OP)
                    {
                        strTree.AddToken(currentToken as TokenVariable);
                        currentToken = GetNextToken(ref i, tree);
                        if (currentToken.type == TokenType.END_OP)
                            break;
                        if (currentToken.type != TokenType.PLUS)
                            throw new Exception("Expect \'+\' in line:" + i.ToString());

                        currentToken = GetNextToken(ref i, tree);
                    }
                }
                else
                    throw new Exception("Somethin bad in Process");
            }
        }

        public Token GetNextToken(ref int pos, TreeFunctional tree)
        {
            char tmp = text[pos];
            while (this.text[pos] == ' ' || this.text[pos] == '\n')
                ++pos;

            if (char.IsLetter(this.text[pos])) // is var
            {
                int start = pos;
                while (char.IsLetter(this.text[++pos])) ;
                string nameVar = this.text.Substring(start, pos - start);

                switch (nameVar)
                {
                    case "if":
                        return new TokenIfElse(pos, EndBracket(pos, "if"), TokenType.IF );
                    case "else":
                        return new TokenIfElse(pos, EndBracket(pos, "else"), TokenType.ELSE );
                    case "for":
                        start = pos;
                        while (this.text[++start] != '{') ;
                        return new TokenFor(start, EndBracket(start, "for"), TokenType.FOR); 
                    case "AND":
                        return new Token() { type = TokenType.AND };
                    case "OR":
                        return new Token() { type = TokenType.OR };
                    case "MORE":
                        return new Token() { type = TokenType.MORE };
                    case "LESS":
                        return new Token() { type = TokenType.LESS };
                    case "EQ":
                        return new Token() { type = TokenType.EQUAL };
                    case "NEQ":
                        return new Token() { type = TokenType.NOT_EQUAL };
                }

                if (tree.VariableExist(nameVar))
                {
                    return tree.GetVar(nameVar);
                }
                else
                {
                    TokenVariable var = new TokenVariable(nameVar);
                    tree.PutVariableinStack(var);
                    return var;
                }
            }
            if(text[pos] == '=')
            {
                ++pos;
                return new Token() { type = TokenType.ASSIGN };
            }
            if (text[pos] == '+')
            {
                ++pos;
                return new Token() { type = TokenType.PLUS };
            }
            if (text[pos] == '-')
            {
                ++pos;
                return new Token() { type = TokenType.MINUS };
            }
            if (text[pos] == '*')
            {
                ++pos;
                return new Token() { type = TokenType.MULTIPLICATION };
            }
            if (text[pos] == '/')
            {
                ++pos;
                return new Token() { type = TokenType.PLUS };
            }
            if (text[pos] == '(')
            {
                ++pos; return new Token() { type = TokenType.ARITHMETIC_BRACKET_OPEN };
            }
            if (text[pos] == ')')
            {
                ++pos; return new Token() { type = TokenType.ARITHMETIC_BRACKET_CLOSE };
            }
            if (text[pos] == '\"')
            {
                int start = pos;
                char c;
                while ((c = text[++pos]) != '\"')
                {
                    if (c == '\n')
                        throw new Exception("Not close string in line: " + pos.ToString());
                }
                string str = this.text.Substring(start + 1, pos - start - 1);
                ++pos;
                return new TokenVariable(null) { type = TokenType.VARIABLE, varType = VariableType.STRING, data = str };
            }
            if (char.IsDigit(this.text[pos])) // is Const
            {
                int start = pos;
                while (char.IsDigit(this.text[++pos])) ;
                string str = this.text.Substring(start, pos - start);
                double value = Convert.ToDouble(str);
                return new TokenConst() { type = TokenType.NUMERIC_CONST, data = value };
            }
            if (this.text[pos] == ';')
            {
                ++pos;
                return new Token() { type = TokenType.END_OP };
            }
            return null;
        }
    }
}
