using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HourGlass.Controls
{
    public partial class HourglassControl : UserControl
    {
        private const double TopSandX = 48;
        private const double TopSandY = 30;
        private const double TopSandWidth = 104;
        private const double TopSandHeight = 100;

        private bool _isFlipping;
        private Storyboard? _streamStoryboard;

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
                GlassRotate.Angle = 0;
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
            TopSandClip.Rect = new Rect(
                TopSandX,
                TopSandY + (TopSandHeight - topHeight),
                TopSandWidth,
                topHeight);
            BottomSandScale.ScaleY = clamped;
        }

        private void UpdateStream()
        {
            var running = IsRunning && Progress > 0 && Progress < 1;
            SandStream.Opacity = running ? 1 : 0;
            SandStreamGlow.Opacity = running ? 0.5 : 0;
            SandStreamParticle1.Opacity = running ? 1 : 0;
            SandStreamParticle2.Opacity = running ? 0.9 : 0;
            SandStreamParticle3.Opacity = running ? 0.8 : 0;

            if (running)
            {
                EnsureStreamAnimation();
                _streamStoryboard?.Begin(this, true);
            }
            else
            {
                _streamStoryboard?.Stop(this);
            }
        }

        private void EnsureStreamAnimation()
        {
            if (_streamStoryboard != null)
            {
                return;
            }

            _streamStoryboard = new Storyboard();

            var glowAnimation = new DoubleAnimation
            {
                From = 0.2,
                To = 0.7,
                Duration = TimeSpan.FromSeconds(0.6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(glowAnimation, SandStreamGlow);
            Storyboard.SetTargetProperty(glowAnimation, new PropertyPath(OpacityProperty));
            _streamStoryboard.Children.Add(glowAnimation);

            var particleAnimation1 = new DoubleAnimation
            {
                From = 140,
                To = 258,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(particleAnimation1, SandStreamParticle1);
            Storyboard.SetTargetProperty(particleAnimation1, new PropertyPath("(Canvas.Top)"));
            _streamStoryboard.Children.Add(particleAnimation1);

            var particleAnimation2 = new DoubleAnimation
            {
                From = 150,
                To = 258,
                Duration = TimeSpan.FromSeconds(0.6),
                BeginTime = TimeSpan.FromSeconds(0.2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(particleAnimation2, SandStreamParticle2);
            Storyboard.SetTargetProperty(particleAnimation2, new PropertyPath("(Canvas.Top)"));
            _streamStoryboard.Children.Add(particleAnimation2);

            var particleAnimation3 = new DoubleAnimation
            {
                From = 160,
                To = 258,
                Duration = TimeSpan.FromSeconds(0.7),
                BeginTime = TimeSpan.FromSeconds(0.35),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(particleAnimation3, SandStreamParticle3);
            Storyboard.SetTargetProperty(particleAnimation3, new PropertyPath("(Canvas.Top)"));
            _streamStoryboard.Children.Add(particleAnimation3);
        }
    }
}
