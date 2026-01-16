using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using HourGlass.Services;

namespace HourGlass;

public partial class MainWindow : Window
{
    private readonly TimerService _timer = new();
    private readonly SoundPlayer _dingPlayer;
    private bool _isResetting;
    private bool _isCompleted;

    public MainWindow()
    {
        InitializeComponent();

        Topmost = true;
        TopmostToggle.IsChecked = true;

        var dingPath = Path.Combine(AppContext.BaseDirectory, "Assets", "ding.wav");
        _dingPlayer = new SoundPlayer(dingPath);

        MinutesSlider.Value = Math.Max(1, _timer.Duration.TotalMinutes);
        RemainingText.Text = FormatTime(_timer.Remaining);
        Hourglass.Progress = _timer.Progress;

        _timer.Tick += (_, _) => UpdateDisplay();
        _timer.Completed += (_, _) => OnTimerCompleted();

        StartPauseButton.Click += OnStartPauseClicked;
        ResetButton.Click += OnResetClicked;
        FlipButton.Click += OnFlipClicked;
        TopmostToggle.Checked += (_, _) => Topmost = true;
        TopmostToggle.Unchecked += (_, _) => Topmost = false;
        MinutesSlider.ValueChanged += (_, _) => OnMinutesChanged();
    }

    private void OnStartPauseClicked(object sender, RoutedEventArgs e)
    {
        if (_timer.IsRunning)
        {
            _timer.Pause();
        }
        else
        {
            ApplyMinutes();
            _timer.Start();
            _isCompleted = false;
        }

        UpdateDisplay();
    }

    private async void OnResetClicked(object sender, RoutedEventArgs e)
    {
        if (_isResetting)
        {
            return;
        }

        _timer.Pause();
        _timer.Reset();
        _isCompleted = false;
        UpdateDisplay();
    }

    private async void OnFlipClicked(object sender, RoutedEventArgs e)
    {
        if (_isResetting || !_isCompleted)
        {
            return;
        }

        _isResetting = true;
        StartPauseButton.IsEnabled = false;
        ResetButton.IsEnabled = false;
        FlipButton.IsEnabled = false;

        await Hourglass.BeginFlipAsync();
        _timer.Reset();
        _timer.Start();

        StartPauseButton.IsEnabled = true;
        ResetButton.IsEnabled = true;
        _isResetting = false;
        _isCompleted = false;
        UpdateDisplay();
    }

    private void OnMinutesChanged()
    {
        if (!_timer.IsRunning)
        {
            ApplyMinutes();
        }
    }

    private void ApplyMinutes()
    {
        if (_timer.IsRunning)
        {
            return;
        }

        var minutes = Math.Max(1, (int)MinutesSlider.Value);
        _timer.SetDuration(TimeSpan.FromMinutes(minutes));
        UpdateDisplay();
    }

    private string FormatTime(TimeSpan value)
    {
        return $"{(int)value.TotalMinutes:00}:00";
    }

    private void UpdateDisplay()
    {
        RemainingText.Text = FormatTime(_timer.Remaining);
        Hourglass.Progress = _timer.Progress;
        Hourglass.IsRunning = _timer.IsRunning;
        StartPauseIcon.Data = _timer.IsRunning
            ? Geometry.Parse("M 3 2 H 7 V 18 H 3 Z M 11 2 H 15 V 18 H 11 Z")
            : Geometry.Parse("M 3 2 L 16 10 L 3 18 Z");
        FlipButton.Visibility = _isCompleted && !_isResetting
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void OnTimerCompleted()
    {
        UpdateDisplay();
        _isCompleted = true;
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