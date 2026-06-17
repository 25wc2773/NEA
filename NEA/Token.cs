using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    public enum TokenType
    {
        Operator,
        Matrix,
        Scalar
    }
    internal class Token
    {
        private TokenType type;
        private Matrix matrixValue;
        private double scalarValue;
        private string operatorValue;

        public Token(string data)
        {
            type = TokenType.Operator;
            operatorValue = data;
        }
        public Token(Matrix data)
        {
            type = TokenType.Matrix;
            matrixValue = data;
        }
        public Token(double data)
        {
            type = TokenType.Scalar;
            scalarValue = data;
        }
        public TokenType GetTokenType()
        {
            return type;
        }
        public Matrix GetMatrixValue() => matrixValue;
        public double GetScalarValue() => scalarValue;
        public string GetOperatorValue() => operatorValue;
    }
}
