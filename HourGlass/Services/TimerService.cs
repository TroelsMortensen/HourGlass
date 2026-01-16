using System;
using System.Windows.Threading;

namespace HourGlass.Services
{
    public class TimerService
    {
        private readonly DispatcherTimer _timer;
        private DateTime _lastTickUtc;
        private TimeSpan _duration = TimeSpan.FromMinutes(25);
        private TimeSpan _remaining = TimeSpan.FromMinutes(25);

        public TimerService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += OnTick;
        }

        public event EventHandler? Tick;
        public event EventHandler? Completed;

        public TimeSpan Duration => _duration;
        public TimeSpan Remaining => _remaining;
        public bool IsRunning => _timer.IsEnabled;

        public void SetDuration(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                duration = TimeSpan.FromSeconds(1);
            }

            _duration = duration;
            if (!IsRunning)
            {
                _remaining = duration;
                Tick?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            if (_remaining <= TimeSpan.Zero)
            {
                _remaining = _duration;
            }

            _lastTickUtc = DateTime.UtcNow;
            _timer.Start();
            Tick?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            if (!IsRunning)
            {
                return;
            }

            _timer.Stop();
        }

        public void Reset()
        {
            _timer.Stop();
            _remaining = _duration;
            Tick?.Invoke(this, EventArgs.Empty);
        }

        public double Progress
        {
            get
            {
                if (_duration.TotalMilliseconds <= 0)
                {
                    return 0;
                }

                var remaining = Math.Max(0, _remaining.TotalMilliseconds);
                return 1.0 - (remaining / _duration.TotalMilliseconds);
            }
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastTickUtc;
            _lastTickUtc = now;

            _remaining -= elapsed;
            if (_remaining <= TimeSpan.Zero)
            {
                _remaining = TimeSpan.Zero;
                _timer.Stop();
                Tick?.Invoke(this, EventArgs.Empty);
                Completed?.Invoke(this, EventArgs.Empty);
                return;
            }

            Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}
