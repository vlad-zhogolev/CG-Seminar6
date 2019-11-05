using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roberts
{
    class Utilities
    {
        public static double ToRadians(double degrees) { return degrees * Math.PI / 180.0; }

        public static double Length(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2));
        }

        public static MyMatrix<double> Inverse(MyMatrix<double> matrix)
        {
            var array1d = matrix.GetInternalStorage().Cast<double>().ToArray();
            var mathnetMatrix = new DenseMatrix(matrix.Height, matrix.Width, array1d);
            var inversedMathnetMatrix = mathnetMatrix.Inverse();
            var result = new MyMatrix<double>(inversedMathnetMatrix.RowCount, inversedMathnetMatrix.ColumnCount);
            for (int i = 0 ; i < inversedMathnetMatrix.RowCount ; ++i )
            {
                for (int j = 0 ; j < inversedMathnetMatrix.ColumnCount ; ++j)
                {
                    result[i, j] = inversedMathnetMatrix[i, j];
                }
            }
            return result;
        }
    }
}
