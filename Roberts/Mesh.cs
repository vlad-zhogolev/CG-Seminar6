using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roberts
{
    public class Face
    {
        public Face(IList<int> indices)
        {
            if (indices == null)
            {
                throw new ArgumentException("Can't create face, indices parameter is null");
            }
            if (indices.Count < 3)
            {
                throw new ArgumentException("Face can't contain less than 3 vertices");
            }
            foreach (var index in indices)
            {
                if (index < 0)
                {
                    throw new ArgumentException("Index of vertex can't be less than zero");
                }
            }
            m_indices = indices;
        }

        public IList<int> Indices { get { return m_indices; } }

        private IList<int> m_indices = null;
    }

    public class FaceBuilder
    {
        private IList<int> m_indices = new List<int>(0);
        
        public Face Build()
        {
            var result = new Roberts.Face(m_indices);
            m_indices = new List<int>(0); ;
            return result;
        }

        public void Add(int index)
        {
            m_indices.Add(index);
        }

        public void Add(int[] indices)
        {
            foreach (var index in indices)
            {
                m_indices.Add(index);
            }
        }
    }

    public class Mesh
    {
        private IList<Face> m_faces = new List<Face>(0);
        private MyMatrix<double> m_vertices = null;
        private MyMatrix<double> m_translation = MyMatrix<double>.Incident(4);
        private MyMatrix<double> m_rotation = MyMatrix<double>.Incident(4);
        private MyMatrix<double> m_scale = MyMatrix<double>.Incident(4);

        public IList<Face> Faces { get { return m_faces; } }

        public MyMatrix<double> Vertices { get { return m_vertices; } }

        public Mesh(IList<Face> faces, MyMatrix<double> vertices)
        {
            CheckNullFacesOrVertices(faces, vertices);
            CheckVerticesShape(vertices);
            m_faces = faces;
            m_vertices = vertices;
        }

        public Mesh(MyMatrix<int> faces, MyMatrix<double> vertices)
        {
            CheckNullFacesOrVertices(faces, vertices);
            if (faces.Height <= 0 || faces.Width < 3)
            {
                throw new ArgumentException(
                    "Wrong faces matrix size, height = " + faces.Height +
                    ", width = " + faces.Width
                );
            }
            CheckVerticesShape(vertices);
            for (int i = 0; i < faces.Height; ++i)
            {
                var faceBuilder = new FaceBuilder();
                for (var j = 0; j < faces.Width; ++j)
                {
                    faceBuilder.Add(faces[i, j]);
                }
                m_faces.Add(faceBuilder.Build());
            }
            m_vertices = vertices;
        }

        public void SetTranslation(MyMatrix<double> translation) { m_translation = translation; }

        public void AddTranslation(MyMatrix<double> translation) { m_translation = m_translation * translation; }
        
        public void ResetTranslation() { m_translation = MyMatrix<double>.Incident(4); }

        public void SetRotation(MyMatrix<double> rotation) { m_rotation = rotation; }

        public void AddRotation(MyMatrix<double> rotation) { m_rotation = m_rotation * rotation; }

        public void ResetRotation() { m_rotation = MyMatrix<double>.Incident(4); }

        public void SetScale(MyMatrix<double> scale) { m_scale = scale; }

        public void AddScale(MyMatrix<double> scale) { m_scale = m_scale * scale; }

        public void ResetScale() { m_scale = MyMatrix<double>.Incident(4); }

        public MyMatrix<double> GetWorldCoordinates()
        {
            return m_vertices * m_scale * m_translation * m_rotation;
        }

        public IList<Face> GetVisibleFaces(double x, double y, double z)
        {
            //var inversedMatrix = Utilities.Inverse(m_rotation);
            //var inversedMatrix = m_rotation;
            //var vertices = m_vertices;
            var vertices = GetWorldCoordinates();

            double barycenterX = 0;
            double barycenterY = 0;
            double barycenterZ = 0;
            for (var i = 0 ; i < vertices.Height ; ++i)
            {
                barycenterX += vertices[i, 0];
                barycenterY += vertices[i, 1];
                barycenterZ += vertices[i, 2];
            }
            barycenterX /= vertices.Height;
            barycenterY /= vertices.Height;
            barycenterZ /= vertices.Height;

            IList<Face> result = new List<Face>();
            var planes = new MyMatrix<double>(4, Faces.Count);
            for (var i = 0 ; i < Faces.Count ; ++i )
            {
                var x1 = vertices[Faces[i].Indices[1], 0] - vertices[Faces[i].Indices[0], 0];
                var y1 = vertices[Faces[i].Indices[1], 1] - vertices[Faces[i].Indices[0], 1];
                var z1 = vertices[Faces[i].Indices[1], 2] - vertices[Faces[i].Indices[0], 2];

                var x2 = vertices[Faces[i].Indices[2], 0] - vertices[Faces[i].Indices[1], 0];
                var y2 = vertices[Faces[i].Indices[2], 1] - vertices[Faces[i].Indices[1], 1];
                var z2 = vertices[Faces[i].Indices[2], 2] - vertices[Faces[i].Indices[1], 2];

                var a = y1 * z2 - y2 * z1;
                var b = z1 * x2 - z2 * x1;
                var c = x1 * y2 - x2 * y1;
                var d = -(a * vertices[Faces[i].Indices[0], 0] + b * vertices[Faces[i].Indices[0], 1] + c * vertices[Faces[i].Indices[0], 2]);

                planes[0, i] = a;
                planes[1, i] = b;
                planes[2, i] = c;
                planes[3, i] = d;

                var sign = -Math.Sign(a * barycenterX + b * barycenterY + c * barycenterZ + d);
                if ( sign == -1 )
                {
                    planes[0, i] *= -1;
                    planes[1, i] *= -1;
                    planes[2, i] *= -1;
                    planes[3, i] *= -1;
                }
            }

            //planes = inversedMatrix * planes;

            for (var i = 0 ; i < planes.Width ; ++i)
            {
                //if (planes[0, i] * (x - barycenterX) + planes[1, i] * (y - barycenterY) + planes[2, i] * (z - barycenterZ) + planes[3, i] > 0 )
                if(planes[0, i] * x + planes[1, i] * y + planes[2, i] * z + planes[3, i] > 0)
                {
                    result.Add(Faces[i]);
                }
            }

            return result;
        }

        public void SaveToFile(string path)
        {
            using ( System.IO.StreamWriter writer = new StreamWriter(path) )
            {
                for ( var i = 0 ; i < m_vertices.Height ; ++i )
                {
                    writer.WriteLine("v " + m_vertices[i, 0] + " " + m_vertices[i, 1] + " " + m_vertices[i, 2]);
                }

                foreach ( var face in Faces )
                {
                    string indices = "";
                    foreach ( var index in face.Indices )
                    {
                        indices += index + " ";
                    }
                    writer.WriteLine("f " + indices);
                }
            }
        }

        private void CheckNullFacesOrVertices(Object faces, Object vertices)
        {
            if (faces == null || vertices == null)
            {
                throw new ArgumentException(
                    "Faces are " + (faces == null ? "" : "not") + " null, " +
                    "vertices are: " + (faces == null ? "" : "not") + " null"
                );
            }
        }

        private void CheckVerticesShape(MyMatrix<double> vertices)
        {
            if (vertices.Height < 3 || vertices.Width != 4)
            {
                throw new ArgumentException(
                    "Wrong vertices matrix size, height = " + vertices.Height +
                    ", width = " + vertices.Width
                );
            }
        }
    }
}
