using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class MatrixCalculator
    {
        private Token solution;
        private Dictionary<char, Token> userVariables = new Dictionary<char, Token>
            {{'A', null}, {'B', null}, {'C', null}, {'D', null}, {'E', null}, {'F', null},
            {'x', null}, {'y', null},};

        public void Evaluate(string input)
        {
            try
            {
                input = input.Replace(" ", "");

                if (IsUserVariableAssignment(input))
                {
                    string definition = input.Substring(2);
                    userVariables[input[0]] = InternalEvaluate(definition);
                    solution = userVariables[input[0]];
                }
                else
                {
                    List<Token> tokenList = Tokenize(input);
                    Token[] rpnExpression = ConvertToRPN(tokenList);
                    solution = EvaluateRPN(rpnExpression);
                }
            }
            catch (Exception exception)
            {
                solution = new Token($"Error: {exception.Message}", TokenType.Error);
            }
        }
        private Token InternalEvaluate(string input)
        {
            return EvaluateRPN(ConvertToRPN(Tokenize(input)));
        }

        private List<Token> Tokenize(string input)
        {
            List<Token> tokens = new List<Token>();

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

                else if (IsDoubleComponent(currentChar))
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

                else if (i < input.Length - 2 && input.Substring(i, 3).ToUpper() == "ANS") //check if the input was 'Ans'
                {
                    if (solution == null || solution.GetTokenType() == TokenType.Error)
                    {
                        throw new ArgumentException("'Ans' does not currently have a value");
                    }

                    tokens.Add(solution);
                    i += 2;
                }

                else if (IsUserVariable(currentChar))
                {
                    Token value = userVariables[currentChar];
                    if (value == null) throw new ArgumentException($"Variable '{currentChar}' does not currently have a value");

                    tokens.Add(value);
                }

                else if (IsOperator(currentChar))
                {
                    string op = currentChar.ToString();

                    if (op == "-" && IsUnaryMinus(tokens))
                        tokens.Add(new Token("~"));
                    else
                        tokens.Add(new Token(op));

                }

                else
                {
                    throw new ArgumentException("Character in input not recognised");
                }
            }

            return tokens;
        }
        private Token[] ConvertToRPN(List<Token> setTokens)
        {
            List<Token> infixExpression = new List<Token>(setTokens);
            List<Token> postfixExpression = new List<Token>();

            Stack<string> operatorStack = new Stack<string>();

            foreach (Token token in infixExpression)
            {
                TokenType tokenType = token.GetTokenType();

                if (tokenType == TokenType.Matrix || tokenType == TokenType.Scalar)
                {
                    postfixExpression.Add(token);
                }
                //after this all tokens must be operators
                else if (token.GetOperatorValue() == "(")
                {
                    operatorStack.Push(token.GetOperatorValue());
                }
                else if (token.GetOperatorValue() == ")")
                {
                    while (operatorStack.Count != 0 && operatorStack.Peek() != "(")
                    {
                        postfixExpression.Add(new Token(operatorStack.Pop()));
                    }
                    operatorStack.Pop();
                }
                else
                {
                    while (operatorStack.Count != 0 && HasLeftAssociativity(token) &&
                        GetPrecedence(token.GetOperatorValue()) <= GetPrecedence(operatorStack.Peek()))
                    {
                        postfixExpression.Add(new Token(operatorStack.Pop()));
                    }
                    operatorStack.Push(token.GetOperatorValue());
                }
            }
            while (operatorStack.Count != 0)
            {
                if (operatorStack.Peek() == "(")
                {
                    throw new ArgumentException("Syntax Error");
                }
                postfixExpression.Add(new Token(operatorStack.Pop()));
            }

            return postfixExpression.ToArray();
        }
        private Token EvaluateRPN(Token[] tokens)
        {
            Stack<Token> evaluateStack = new Stack<Token>();
            Token left, right;
            TokenType leftType, rightType;

            foreach (Token token in tokens)
            {
                TokenType tokenType = token.GetTokenType();

                if (tokenType != TokenType.Operator)
                {
                    evaluateStack.Push(token);
                }
                else //token is a operator
                {
                    string op = token.GetOperatorValue();
                    int arity = GetArity(op);

                    if (arity == 1)
                    {
                        right = evaluateStack.Pop();
                        rightType = right.GetTokenType();

                        switch (op)
                        {
                            case "~":
                                if (rightType == TokenType.Scalar)
                                    evaluateStack.Push(new Token(-right.GetScalarValue()));
                                else //matrix
                                    evaluateStack.Push(new Token(-right.GetMatrixValue()));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    else if (arity == 2)
                    {
                        right = evaluateStack.Pop();
                        left = evaluateStack.Pop();
                        rightType = right.GetTokenType();
                        leftType = left.GetTokenType();

                        if (!IsSameTokenType(rightType, leftType))
                        {
                            double scalar;
                            Matrix matrix;

                            if (rightType == TokenType.Scalar)
                            {
                                scalar = right.GetScalarValue();
                                matrix = left.GetMatrixValue();
                            }
                            else
                            {
                                matrix = right.GetMatrixValue();
                                scalar = left.GetScalarValue();
                            }

                            switch (op)
                            {
                                case "*":
                                    evaluateStack.Push(new Token(scalar * matrix));
                                    break;
                                case "^":
                                    evaluateStack.Push(new Token(matrix ^ scalar));
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (rightType == TokenType.Scalar) //both tokens are scalars
                        {
                            double rightValue = right.GetScalarValue();
                            double leftValue = left.GetScalarValue();

                            switch (op)
                            {
                                case "+":
                                    evaluateStack.Push(new Token(leftValue + rightValue));
                                    break;
                                case "-":
                                    evaluateStack.Push(new Token(leftValue - rightValue));
                                    break;
                                case "*":
                                    evaluateStack.Push(new Token(leftValue * rightValue));
                                    break;
                                case "/":
                                    evaluateStack.Push(new Token(leftValue / rightValue));
                                    break;
                                case "^":
                                    evaluateStack.Push(new Token(Math.Pow(leftValue, rightValue)));
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else //both tokens are matrices
                        {
                            Matrix rightValue = right.GetMatrixValue();
                            Matrix leftValue = left.GetMatrixValue();

                            switch (op)
                            {
                                case "+":
                                    evaluateStack.Push(new Token(leftValue + rightValue));
                                    break;
                                case "-":
                                    evaluateStack.Push(new Token(leftValue - rightValue));
                                    break;
                                case "*":
                                    evaluateStack.Push(new Token(leftValue * rightValue));
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                }
            }
            return evaluateStack.Pop();
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
                    try
                    {
                        data[row, col] = double.Parse(currentColumnContents[col]);
                    }
                    catch //the cell may be an equation
                    {
                        Token solutionToCell = InternalEvaluate(currentColumnContents[col]);
                        if (solutionToCell.GetTokenType() == TokenType.Scalar)
                        {
                            data[row, col] = solutionToCell.GetScalarValue();
                        }
                        else
                        {
                            throw new ArgumentException("Syntax error");
                        }
                    }
                }
            }

            Matrix matrix = new Matrix(data);
            return new Token(matrix);
        }

        private void Print(List<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                TokenType type = token.GetTokenType();
                if (type == TokenType.Matrix) Console.Write(token.GetMatrixValue().ToString());
                else if (type == TokenType.Scalar) Console.Write(token.GetScalarValue());
                else if (type == TokenType.Operator) Console.Write(token.GetOperatorValue());

                Console.Write(" ");
            }
            Console.WriteLine();
        }
        private void Print(Token token)
        {
            TokenType type = token.GetTokenType();
            if (type == TokenType.Matrix) Console.WriteLine(token.GetMatrixValue().ToString());
            else if (type == TokenType.Scalar) Console.WriteLine(token.GetScalarValue());
            else if (type == TokenType.Operator) Console.WriteLine(token.GetOperatorValue());
        }
        public void Print()
        {
            TokenType type = solution.GetTokenType();

            string solutionToString = null;

            if (type == TokenType.Matrix) solutionToString = solution.GetMatrixValue().ToString();
            else if (type == TokenType.Scalar) solutionToString = solution.GetScalarValue().ToString();
            else if (type == TokenType.Operator) solutionToString = solution.GetOperatorValue();
            else if (type == TokenType.Error) solutionToString = solution.GetErrorMessage();

            string[] lines = solutionToString.Split('\n');

            Console.WriteLine($"= {lines[0]}".PadLeft(Console.WindowWidth));
            for (int i = 1; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i].PadLeft(Console.WindowWidth));
            }
        }

        private bool IsDoubleComponent(char c) => (char.IsDigit(c) || c == '.');
        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '^' ||
                c == '(' || c == ')';
        }
        private bool IsUserVariable(char c)
        {
            return c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' ||
                c == 'F' || c == 'x' || c == 'y';
        }
        private bool IsSameTokenType(TokenType right, TokenType left) => right == left;
        private bool IsUnaryMinus(List<Token> tokens)
        {
            if (tokens.Count == 0) return true; //it is the first token

            Token recentToken = tokens.Last();

            if (recentToken.GetTokenType() == TokenType.Operator)
            {
                if (recentToken.GetOperatorValue() != ")")
                {
                    return true;
                }
            }

            return false;
        }
        private bool IsUserVariableAssignment(string input)
        {
            if (input.Length >= 2 && input[1] == '=')
            {
                if (IsUserVariable(input[0])) return true;
                else throw new ArgumentException($"'{input[0]}' is not a valid user variable");
            }
            return false;
        }

        private int GetPrecedence(string op)
        {
            if (op == "+" || op == "-")
            {
                return 1;
            }
            else if (op == "*" || op == "/")
            {
                return 2;
            }
            else if (op == "~")
            {
                return 3;
            }
            else if (op == "^")
            {
                return 4;
            }
            else
            {
                return -1;
            }
        }
        private int GetArity(string op)
        {
            if (op == "+" || op == "-" || op == "*" || op == "/" || op == "^")
            {
                return 2;
            }
            if (op == "~")
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        private bool HasLeftAssociativity(Token token)
        {
            string currentOperator = token.GetOperatorValue();

            if (currentOperator == "+" || currentOperator == "-" || currentOperator == "*"
                || currentOperator == "/")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Token GetSolution() => solution;
    }
}
