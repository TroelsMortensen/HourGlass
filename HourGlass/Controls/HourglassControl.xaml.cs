using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HourGlass.Controls
{
    public partial class HourglassControl : UserControl
    {
        private const double TopSandX = 55;
        private const double TopSandY = 30;
        private const double TopSandWidth = 90;
        private const double TopSandHeight = 100;

        private const double BottomSandX = 55;
        private const double BottomSandY = 190;
        private const double BottomSandWidth = 90;
        private const double BottomSandHeight = 80;

        private bool _isFlipping;

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
                nameof(Progress),
                typeof(double),
                typeof(HourglassControl),
                new PropertyMetadata(0.0, OnProgressChanged));

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(
                nameof(IsRunning),
                typeof(bool),
                typeof(HourglassControl),
                new PropertyMetadata(false, OnIsRunningChanged));

        public HourglassControl()
        {
            InitializeComponent();
            SizeChanged += (_, _) => UpdateSand();
            UpdateSand();
            UpdateStream();
        }

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public Task BeginFlipAsync()
        {
            if (_isFlipping)
            {
                return Task.CompletedTask;
            }

            _isFlipping = true;

            var tcs = new TaskCompletionSource<bool>();
            var targetAngle = GlassRotate.Angle + 180;

            var animation = new DoubleAnimation
            {
                From = GlassRotate.Angle,
                To = targetAngle,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (_, _) =>
            {
                GlassRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);
                GlassRotate.Angle = targetAngle % 360;
                _isFlipping = false;
                tcs.TrySetResult(true);
            };

            GlassRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animation);
            return tcs.Task;
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HourglassControl control)
            {
                control.UpdateSand();
                control.UpdateStream();
            }
        }

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HourglassControl control)
            {
                control.UpdateStream();
            }
        }

        private void UpdateSand()
        {
            var clamped = Math.Max(0, Math.Min(1, Progress));
            var topHeight = TopSandHeight * (1 - clamped);
            var bottomHeight = BottomSandHeight * clamped;

            TopSandClip.Rect = new Rect(TopSandX, TopSandY, TopSandWidth, topHeight);
            BottomSandClip.Rect = new Rect(
                BottomSandX,
                BottomSandY + (BottomSandHeight - bottomHeight),
                BottomSandWidth,
                bottomHeight);
        }

        private void UpdateStream()
        {
            var running = IsRunning && Progress > 0 && Progress < 1;
            SandStream.Opacity = running ? 1 : 0;
        }
    }
}
