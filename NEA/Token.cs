namespace NEA
{
    public enum TokenType
    {
        Operator,
        Matrix,
        Scalar,
        Error
    }
    internal class Token
    {
        private TokenType type;
        private Matrix matrixValue;
        private double scalarValue;
        private string operatorValue, errorMessage;

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
        public Token(string error, TokenType tokenType) //second parameter prevents confusion with operator construct
        {
            type = TokenType.Error;
            errorMessage = error;
        }
        public TokenType GetTokenType()
        {
            return type;
        }
        public Matrix GetMatrixValue() => matrixValue;
        public double GetScalarValue() => scalarValue;
        public string GetOperatorValue() => operatorValue;
        public string GetErrorMessage() => errorMessage;
    }
}
