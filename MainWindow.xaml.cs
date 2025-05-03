using SunRise_SunDown.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        private double m_SunPathStep;

        private bool m_StartClick;

        private string m_StartButtonTittle;

        private DoubleAnimationUsingPath m_animationX;

        private DoubleAnimationUsingPath m_animationY;

        private ColorAnimationUsingKeyFrames m_colorAnimationUsingKeyFrames;

        private bool m_AnimPlaying;

        private double m_AnimDuration;

        private double m_sunWidth;

        private double m_sunHeight;

        private bool m_CntrlPanelEnabled;

        private RadialGradientBrush m_sunBrush;

        private ColorAnimationUsingKeyFrames m_SunGradStepAnimation;

        private bool m_SunSetUp;

        private double m_groundHeight;

        private double m_skyHeight;

        private ColorAnimationUsingKeyFrames m_maskColor1Animation;

        private ColorAnimationUsingKeyFrames m_maskColor2Animation;

        private GradientBrush m_maskBrash;

        private Int32AnimationUsingKeyFrames m_ZIndexAnimation;

        #endregion

        #region Properties

        public Vector CurrentSunPosition
        {
            get => m_currentSunPosition;
            set => Set(ref m_currentSunPosition, value);
        }

        public Vector CentralLowPoint
        {
            get => m_CentralLowPoint;
            set => Set(ref m_CentralLowPoint, value);
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
            get => m_MaxZenitValue;
            set => Set(ref m_MaxZenitValue, value);
        }

        public double SunPathStep
        {
            get => m_SunPathStep;
            set
            {
                Set(ref m_SunPathStep, value);

                GenerateSunPath();
            }
        }

        public string StartButtonTitle
        {
            get => m_StartButtonTittle;
            set => Set(ref m_StartButtonTittle, value);
        }

        public double AnimDuration
        {
            get => m_AnimDuration;
            set => Set(ref m_AnimDuration, value);
        }


        public double SunWidth
        {
            get => m_sunWidth;
            set => Set(ref m_sunWidth, value);
        }

        public double SunHeight
        {
            get => m_sunHeight;
            set => Set(ref m_sunHeight, value);
        }

        public bool ControlPanelEnabled
        {
            get => m_CntrlPanelEnabled;
            set => Set(ref m_CntrlPanelEnabled, value);
        }

        public double GroundHeight
        {
            get => m_groundHeight;
            set => Set(ref m_groundHeight, value);
        }

        public double SkyHeigth
        {
            get => m_skyHeight;
            set => Set(ref m_skyHeight, value);
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

            m_SunPathStep = 0.01;

            m_StartClick = false;

            m_StartButtonTittle = "Start";

            m_AnimPlaying = false;

            m_sunHeight = 50;
            m_sunWidth = 50;

            m_CntrlPanelEnabled = true;

            m_AnimDuration = 1;

            m_groundHeight = 100;

            SetupSunBackColor();

            SetupMaskBackColor();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            var temp = Volatile.Read(ref this.PropertyChanged);
            temp?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string propName = "")
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

            MaxZenitValue = canvasActualHeight - m_sunHeight;
        }

        private IEnumerable<Point> CalculateEllipsePath(double start, double end)
        {
            double current = start;

            double a_coeficient = CentralLowPoint.X;

            double x = 0;

            while (current <= end)
            {
                x = current - a_coeficient;

                double y = Math.Sqrt(Math.Pow(BCoeficient, 2) - Math.Pow((BCoeficient / a_coeficient), 2) * Math.Pow(x, 2));

                yield return new Point(current, MaxZenitValue - y);

                current += SunPathStep;
            }
        }

        private void GenerateSunPath()
        {
            if (this.PolyLineMock.Points.Count > 0)
                this.PolyLineMock.Points.Clear();

            foreach (var p in CalculateEllipsePath(0, this.ActualWidth))
            {
                if (!double.IsNaN(p.Y))
                    this.PolyLineMock.Points.Add(p);
            }
        }

        private void CalculateSkyHeight(double canvasHeight)
        {
            SkyHeigth = canvasHeight - GroundHeight;
        }

        #endregion

        private void Canvas_Initialized(object sender, EventArgs e)
        {
            Canvas canvas = sender as Canvas;

            if (canvas == null)
                throw new Exception("Fail to get Canvas! <Canvas_Initialized>");

            CalculateMaxZenitValue(canvas.ActualHeight);

            CalculateSkyHeight(canvas.ActualHeight);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas canvas = sender as Canvas;

            if (canvas == null)
                throw new Exception("Fail to get Canvas! <Canvas_SizeChanged>");

            CalculateCentralPoint(canvas.ActualWidth);

            CalculateMaxZenitValue(canvas.ActualHeight);

            CalculateSkyHeight(canvas.ActualHeight);

            BCoeficient += 0.000000000000001;
        }

        private void StartSunPathAnimation()
        {
            if (!m_AnimPlaying)
            {
                ControlPanelEnabled = false;

                this.Sun.Visibility = Visibility.Visible;
                
                this.PolyLineMock.Visibility = Visibility.Hidden;
                this.SunMock.Visibility = Visibility.Hidden;

                this.Sun.BeginAnimation(Canvas.LeftProperty, m_animationX);
                this.Sun.BeginAnimation(Canvas.TopProperty, m_animationY);

                m_AnimPlaying = true;
            }
        }
        
        private void SetupSunBackColor()
        {
            if (!m_SunSetUp)
            {
                m_sunBrush = new RadialGradientBrush();

                m_sunBrush.GradientOrigin = new Point(0.5, 0.5);
                m_sunBrush.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 251, 99, 99), Offset = 0.337 });
                m_sunBrush.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0, 255, 255, 255), Offset = 1 });

                this.Sun.Fill = m_sunBrush;

                m_SunSetUp = true;
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

                ControlPanelEnabled = true;
            }
        }

        private void SetupSunColorAnimation()
        {
            m_SunGradStepAnimation = new ColorAnimationUsingKeyFrames();
            m_SunGradStepAnimation.Duration = TimeSpan.FromSeconds(AnimDuration);

            m_SunGradStepAnimation.AutoReverse = false;
            m_SunGradStepAnimation.RepeatBehavior = RepeatBehavior.Forever;
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0), Value = Color.FromArgb(255, 251, 99, 99) });
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.1), Value = Color.FromArgb(255, 251, 123, 99) });
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.15), Value = Color.FromArgb(255, 251, 160, 99) });
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.20), Value = Color.FromArgb(255, 251, 212, 99) });

            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.30), Value = Color.FromArgb(255, 251, 240, 99) });

            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.8), Value = Color.FromArgb(255, 251, 212, 99) });
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.85), Value = Color.FromArgb(255, 251, 160, 99) });
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.90), Value = Color.FromArgb(255, 251, 123, 99) });
            m_SunGradStepAnimation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(1), Value = Color.FromArgb(255, 251, 99, 99) });
        }

        private void StartSunColorAnimation()
        {
            m_sunBrush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, m_SunGradStepAnimation);
        }

        private void StopSunColorAnimation()
        {
            m_SunGradStepAnimation.BeginTime = null;
            m_sunBrush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, m_SunGradStepAnimation);
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
            m_animationX.Duration = TimeSpan.FromSeconds(AnimDuration);
            m_animationX.RepeatBehavior = RepeatBehavior.Forever;
            m_animationX.AutoReverse = false;

            m_animationY = new DoubleAnimationUsingPath();
            m_animationY.PathGeometry = animationPath;
            m_animationY.Source = PathAnimationSource.Y;
            m_animationY.Duration = TimeSpan.FromSeconds(AnimDuration);
            m_animationY.RepeatBehavior = RepeatBehavior.Forever;
            m_animationY.AutoReverse = false;
        }
        
        private void SetupMaskBackColor()
        {
            m_maskBrash = new LinearGradientBrush();
            m_maskBrash.SetValue(LinearGradientBrush.StartPointProperty, new Point(0.5, 0));
            m_maskBrash.SetValue(LinearGradientBrush.EndPointProperty, new Point(0.5, 1));
            m_maskBrash.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0,0,0,0), Offset=0 });
            m_maskBrash.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0, 0, 0, 0), Offset=1 });

            this.mask.Fill = m_maskBrash;
        }

        private void SetupZIndexAnimation()
        {
            m_ZIndexAnimation = new Int32AnimationUsingKeyFrames();

            m_ZIndexAnimation.RepeatBehavior = RepeatBehavior.Forever;
            m_ZIndexAnimation.Duration = TimeSpan.FromSeconds(AnimDuration);

            m_ZIndexAnimation.AutoReverse = false;
            m_ZIndexAnimation.KeyFrames.Add(new DiscreteInt32KeyFrame() 
            { KeyTime = KeyTime.FromPercent(0.09), Value = 1 });

        }

        private void StartZIndexAnimation()
        {
            this.Sun.BeginAnimation(Panel.ZIndexProperty, m_ZIndexAnimation);
        }

        private void StopZIndexAnimation()
        {
            this.Sun.BeginAnimation(Panel.ZIndexProperty, null);
            Panel.SetZIndex(this.Sun, 0);
        }

        private void SetupColorMaskAnimation()
        {
            m_maskColor1Animation = new ColorAnimationUsingKeyFrames();
            m_maskColor2Animation = new ColorAnimationUsingKeyFrames();

            m_maskColor1Animation.Duration = TimeSpan.FromSeconds(AnimDuration);
            m_maskColor2Animation.Duration = TimeSpan.FromSeconds(AnimDuration);

            m_maskColor1Animation.AutoReverse = false;
            m_maskColor2Animation.AutoReverse = false;

            m_maskColor1Animation.RepeatBehavior = RepeatBehavior.Forever;
            m_maskColor2Animation.RepeatBehavior = RepeatBehavior.Forever;

            
            //Start -> night
            m_maskColor1Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0), Value = Color.FromArgb(255, 5, 8, 51) });
            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0), Value = Color.FromArgb(228, 10, 14, 66) });
            //Dawn -> 
            m_maskColor1Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.1), Value = Color.FromArgb(255, 45, 49, 111) });
            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.1), Value = Color.FromArgb(217, 27, 0, 31) });

            m_maskColor1Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.15), Value = Color.FromArgb(255, 145, 151, 239) });
            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.15), Value = Color.FromArgb(217, 15, 0, 31) });
            
            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.20), Value = Color.FromArgb(153, 15, 0, 31) });

            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.30), Value = Color.FromArgb(0, 15, 0, 31) });

            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.8), Value = Color.FromArgb(153, 15, 0, 31) });

            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.85), Value = Color.FromArgb(217, 15, 0, 31) });
            m_maskColor1Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.85), Value = Color.FromArgb(255, 145, 151, 239) });

            m_maskColor1Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.9), Value = Color.FromArgb(255, 45, 49, 111) });
            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(0.9), Value = Color.FromArgb(217, 27, 0, 31) });

            m_maskColor1Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(1), Value = Color.FromArgb(255, 5, 8, 51) });
            m_maskColor2Animation.KeyFrames.Add(new LinearColorKeyFrame() { KeyTime = KeyTime.FromPercent(1), Value = Color.FromArgb(228, 10, 14, 66) });
        }


        private void StartMaskAnimation()
        {
            m_maskBrash.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, m_maskColor1Animation);
            m_maskBrash.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, m_maskColor2Animation);
        }

        private void StopMaskAnimation()
        {
            m_maskColor1Animation.BeginTime = null;
            m_maskColor2Animation.BeginTime = null;

            m_maskBrash.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, m_maskColor1Animation);
            m_maskBrash.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, m_maskColor2Animation);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_StartClick)
            {
                SetupSunPathAnimation();

                SetupSunColorAnimation();

                SetupColorMaskAnimation();

                SetupZIndexAnimation();

                StartSunPathAnimation();

                StartSunColorAnimation();

                StartMaskAnimation();

                StartZIndexAnimation();
            }
            else
            {
                StartButtonTitle = "Start";
                m_StartClick = false;

                StopSunPathAnimation();

                StopSunColorAnimation();

                StopMaskAnimation();

                StopZIndexAnimation();
            }
        }
    }
}
