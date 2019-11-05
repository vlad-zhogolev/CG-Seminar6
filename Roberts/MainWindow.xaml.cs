using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Microsoft.Win32;

namespace Roberts
{

    public enum Projection
    {
        Perspective,
        Orthogonal
    }

    class Default
    {
        public static readonly Vector3D SCALE = new Vector3D(0.25, 0.25, 0.25);
        public static readonly double CAMERA_Z_VALUE = 15.0;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap m_bitmap;
        private MyObject m_currentObject;
        private Drawer m_drawer;
        private bool m_cutFaces = false;
        private IDictionary<string, MyObject> m_objectsMap = new Dictionary<string, MyObject>();

        public MainWindow()
        {
            InitializeComponent();
            m_bitmap = new WriteableBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Bgr32, null);
            image.Source = m_bitmap;
            image.Stretch = Stretch.None;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.VerticalAlignment = VerticalAlignment.Top;

            var vertices = new double[,]
            {
                {0, 0, 0, 1},
                {1, 0, 0, 1},
                {0.5, -0.2, 1, 1},
                {0.5, 1, 0.5, 1}
            };

            var faces = new int[,]
            {
                {1, 0, 3},
                {2, 1, 3},
                {0, 2, 3},
                {2, 0, 1}
            };

            var defaultObject = new MyObject("default", new Vector3D(0, 0, 0), new Vector3D(0, 0, 0), Default.SCALE, Shape.Tetrahedron);
            AddObject(defaultObject);
            objectsListBox.SelectedIndex = 0;
            var r = -1.0 / 15.0;
            var perspective = new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, r },
                { 0, 0, 0, 1 }
            };
            var projection = new MyMatrix<double>(perspective);

            m_currentObject = GetCurrentObject();
            m_drawer = new Drawer(projection, (int)m_bitmap.Width, (int)m_bitmap.Height);
            projectionComboBox.SelectedIndex = 0;
            Redraw();
        }

        private void ClearImage()
        {
            DrawAlgorithm.ResetColor(Colors.Black, m_bitmap);
        }

        private void Redraw()
        {
            ClearImage();
            foreach (var pair in m_objectsMap)
            {
                m_drawer.Draw(m_bitmap, pair.Value, m_cutFaces);
            }
        }

        private void AddObject()
        {
            var name = objectNameTextBox.Text;
            AddObject(new MyObject(name, new Vector3D(0, 0, 0), new Vector3D(0, 0, 0), Default.SCALE, GetSelectedShape()));
        }

        private void AddObject(MyObject obj)
        {
            m_objectsMap.Add(obj.Name, obj);
            var item = new ListBoxItem();
            item.Content = obj.Name;
            objectsListBox.Items.Add(item);
        }

        public MyObject GetCurrentObject()
        {
            var lbi = objectsListBox.SelectedItem as ListBoxItem;
            var name = lbi.Content.ToString();
            MyObject result;
            m_objectsMap.TryGetValue(name, out result);
            return result;
        }

        private void xTranslationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TranslationSliderValueChanged();
        }

        private void yTranslationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TranslationSliderValueChanged();
        }

        private void zTranslationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TranslationSliderValueChanged();
        }

        private void TranslationSliderValueChanged()
        {
            var xyz = GetTranslationSliderValues();
            m_currentObject.Position = new Vector3D(xyz[0], xyz[1], xyz[2]);
            Redraw();
        }

        private double[] GetTranslationSliderValues()
        {
            return new double[] { xTranslationSlider.Value, yTranslationSlider.Value, zTranslationSlider.Value };
        }

        private void xRotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotationSliderValueChanged();
        }

        private void yRotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotationSliderValueChanged();
        }

        private void zRotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotationSliderValueChanged();
        }

        private void RotationSliderValueChanged()
        {
            var xyz = GetRotationSliderValues();
            m_currentObject.Rotation = new Vector3D(xyz[0], xyz[1], xyz[2]);
            //m_currentObject.SetRotation(TransformFactory.CreateOxRotation(xyz[0]));
            //m_currentObject.AddRotation(TransformFactory.CreateOyRotation(xyz[1]));
            //m_currentObject.AddRotation(TransformFactory.CreateOzRotation(xyz[2]));
            Redraw();
        }

        private double[] GetRotationSliderValues()
        {
            return new double[] { xRotationSlider.Value, yRotationSlider.Value, zRotationSlider.Value };
        }

        private Shape GetSelectedShape()
        {
            var shape = (shapeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            switch(shape)
            {
                case "Tetrahedron":
                return Shape.Tetrahedron;
                case "Hexahedron":
                return Shape.Hexahedron;
                case "Octahedron":
                return Shape.Octahedron;
                case "Dodecahedron":
                return Shape.Dodecahedron;
                case "Icosahedron":
                return Shape.Icosahedron;
                case "Sphere":
                return Shape.Sphere;
                case "Sphere without poles":
                return Shape.SphereWithoutPole;

                default:
                throw new ArgumentException("No such shape type: " + shape);
            }
        }

        private Projection GetCurrentProjection()
        {
            var type = (projectionComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            switch ( type )
            {
                case "Perspective":
                return Projection.Perspective;
                case "Orthogonal":
                return Projection.Orthogonal;
                default:
                throw new ArgumentException("No such projection type: " + type);
            }
        }

        private void projectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = GetCurrentProjection();
            switch ( type )
            {
                case Projection.Perspective:
                {
                    m_drawer.Projection = GetPerspectiveProjection(Default.CAMERA_Z_VALUE);
                }
                break;
                case Projection.Orthogonal:
                {
                    m_drawer.Projection = GetOrthogonalProjection();
                }
                break;
                default:
                throw new ArgumentException("Can't handle that projection type");
            }
            Redraw();
        }

        private MyMatrix<double> GetPerspectiveProjection(double z)
        {
            var r = -1.0 / z;
            return new MyMatrix<double>( new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, r },
                { 0, 0, 0, 1 }
            });
        }

        private MyMatrix<double> GetOrthogonalProjection()
        {
            return new MyMatrix<double>(new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 1 }
            });
        }

        private void addObjectButton_Click(object sender, RoutedEventArgs e)
        {
            var name = objectNameTextBox.Text;
            if ( !m_objectsMap.ContainsKey(name) )
            {
                AddObject();
            }
            Redraw();
        }

        private void objectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_currentObject = GetCurrentObject();
            RestoreControlsForCurrentObject();
        }

        private void RestoreControlsForCurrentObject()
        {
            xTranslationSlider.ValueChanged -= xTranslationSlider_ValueChanged;
            yTranslationSlider.ValueChanged -= yTranslationSlider_ValueChanged;
            zTranslationSlider.ValueChanged -= zTranslationSlider_ValueChanged;
            xTranslationSlider.Value = m_currentObject.Position.X;
            yTranslationSlider.Value = m_currentObject.Position.Y;
            zTranslationSlider.Value = m_currentObject.Position.Z;
            xTranslationSlider.ValueChanged += xTranslationSlider_ValueChanged;
            yTranslationSlider.ValueChanged += yTranslationSlider_ValueChanged;
            zTranslationSlider.ValueChanged += zTranslationSlider_ValueChanged;

            xRotationSlider.ValueChanged -= xRotationSlider_ValueChanged;
            yRotationSlider.ValueChanged -= yRotationSlider_ValueChanged;
            zRotationSlider.ValueChanged -= zRotationSlider_ValueChanged;
            xRotationSlider.Value = m_currentObject.Rotation.X;
            yRotationSlider.Value = m_currentObject.Rotation.Y;
            zRotationSlider.Value = m_currentObject.Rotation.Z;
            xRotationSlider.ValueChanged += xRotationSlider_ValueChanged;
            yRotationSlider.ValueChanged += yRotationSlider_ValueChanged;
            zRotationSlider.ValueChanged += zRotationSlider_ValueChanged;

            xScaleSlider.ValueChanged -= xScaleSlider_ValueChanged;
            yScaleSlider.ValueChanged -= yScaleSlider_ValueChanged;
            zScaleSlider.ValueChanged -= zScaleSlider_ValueChanged;
            xScaleSlider.Value = m_currentObject.Scale.X;
            yScaleSlider.Value = m_currentObject.Scale.Y;
            zScaleSlider.Value = m_currentObject.Scale.Z;
            xScaleSlider.ValueChanged += xScaleSlider_ValueChanged;
            yScaleSlider.ValueChanged += yScaleSlider_ValueChanged;
            zScaleSlider.ValueChanged += zScaleSlider_ValueChanged;

        }

        private void cutFacesButton_Click(object sender, RoutedEventArgs e)
        {
            cutFacesButton.Content = m_cutFaces ? "Hide invisible faces" : "Show invisible faces";
            m_cutFaces = !m_cutFaces;
            Redraw();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            m_currentObject.Position = new Vector3D(0, 0, 0);
            m_currentObject.Rotation = new Vector3D(0, 0, 0);
            m_currentObject.Scale = Default.SCALE;
            RestoreControlsForCurrentObject();
            Redraw();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;

                if ( saveFileDialog.ShowDialog() == true )
                {
                    m_currentObject.Mesh.SaveToFile(saveFileDialog.FileName);
                }
            }
            catch
            {
                MessageBox.Show("Failed to save file.");
            }
        }

        private void aboutProgramButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Семинар 5: Алгоритм Робертса\n Автор: Жоголев Владислав\n Группа: БПИ 164\n Среда разработки: Visual Studio 2015\n Язык разработки:C#\n Дата выполнения:30.10.2019");
        }

        private double[] GetScaleSliderValues()
        {
            return new double[] { xScaleSlider.Value, yScaleSlider.Value, zScaleSlider.Value };
        }

        private void ScaleSliderValueChanged()
        {
            var xyz = GetScaleSliderValues();
            if (m_currentObject == null)
            {
                return;
            }
            m_currentObject.Scale = new Vector3D(xyz[0], xyz[1], xyz[2]);
            Redraw();
        }

        private void xScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (xScaleSlider == null || yScaleSlider == null || zScaleSlider == null)
            {
                return;
            }
            ScaleSliderValueChanged();
        }

        private void yScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ( xScaleSlider == null || yScaleSlider == null || zScaleSlider == null )
            {
                return;
            }
            ScaleSliderValueChanged();
        }

        private void zScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ( xScaleSlider == null || yScaleSlider == null || zScaleSlider == null )
            {
                return;
            }
            ScaleSliderValueChanged();
        }

        private void fileStructureButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Структура файла\n" +
                "Файл модели содержит определение вершин в виде: v x y z. И граней в виде: f a b c\n" +
                "где x, y, z - соответствующие координаты вершин по осям Ox, Oy, Oz\n" +
                "и a, b, c - номера вершин, составляющих грань\n" +
                "Сперва файл содержит список вершин, затем список граней");
        }
    }
};
