using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Roberts
{
    public class MyObject
    {
        private Mesh m_mesh;
        private Vector3D m_position;
        private Vector3D m_rotation;
        private Vector3D m_scale;
        private string m_name;

        public Vector3D Position {
            get { return m_position; }

            set
            {
                m_position = value;
                m_mesh.SetTranslation(TransformFactory.CreateTranslation(Position.X, Position.Y, Position.Z));
            }
        }

        public Vector3D Rotation
        {
            get { return m_rotation; }
            set
            {
                m_rotation = value;
                m_mesh.SetRotation(TransformFactory.CreateOxRotation(Rotation.X));
                m_mesh.AddRotation(TransformFactory.CreateOyRotation(Rotation.Y));
                m_mesh.AddRotation(TransformFactory.CreateOzRotation(Rotation.Z));
            }
        }

        public Vector3D Scale
        {
            get { return m_scale; }
            set
            {
                m_scale = value;
                m_mesh.SetScale(TransformFactory.CreateScale(Scale.X, Scale.Y, Scale.Z));
            }
        }

        public string Name { get { return m_name; } }

        public Mesh Mesh
        {
            get { return m_mesh; }
        }

        public MyObject(string name, Vector3D position, Vector3D rotation, Vector3D scale, Shape shape, double radius = 1.0, int subdivisions = 3)
        {
            m_mesh = ShapeFactory.CreateShape(shape, radius, subdivisions);
            m_name = name;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
