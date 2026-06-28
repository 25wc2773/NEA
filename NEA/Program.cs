using System;
using System.Text;

namespace NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.BufferWidth = Console.WindowWidth;
            MatrixCalculator matrixCalculator = new MatrixCalculator();
            matrixCalculator.Clear();

            while (true)
            {
                string equation = Console.ReadLine();
                matrixCalculator.Evaluate(equation);
            }
        }
    }
}
