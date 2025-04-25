using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SunRise_SunDown
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private Vector m_currentSunPosition;

        private Vector m_CentralLowPoint;
        
        private double m_BCoeficient;

        private double m_MaxZenitValue;

        private double m_SunMockUpHeight;

        private double m_SunPathStep;
        
        private bool m_StartClick;

        private string m_StartButtonTittle;

        private DoubleAnimationUsingPath m_animationX;

        private DoubleAnimationUsingPath m_animationY;

        private bool m_AnimPlaying;
        
        #endregion

        #region Properties

        public Vector CurrentSunPosition 
        {
            get => m_currentSunPosition; 
            set => Set(ref m_currentSunPosition, value); 
        }

        public Vector CentralLowPoint 
        {
            get=> m_CentralLowPoint; 
            set=> Set(ref m_CentralLowPoint, value); 
        }

        public double BCoeficient
        {
            get => m_BCoeficient;
            set 
            { 
                Set(ref m_BCoeficient, value);

                GenerateSunPath();
            }
        }

        public double MaxZenitValue 
        {
            get=>m_MaxZenitValue; 
            set=>Set(ref m_MaxZenitValue, value);
        }

        public double SunPathStep 
        {
            get=> m_SunPathStep;
            set 
            {
                Set(ref m_SunPathStep, value); 

                GenerateSunPath();
            }
        }

        public string StartButtonTitle 
        {
            get=> m_StartButtonTittle; 
            set=> Set(ref m_StartButtonTittle, value);
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            
            this.DataContext = this;

            this.Task_Desc.Text = "Варіант номер 9, Литвинов Б Ю 125 гр\n" + 
                "Схід сонця і захід сонця. Сонце повинне з'являтися в лівій нижній частині вікна, \n" +
                "рухатися у вікні деяким радіусом і сідати в нижній правій частині вікна. \n" +
                "У процесі руху воно має максимально правдоподібно змінювати колір та розмір\n";

            m_SunMockUpHeight = (double)this.Resources["SunMockupHeight"];

            m_SunPathStep = 0.01;

            m_StartClick = false;

            m_StartButtonTittle = "Start";

            m_AnimPlaying = false;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            var temp = Volatile.Read(ref this.PropertyChanged);
            temp?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string propName= "")
        {
            if (field == null)
            {
                throw new ArgumentNullException(string.Format("Field was null! Property: {0}", propName));
            }

            if (field.Equals(value))
            {
                return false;
            }
            else
            {
                field = value;

                OnPropertyChanged(propName);

                return true;
            }
        }

        #endregion
 
        #region Private Helper Methods
        
        private void CalculateCentralPoint(double actualWidth)
        {
            CentralLowPoint = new Vector(actualWidth / 2, 0);
        }

        private void CalculateMaxZenitValue(double canvasActualHeight)
        {
            if (canvasActualHeight == 0)
                return;

            MaxZenitValue = canvasActualHeight - m_SunMockUpHeight;
        }

        private IEnumerable<Point> CalculateEllipsePath(double start, double end)
        {
            double current = start;

            while (current < end)
            {
                double x = 0;

                x = current - CentralLowPoint.X;

                double y = Math.Sqrt(Math.Pow(BCoeficient, 2) - Math.Pow((BCoeficient/CentralLowPoint.X), 2) * Math.Pow(x, 2));
                
                yield return new Point(current, MaxZenitValue - y);

                current+= SunPathStep;
            }
        }

        private void GenerateSunPath()
        {
            if (this.PolyLineMock.Points.Count > 0)
                this.PolyLineMock.Points.Clear();

            foreach (var p in CalculateEllipsePath(0, this.ActualWidth))
            {
                this.PolyLineMock.Points.Add(p);
            }
        }

        #endregion

        private void Canvas_Initialized(object sender, EventArgs e)
        {
            Canvas canvas = sender as Canvas;

            if (canvas == null)
                throw new Exception("Fail to get Canvas! <Canvas_Initialized>");

            CalculateMaxZenitValue(canvas.ActualHeight);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateCentralPoint(this.ActualWidth);

            Canvas canvas = sender as Canvas;

            if (canvas == null)
                throw new Exception("Fail to get Canvas! <Canvas_SizeChanged>");

            CalculateMaxZenitValue(canvas.ActualHeight);
            
            BCoeficient += 0.000000000000001;
        }

        private void StartSunPathAnimation()
        {
            if (!m_AnimPlaying)
            {
                this.Sun.BeginAnimation(Canvas.LeftProperty, m_animationX);
                this.Sun.BeginAnimation(Canvas.TopProperty, m_animationY);

                this.PolyLineMock.Visibility = Visibility.Hidden;
                this.SunMock.Visibility = Visibility.Hidden;
                m_AnimPlaying = true;
            }
        }

        private void StopSunPathAnimation()
        {
            if (m_AnimPlaying)
            {
                m_animationX.BeginTime = null;
                m_animationY.BeginTime = null;

                this.Sun.BeginAnimation(Canvas.LeftProperty, m_animationX);
                this.Sun.BeginAnimation(Canvas.TopProperty, m_animationY);

                this.PolyLineMock.Visibility = Visibility.Visible;
                this.SunMock.Visibility = Visibility.Visible;
                m_AnimPlaying = false;
            }
        }

        private void SetupSunPathAnimation()
        {
            var points = this.PolyLineMock.Points;

            StartButtonTitle = "Stop";
            m_StartClick = true;

            PathGeometry animationPath = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = points[0];

            PolyLineSegment polyLineSegment = new PolyLineSegment(points, false);
            
            pathFigure.Segments.Add(polyLineSegment);
            animationPath.Figures.Add(pathFigure);

            m_animationX = new DoubleAnimationUsingPath();
            m_animationX.PathGeometry = animationPath;
            m_animationX.Source = PathAnimationSource.X;
            m_animationX.Duration = TimeSpan.FromSeconds(5);
            m_animationX.RepeatBehavior = RepeatBehavior.Forever;
            m_animationX.AutoReverse = false;

            m_animationY = new DoubleAnimationUsingPath();
            m_animationY.PathGeometry = animationPath;
            m_animationY.Source = PathAnimationSource.Y;
            m_animationY.Duration = TimeSpan.FromSeconds(5);
            m_animationY.RepeatBehavior = RepeatBehavior.Forever;
            m_animationY.AutoReverse = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_StartClick)
            {
                SetupSunPathAnimation();

                StartSunPathAnimation();
            }
            else
            {
                StartButtonTitle = "Start";
                m_StartClick = false;

                StopSunPathAnimation();
            }
        }
    }
}
