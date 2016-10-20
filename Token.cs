using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    enum TokenType { VARIABLE, NUMERIC_CONST, PLUS, MINUS, ASSIGN, FUNCTION,
        ARITHMETIC_BRACKET_OPEN, ARITHMETIC_BRACKET_CLOSE, IF, AND, OR, MORE, LESS, EQUAL, NOT_EQUAL,
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
        public TokenFunction(int startPos, int endPos)
        {
            this.type = TokenType.FUNCTION;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }

    class TokenIf : TokenLogic
    {
        public TokenIf(int startPos, int endPos)
        {
            this.type = TokenType.IF;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }

}
