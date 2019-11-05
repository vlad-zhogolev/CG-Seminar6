using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roberts
{
    public class MyMatrix<T> where T : struct
    {
        private T[,] m_matrix = null;

        public MyMatrix(int size) : this(size, size) {}

        public MyMatrix(int height, int width)
        {
            if (height <= 0)
            {
                throw new ArgumentException("Matrix height can't be less than zero");
            }
            if (width <= 0)
            {
                throw new ArgumentException("Matrix width can't be less than zero");
            }
            m_matrix = new T[height, width];
        }

        public MyMatrix(T[,] values)
        {
            string errorMessage = null;
            if (values == null)
            {
                errorMessage = "Values parameter is null";
            }
            if (values.GetLength(0) <= 0)
            {
                errorMessage = "Values height must be greater than zero";
            }
            if (values.GetLength(1) <= 0)
            {
                errorMessage = "Values width must be greater than zero";
            }
            if (errorMessage != null)
            {
                throw new ArgumentException(errorMessage);
            }
            m_matrix = values;
        }

        public static MyMatrix<T> Incident(int size)
        {
            var result = new MyMatrix<T>(size);
            for (var i = 0; i < size; ++i)
            {
                result[i, i] = (dynamic)default(T) + 1;
            }
            return result;
        }

        public static MyMatrix<T> Incident(int size, T value)
        {
            var result = new MyMatrix<T>(size);
            for (var i = 0; i < size; ++i)
            {
                result[i, i] = (dynamic)default(T) + value;
            }
            return result;
        }

        public T this [int i, int j]
        {
            get { return m_matrix[i, j]; }
            set { m_matrix[i, j] = value; }
        }

        public int Height { get { return m_matrix.GetLength(0); } }

        public int Width { get { return m_matrix.GetLength(1); } }

        public static MyMatrix<T> operator * (MyMatrix<T> first, MyMatrix<T> second)
        {
            if (first.Width != second.Height)
            {
                throw new ArgumentException("Matrix multiplication: first matrix width must be equal to second matrix height");
            }
            var result = new MyMatrix<T>(first.m_matrix.GetLength(0), second.m_matrix.GetLength(1));
            for (var i = 0; i < first.Height; ++i)
            {
                for (var j = 0; j < second.Width; ++j)
                {
                    for (var k = 0; k < first.Width; ++k)
                    {
                        result[i, j] += (dynamic)first[i, k] * (dynamic)second[k, j]; // some cheats with dynamic
                    }
                }
            }
            return result;
        }

        public T[,] GetInternalStorage() { return m_matrix; }
    }
}
