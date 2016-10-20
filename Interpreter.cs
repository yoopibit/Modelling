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
            TokenFunction tokenHead = tree.head as TokenFunction; 
            for (int i = tokenHead.startPos + 1; i < tokenHead.endPos;)
            {
                Token currentToken = GetNextToken(ref i, tree);
                switch (currentToken.type)
                {
                    case TokenType.VARIABLE:

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

                        break;
                    case TokenType.IF:
                        TreeIf ifTree = new TreeIf(currentToken as TokenIf);
                        // process statement

                        currentToken = GetNextToken(ref i, tree);
                        int deep;
                        if (currentToken.type == TokenType.ARITHMETIC_BRACKET_OPEN)
                            deep = 1;
                        else
                            throw new Exception("Expected open bracket in if statement line: " + i.ToString());

                        while(deep != 0)
                        {
                            currentToken = GetNextToken(ref i, tree);
                            double leftData = (double)(currentToken as TokenNumeric).data;
                            switch (currentToken.type)
                            {
                                case TokenType.ARITHMETIC_BRACKET_OPEN:
                                    ifTree.PutTokenInStatement(new Token() { type = TokenType.ARITHMETIC_BRACKET_OPEN });
                                    deep++;
                                    break;
                                case TokenType.ARITHMETIC_BRACKET_CLOSE:
                                    ifTree.PutTokenInStatement(new Token() { type = TokenType.ARITHMETIC_BRACKET_CLOSE });
                                    deep--;
                                    break;
                                case TokenType.VARIABLE:
                                case TokenType.NUMERIC_CONST:
                                    var tokenCondition = GetNextToken(ref i, tree);
                                    nextToken = GetNextToken(ref i, tree);
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
                                            if (leftData > rightData)
                                                resultSmallStation = 0;
                                            else
                                                resultSmallStation = 1;
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
                                            ifTree.PutTokenInStatement(new TokenNumeric() { type = TokenType.NUMERIC_CONST, data = resultSmallStation });
                                            break;
                                    }
                                    break;

                                case TokenType.AND:
                                    ifTree.PutTokenInStatement(new Token() { type = TokenType.MULTIPLICATION });
                                    break;

                                case TokenType.OR:
                                    ifTree.PutTokenInStatement(new Token() { type = TokenType.PLUS});
                                    break;

                                default:
                                    throw new Exception("Something went bad in if condition Line:" + i.ToString()); 
                            }
                        }
                        if (ifTree.ProcessStatement())
                        {
                            Process(ifTree);
                            i = (ifTree.head as TokenIf).endPos + 1;
                        }
                        break;
                }

                if (tree.next == null)
                    break;

                tree.next.Process(tree);
                tree.next = null;
            }
        }

        Token GetNextToken(ref int pos, TreeFunctional tree)
        {
            if (char.IsLetter(this.text[pos])) // is var
            {
                int start = pos;
                while (char.IsLetter(this.text[++pos])) ;
                string nameVar = this.text.Substring(start, pos - start);

                switch (nameVar)
                {
                    case "if":
                        return new TokenIf(pos + 1, EndBracket(pos, "if") );
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
