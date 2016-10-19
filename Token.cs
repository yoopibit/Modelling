using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    enum TokenType { VARIABLE, NUMERIC_CONST, PLUS, MINUS, EQUAL, FUNCTION,EOF, END_OP};
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

        public TokenVariable(string name)
        {
            this.type = TokenType.VARIABLE;
            this.name = name;
        }

    }

    class TokenFunction : Token {
        public int startPos { get; }
        public int endPos { get; }

        public TokenFunction(int startPos, int endPos)
        {
            this.type = TokenType.FUNCTION;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }
}
