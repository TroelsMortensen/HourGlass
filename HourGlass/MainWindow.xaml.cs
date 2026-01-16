using System;
using System.Globalization;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using HourGlass.Services;

namespace HourGlass;

public partial class MainWindow : Window
{
    private readonly TimerService _timer = new();
    private readonly SoundPlayer _dingPlayer;
    private bool _isResetting;

    public MainWindow()
    {
        InitializeComponent();

        Topmost = true;
        TopmostCheckBox.IsChecked = true;

        var dingPath = Path.Combine(AppContext.BaseDirectory, "Assets", "ding.wav");
        _dingPlayer = new SoundPlayer(dingPath);

        DurationTextBox.Text = FormatTime(_timer.Duration);
        RemainingText.Text = FormatTime(_timer.Remaining);
        Hourglass.Progress = _timer.Progress;

        _timer.Tick += (_, _) => UpdateDisplay();
        _timer.Completed += (_, _) => OnTimerCompleted();

        StartPauseButton.Click += OnStartPauseClicked;
        ResetButton.Click += OnResetClicked;
        TopmostCheckBox.Checked += (_, _) => Topmost = true;
        TopmostCheckBox.Unchecked += (_, _) => Topmost = false;
        DurationTextBox.LostFocus += (_, _) => TryApplyDuration();
        DurationTextBox.KeyDown += OnDurationKeyDown;
    }

    private void OnStartPauseClicked(object sender, RoutedEventArgs e)
    {
        if (_timer.IsRunning)
        {
            _timer.Pause();
        }
        else
        {
            TryApplyDuration();
            _timer.Start();
        }

        UpdateDisplay();
    }

    private async void OnResetClicked(object sender, RoutedEventArgs e)
    {
        if (_isResetting)
        {
            return;
        }

        _isResetting = true;
        StartPauseButton.IsEnabled = false;
        ResetButton.IsEnabled = false;

        await Hourglass.BeginFlipAsync();
        _timer.Reset();
        _timer.Start();

        StartPauseButton.IsEnabled = true;
        ResetButton.IsEnabled = true;
        _isResetting = false;
        UpdateDisplay();
    }

    private void OnDurationKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TryApplyDuration();
            e.Handled = true;
        }
    }

    private void TryApplyDuration()
    {
        if (_timer.IsRunning)
        {
            DurationTextBox.Text = FormatTime(_timer.Duration);
            return;
        }

        if (TryParseDuration(DurationTextBox.Text, out var duration))
        {
            _timer.SetDuration(duration);
            DurationTextBox.Text = FormatTime(_timer.Duration);
            UpdateDisplay();
            return;
        }

        DurationTextBox.Text = FormatTime(_timer.Duration);
    }

    private bool TryParseDuration(string? text, out TimeSpan duration)
    {
        duration = TimeSpan.FromMinutes(25);
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        text = text.Trim();
        if (text.Contains(":"))
        {
            var parts = text.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
            {
                return false;
            }

            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
            {
                return false;
            }

            minutes = Math.Max(0, minutes);
            seconds = Math.Max(0, Math.Min(59, seconds));
            duration = new TimeSpan(0, minutes, seconds);
            return duration > TimeSpan.Zero;
        }

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mins))
        {
            return false;
        }

        mins = Math.Max(0, mins);
        duration = TimeSpan.FromMinutes(mins);
        return duration > TimeSpan.Zero;
    }

    private string FormatTime(TimeSpan value)
    {
        return $"{(int)value.TotalMinutes:00}:{value.Seconds:00}";
    }

    private void UpdateDisplay()
    {
        RemainingText.Text = FormatTime(_timer.Remaining);
        Hourglass.Progress = _timer.Progress;
        Hourglass.IsRunning = _timer.IsRunning;
        StartPauseButton.Content = _timer.IsRunning ? "Pause" : "Start";
    }

    private void OnTimerCompleted()
    {
        UpdateDisplay();
        PlayDing();
    }

    private void PlayDing()
    {
        try
        {
            _dingPlayer.Play();
        }
        catch
        {
            SystemSounds.Asterisk.Play();
        }
    }
}