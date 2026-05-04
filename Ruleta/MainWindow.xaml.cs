using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ruleta.Models;
using Ruleta.Services;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PrizeRoulette
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Handles both Touch API and legacy Mouse inputs for the spin animation.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Animation state variables
        private double _currentAngle = 0;
        private bool _isSpinning = false;

        // Mouse tracking state variables
        private bool _isMouseSwiping = false;
        private double _mouseStartY = 0;
        private DateTime _mouseStartTime;

        //Prize management
        private PrizeCalculator _prizeCalculator;
        private List<PrizeConfig> _currentPrizes;

        public MainWindow()
        {
            InitializeComponent();
            _prizeCalculator = new PrizeCalculator();
            LoadDynamicRouletteData();
        }

        private void LoadDynamicRouletteData()
        {
            // Simulate extracting inventory based on a ticket tier
            _currentPrizes = MockDatabaseDao.GetPrizesForCategory("SMALL"); //can changed by LARGE or SALL, its for testing 

            // Assign mathematical geometry ranges to each prize
            _currentPrizes = _prizeCalculator.CalculateSliceAngles(_currentPrizes);

            // Render the UI
            RenderDynamicRoulette(_currentPrizes);
        }
        // --------------------------------------------------------
        // DYNAMIC RENDERING ENGINE
        // --------------------------------------------------------

        private void RenderDynamicRoulette(List<PrizeConfig> prizes)
        {
            RouletteCanvas.Children.Clear();
            double radius = 300;
            Point center = new Point(radius, radius);

            // High contrast color palette for visibility
            string[] hexColors = { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899", "#14b8a6", "#f97316" };

            for (int i = 0; i < prizes.Count; i++)
            {
                var prize = prizes[i];
                string sliceColor = hexColors[i % hexColors.Length];

                Path slicePath = CreateSlice(prize.StartAngle, prize.EndAngle, center, radius, sliceColor);
                RouletteCanvas.Children.Add(slicePath);

                UIElement textBlock = CreateSliceText(prize.ProductName, prize.StartAngle, prize.EndAngle, center, radius);
                RouletteCanvas.Children.Add(textBlock);
            }
        }

        private Point GetPointFromAngle(Point center, double radius, double angleInDegrees)
        {
            // Subtract 90 to align 0 degrees strictly to the vertical top pointer
            double angleInRadians = (angleInDegrees - 90) * Math.PI / 180.0;
            double x = center.X + radius * Math.Cos(angleInRadians);
            double y = center.Y + radius * Math.Sin(angleInRadians);
            return new Point(x, y);
        }

        private Path CreateSlice(double startAngle, double endAngle, Point center, double radius, string hexColor)
        {
            Point startPoint = GetPointFromAngle(center, radius, startAngle);
            Point endPoint = GetPointFromAngle(center, radius, endAngle);
            bool isLargeArc = (endAngle - startAngle) > 180.0;

            PathFigure pathFigure = new PathFigure
            {
                StartPoint = center,
                IsClosed = true
            };

            pathFigure.Segments.Add(new LineSegment(startPoint, false));
            pathFigure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = isLargeArc,
                SweepDirection = SweepDirection.Clockwise
            });

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return new Path
            {
                Data = pathGeometry,
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor),
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
        }

        private UIElement CreateSliceText(string text, double startAngle, double endAngle, Point center, double radius)
        {
            double middleAngle = startAngle + ((endAngle - startAngle) / 2);

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = 180 // Constrain width for centering
            };

            TransformGroup transformGroup = new TransformGroup();
            // Center the text block horizontally and push it radially outward by 75%
            transformGroup.Children.Add(new TranslateTransform(-90, -(radius * 0.75)));
            transformGroup.Children.Add(new RotateTransform(middleAngle));

            Canvas.SetLeft(textBlock, center.X);
            Canvas.SetTop(textBlock, center.Y);

            textBlock.RenderTransform = transformGroup;

            return textBlock;
        }



        // --------------------------------------------------------
        // TOUCH API IMPLEMENTATION
        // --------------------------------------------------------

        /// <summary>
        /// Initializes the manipulation context for touch events.
        /// </summary>
        private void OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Mode = ManipulationModes.Translate | ManipulationModes.Rotate;
        }

        /// <summary>
        /// Captures the raw linear velocity from the user's swipe (Touch).
        /// </summary>
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_isSpinning) return;

            double swipeVelocity = e.FinalVelocities.LinearVelocity.Y;

            if (Math.Abs(swipeVelocity) < 0.5) return;

            ExecuteSpinAnimation(swipeVelocity);
        }

        // --------------------------------------------------------
        // MOUSE FALLBACK IMPLEMENTATION
        // --------------------------------------------------------

        /// <summary>
        /// Captures the initial position and time when the mouse button is pressed.
        /// </summary>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSpinning) return;

            _isMouseSwiping = true;
            _mouseStartY = e.GetPosition(this).Y;
            _mouseStartTime = DateTime.Now;

            RouletteContainer.CaptureMouse();
        }

        /// <summary>
        /// Calculates the velocity of the mouse drag and triggers the spin animation.
        /// </summary>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isMouseSwiping) return;

            _isMouseSwiping = false;
            RouletteContainer.ReleaseMouseCapture();

            double currentY = e.GetPosition(this).Y;
            double distanceY = currentY - _mouseStartY;
            double elapsedSeconds = (DateTime.Now - _mouseStartTime).TotalSeconds;

            if (elapsedSeconds <= 0) return;

            // Normalize mouse velocity to match Touch API scale
            double simulatedVelocity = (distanceY / elapsedSeconds) / 50;

            if (Math.Abs(simulatedVelocity) < 0.5) return;

            ExecuteSpinAnimation(simulatedVelocity);
        }

        // --------------------------------------------------------
        // HARDWARE ACCELERATED ANIMATION
        // --------------------------------------------------------

        /// <summary>
        /// Calculates the final angle based on inertia and executes the DoubleAnimation.
        /// </summary>
        private void ExecuteSpinAnimation(double velocity)
        {
            _isSpinning = true;

            // 1. Business Logic resolves the winner before the UI spins
            PrizeResult result = _prizeCalculator.DetermineWinningPrize(_currentPrizes);

            // 2. Calculate the UI offset needed to align the winning slice to the top pointer
            // The top pointer is at 0 degrees. If the target angle is 90, the wheel must rotate backward by 90 (or forward to 360-90)
            double requiredRotation = 360 - result.TargetAngle;

            // 3. Add multiple full baseline rotations (5) based on user's input inertia for a realistic feel
            double baseSpins = 360 * 5;
            double targetVisualAngle = _currentAngle + baseSpins + requiredRotation - (_currentAngle % 360);

            var spinAnimation = new DoubleAnimation
            {
                From = _currentAngle,
                To = targetVisualAngle,
                Duration = TimeSpan.FromSeconds(4.5),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            spinAnimation.Completed += (s, e) =>
            {
                _currentAngle = targetVisualAngle % 360;
                RouletteTransform.BeginAnimation(RotateTransform.AngleProperty, null);
                RouletteTransform.Angle = _currentAngle;

                _isSpinning = false;

                // Execute strictly after animation finishes
                MessageBox.Show($"Tu premio es: {result.WinningPrize.ProductName}", "Premio");
            };

            RouletteTransform.BeginAnimation(RotateTransform.AngleProperty, spinAnimation);
        }
    }
}