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
        private TreeFunction mainTree;
        public Interpreter(RichTextBox richTextBoxResult)
        {
          //  tokensFunstions.Add("main", new List<Token>());
            this.richTextBoxResult = richTextBoxResult;
        }

        public object[] Execute(string programText, object[] args)
        {
            this.text = programText;
            RemoveExtraSpaces();
            FindMain();
            Process(mainTree); 
            return null;
        }

        void RemoveExtraSpaces()
        {
            StringBuilder withoutSpace = new StringBuilder(this.text);

            int j = 0;
            for (int i = 0; i < this.text.Length; i++)
            {
                if (this.text[i] != ' ' && this.text[i] != '\n')
                {
                    withoutSpace[j++] = this.text[i];
                }
            }
            this.text = withoutSpace.ToString().Substring(0, j);
        }

        void FindMain()
        {
            string nameMain = "defmain()";
            int start = this.text.IndexOf(nameMain) + nameMain.Length;
            if (this.text.LastIndexOf(nameMain) != 0)
                throw new Exception("Multiplied definition of main");

            int end = EndBracket(start - 1, "main");
            mainTree = new TreeFunction("main", start, end);
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
        
        void Process(TreeFunction tree)
        {
            TokenFunction tokenHead = tree.head as TokenFunction; 
            for (int i = tokenHead.startPos + 1; i < tokenHead.endPos; i++)
            {
                Token currentToken = GetNextToken(ref i, tree);
                if (currentToken.type == TokenType.VARIABLE)
                {
                    Token nextToken = GetNextToken(ref i, tree);
                    if (nextToken.type == TokenType.EQUAL)
                    {
                        ArichmetichTree arTree = new ArichmetichTree(currentToken as TokenVariable);
                        tree.next = arTree;
                        nextToken = GetNextToken(ref i, tree);
                        while (nextToken.type != TokenType.END_OP)
                        {
                            arTree.PutToken(nextToken);
                            nextToken = GetNextToken(ref i, tree);
                            TokenType type = nextToken.type;
                        }
                        //while (true)
                        //{
                        //    currentToken = GetNextToken(ref i, tree);
                        //    TokenType type = currentToken.type;
                        //    if (type != TokenType.VARIABLE && type != TokenType.NUMERIC_CONST)
                        //        throw new Exception("Not correct arichmetic expression. Line: " + i.ToString());

                        //    nextToken = GetNextToken(ref i, tree);
                        //    type = nextToken.type;
                        //    if (type == TokenType.PLUS || type == TokenType.MINUS)
                        //    {
                        //        arTree.PutInTree(currentToken, nextToken);
                        //    }
                        //    else if (type == TokenType.END_OP)
                        //    {
                        //        arTree.PutInTree(currentToken);
                        //        break;
                        //    }
                        //    else
                        //        throw new Exception("Not correct arichmetic expression. Line: " + i.ToString());
                        //}
                    }
                }
                tree.next.Process(tree);
                tree.next = null;
            }
        }

        Token GetNextToken(ref int pos, Tree tree)
        {
            if (char.IsLetter(this.text[pos]))
            {
                int start = pos;
                while (char.IsLetter(this.text[++pos])) ;
                if (this.text[pos] == '=')
                {
                    string str = this.text.Substring(start, pos - start);
                    TokenVariable var = new TokenVariable(str);
                    (tree as TreeFunction).PutVariableinStack(var);
                    return var;
                } else
                {
                    return (tree as TreeFunction).GetVar(this.text.Substring(start, pos - start)); // check if not found
                }
            }
            if(text[pos] == '=')
            {
                ++pos;
                return new Token() { type = TokenType.EQUAL };
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
            if (char.IsDigit(this.text[pos]))
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
