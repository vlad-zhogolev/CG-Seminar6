using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roberts
{
    class TransformFactory
    {
        public static MyMatrix<double> CreateTranslation(double dx, double dy, double dz)
        {
            return new MyMatrix<double>(new double[,] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { dx, dy, dz, 1  }
            });
        }

        public static MyMatrix<double> CreateScale(double xScale, double yScale, double zScale)
        {
            return new MyMatrix<double>(new double[,]
            {
                { xScale, 0,      0,      0 },
                { 0,      yScale, 0,      0 },
                { 0,      0,      zScale, 0 },
                { 0,      0,      0,      1 }
            });
        }

        public static MyMatrix<double> CreateOxRotation(double degrees)
        {
            var sin = Math.Sin(Utilities.ToRadians(degrees));
            var cos = Math.Cos(Utilities.ToRadians(degrees));
            return new MyMatrix<double>(new double[,]
            {
                { 1, 0,    0,   0 },
                { 0, cos,  sin, 0 },
                { 0, -sin, cos, 0 },
                { 0, 0,    0,   1 }
            });
        }

        public static MyMatrix<double> CreateOyRotation(double degrees)
        {
            var sin = Math.Sin(Utilities.ToRadians(degrees));
            var cos = Math.Cos(Utilities.ToRadians(degrees));
            return new MyMatrix<double>(new double[,]
            {
                { cos, 0, -sin, 0 },
                { 0,   1, 0,    0 },
                { sin, 0, cos,  0 },
                { 0,   0, 0,    1 }
            });
        }

        public static MyMatrix<double> CreateOzRotation(double degrees)
        {
            var sin = Math.Sin(Utilities.ToRadians(degrees));
            var cos = Math.Cos(Utilities.ToRadians(degrees));
            return new MyMatrix<double>(new double[,]
            {
                { cos,  sin, 0, 0 },
                { -sin, cos, 0, 0 },
                { 0,    0,   1, 0 },
                { 0,    0,   0, 1 }
            });
        }
    }
}
