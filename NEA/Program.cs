using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            Matrix matrix1 = new Matrix(new double[2, 2] { 
                { 1, 2 },
                { 3, 4 } });
            Matrix matrix2 = new Matrix(new double[2, 1] { 
                { 4 }, 
                { 1 } });

            Matrix matrix3 = matrix2.Transpose();

            string equation = "6 + (2 - 1 + - 1 * (2 + (2 - 1) ) )";
            MatrixCalculator matrixCalculator = new MatrixCalculator(equation);


            //⎡⎢⎣⎤⎥⎦[
        }
    }
}
