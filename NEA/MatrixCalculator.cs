using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class MatrixCalculator
    {
        public MatrixCalculator(string input)
        {
            List<Token> tokenList = Tokenize(input);
            Print(tokenList);
        }
        public List<Token> Tokenize(string input)
        {
            List<Token> tokens = new List<Token>();
            input = input.Replace(" ", "");

            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];

                if (currentChar == '[') //then there is a matrix ahead
                {
                    int matrixStartIndex = i;
                    int matrixCloseIndex = i;

                    for (int j = matrixStartIndex; j < input.Length; j++)
                    {
                        matrixCloseIndex = j;
                        if (input[j] == ']') break;
                        if (j == input.Length - 1) throw new ArgumentException("Matrix does not close");
                    }

                    int matrixContentsLength = matrixCloseIndex - matrixStartIndex - 1;
                    string matrixContents = input.Substring(matrixStartIndex + 1, matrixContentsLength);

                    tokens.Add(ParseMatrixContents(matrixContents));

                    i = matrixCloseIndex; //skip ahead to after the matrix
                }

               else if (char.IsDigit(currentChar) || (currentChar == '-' && (i == 0 || IsOperator(input[i-1]))))
               {
                    int doubleStartIndex = i;
                    int doubleEndIndex = i + 1;

                    while (doubleEndIndex < input.Length && IsDoubleComponent(input[doubleEndIndex]))
                    {
                        doubleEndIndex++;
                    }

                    int doubleLength = doubleEndIndex - doubleStartIndex;
                    string stringDouble = input.Substring(doubleStartIndex, doubleLength);
                    double setToken = double.Parse(stringDouble);

                    tokens.Add(new Token(setToken));

                    i = doubleEndIndex - 1;
               }

                else if (IsOperator(currentChar))
                {
                    tokens.Add(new Token(currentChar.ToString()));
                }
            }

            return tokens;
        }

        private Token ParseMatrixContents(string matrixContents)
        {
            string[] rowContents = matrixContents.Split(';');
            int matrixRows = rowContents.Length;
            int matrixColumns = rowContents[0].Split(',').Length;

            double[,] data = new double[matrixRows, matrixColumns];

            for (int row = 0; row < matrixRows; row++)
            {
                string[] currentColumnContents = rowContents[row].Split(',');

                if (currentColumnContents.Length != matrixColumns)
                {
                    throw new ArgumentException("Invalid matrix");
                }

                for (int col = 0; col < matrixColumns; col++)
                {
                    data[row, col] = double.Parse(currentColumnContents[col]);
                }
            }

            Matrix matrix = new Matrix(data);
            return new Token(matrix);
        }
        private void Print(List<Token> tokens)
        {
            foreach(Token token in tokens)
            {
                TokenType type = token.GetTokenType();
                if (type == TokenType.Matrix) Console.Write(token.GetMatrixValue().ToString());
                else if (type == TokenType.Scalar) Console.Write(token.GetScalarValue());
                else if (type == TokenType.Operator) Console.Write(token.GetOperatorValue());

                Console.Write(" ");
            }
        }

        private bool IsDoubleComponent(char c) => (char.IsDigit(c) || c == '.');
        private bool IsOperator(char c)
        {
            return (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' ||
                c == '(' || c == ')');
        }
    }
}
