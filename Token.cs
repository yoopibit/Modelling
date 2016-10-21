using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    enum TokenType { VARIABLE, NUMERIC_CONST, PLUS, MINUS, ASSIGN, FUNCTION, FOR, RETURN,
        ARITHMETIC_BRACKET_OPEN, ARITHMETIC_BRACKET_CLOSE, IF, ELSE, AND, OR, MORE, LESS, EQUAL, NOT_EQUAL,
        MULTIPLICATION, DIVIDE, EOF, END_OP};
    enum VariableType { NUMERIC, STRING, UNDEFINE};
    class Token
    {
        public TokenType type;
    }

    class TokenNumeric : Token // make abstarct
    {
        public object data;
    }

    class TokenConst : TokenNumeric
    {
        public TokenVariable ConvertToTokenVariable(string nameVar)
        {
            return new TokenVariable(nameVar) { type = TokenType.VARIABLE, varType = VariableType.NUMERIC, data = this.data };
        }
    }

    class TokenVariable : TokenNumeric
    {
        public string name;//{ get; }
        public VariableType varType;
        public TokenVariable(string name)
        {
            this.type = TokenType.VARIABLE;
            this.name = name;
            this.data = null;
            this.varType = VariableType.UNDEFINE;
        }

    }
    abstract class TokenLogic : Token
    {
        public int startPos;
        public int endPos;
    }
    class TokenFunction : TokenLogic
    {
        public string name;
        public TokenFunction(string name, int startPos, int endPos)
        {
            this.name = name;
            this.type = TokenType.FUNCTION;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }

    class TokenIfElse : TokenLogic
    {
        public TokenIfElse(int startPos, int endPos, TokenType type)
        {
            if (type != TokenType.IF && type != TokenType.ELSE)
                throw new Exception("Something bad in constructor ifElse token");
            this.type = type;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }

    class TokenFor: TokenLogic
    {
        public TokenFor(int startPos, int endPos, TokenType type)
        {
            if (type != TokenType.FOR)
                throw new Exception("Something bad in constructor for token");
            this.type = type;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }
}
