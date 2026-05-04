using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

        public MainWindow()
        {
            InitializeComponent();
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

            double velocityMultiplier = Math.Abs(velocity * 15);
            double targetAngle = _currentAngle + (360 * 5) + velocityMultiplier;

            var spinAnimation = new DoubleAnimation
            {
                From = _currentAngle,
                To = targetAngle,
                Duration = TimeSpan.FromSeconds(4.5),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            spinAnimation.Completed += (s, e) =>
            {
                // Normalize angle
                _currentAngle = targetAngle % 360;

                // Release the animation lock so properties can be modified again
                RouletteTransform.BeginAnimation(RotateTransform.AngleProperty, null);
                RouletteTransform.Angle = _currentAngle;

                _isSpinning = false;
            };

            RouletteTransform.BeginAnimation(RotateTransform.AngleProperty, spinAnimation);
        }
    }
}