using System;
using System.Data;
using System.Text;

namespace NEA
{
    internal class Matrix
    {
        private double[,] values;
        private int rows;
        private int cols;

        public Matrix(double[,] input)
        {
            values = input;
            rows = values.GetLength(0);
            cols = values.GetLength(1);
        }
        public static Matrix operator +(Matrix left, Matrix right)
        {
            if (left.rows != right.rows || left.cols != right.cols)
            {
                throw new ArgumentException("Matrices must be the same dimensions to be added.");
            }

            double[,] result = new double[left.rows, left.cols];
            for (int row = 0; row < left.rows; row++)
            {
                for (int col = 0; col < left.cols; col++)
                {
                    result[row, col] = left.values[row, col] + right.values[row, col];
                }
            }

            return new Matrix(result);
        }
        public static Matrix operator -(Matrix left, Matrix right)
        {
            if (left.rows != right.rows || left.cols != right.cols)
            {
                throw new ArgumentException("Matrices must be the same dimensions to be added.");
            }

            double[,] result = new double[left.rows, left.cols];
            for (int row = 0; row < left.rows; row++)
            {
                for (int col = 0; col < left.cols; col++)
                {
                    result[row, col] = left.values[row, col] - right.values[row, col];
                }
            }

            return new Matrix(result);
        }
        public static Matrix operator -(Matrix matrix)
        {
            double[,] result = new double[matrix.rows, matrix.cols];
            for (int row = 0; row < matrix.rows; row++)
            {
                for (int col = 0; col < matrix.cols; col++)
                {
                    result[row, col] = -matrix.values[row, col];
                }
            }

            return new Matrix(result);
        }
        public static Matrix operator *(Matrix left, Matrix right)
        {
            if (left.cols != right.rows)
            {
                throw new ArgumentException("Invalid sizes for matrix multiplication");
            }

            double[,] result = new double[left.rows, right.cols];

            for (int row = 0; row < left.rows; row++)
            {
                for (int col = 0; col < right.cols; col++)
                {
                    double currentTotal = 0;
                    for (int position = 0; position < left.cols; position++)
                    {
                        currentTotal += left.values[row, position] * right.values[position, col];
                    }
                    result[row, col] = currentTotal;
                }
            }

            return new Matrix(result);
        }
        public static Matrix operator *(double scalar, Matrix matrix)
        {
            double[,] result = new double[matrix.rows, matrix.cols];
            for (int row = 0; row < matrix.rows; row++)
            {
                for (int col = 0; col < matrix.cols; col++)
                {
                    result[row, col] = scalar * matrix.values[row, col];
                }
            }

            return new Matrix(result);
        }
        public static Matrix operator *(Matrix matrix, double scalar) => scalar * matrix;
        public static Matrix operator ^(Matrix matrix, double setPower)
        {
            if (matrix.rows != matrix.cols) throw new ArgumentException("You can only raise a square matrix to a power");
            if (setPower == 0) return matrix.Identity(matrix.rows);

            int power;

            if (setPower % 1 != 0) throw new ArgumentException("Power must be a whole number");
            else power = (int)setPower;

            Matrix result = matrix;

            for (int i = 1; i < power; i++)
            {
                result = result * matrix;
            }

            return result;
        }
        public Matrix Transpose()
        {
            double[,] result = new double[cols, rows];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    result[col, row] = values[row, col];
                }
            }

            return new Matrix(result);
        }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            string[,] matrixToString = new string[rows, cols];
            string currentValueToString;
            int[] colMaxLengths = new int[cols];
            int currentValueLength;

            for (int col = 0; col < cols; col++)
            {
                colMaxLengths[col] = 0;
                for (int row = 0; row < rows; row++)
                {
                    currentValueToString = values[row, col].ToString();
                    currentValueLength = currentValueToString.Length;

                    if (currentValueLength > colMaxLengths[col]) colMaxLengths[col] = currentValueLength;
                    matrixToString[row, col] = currentValueToString;
                }
            }

            for (int row = 0; row < rows; row++)
            {
                result.Append("|");
                for (int col = 0; col < cols; col++)
                {
                    result.Append(matrixToString[row, col].PadRight(colMaxLengths[col]));
                    if (col != cols - 1) result.Append(" ");
                }
                result.Append("|");
                if (row != rows - 1) result.Append("\n");
            }

            return result.ToString();
        }
        public Matrix Identity(int size)
        {
            double[,] result = new double[size, size];

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (row == col) result[row, col] = 1;
                    else result[row, col] = 0;
                }
            }

            return new Matrix(result);
        }
    }
}
