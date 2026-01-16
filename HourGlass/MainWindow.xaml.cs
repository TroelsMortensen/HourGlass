using System;
using System.IO;
using System.Media;
using System.Windows;
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

        MinutesSlider.Value = Math.Max(1, _timer.Duration.TotalMinutes);
        MinutesLabel.Text = $"{(int)MinutesSlider.Value:0} min";
        RemainingText.Text = FormatTime(_timer.Remaining);
        Hourglass.Progress = _timer.Progress;

        _timer.Tick += (_, _) => UpdateDisplay();
        _timer.Completed += (_, _) => OnTimerCompleted();

        StartPauseButton.Click += OnStartPauseClicked;
        ResetButton.Click += OnResetClicked;
        TopmostCheckBox.Checked += (_, _) => Topmost = true;
        TopmostCheckBox.Unchecked += (_, _) => Topmost = false;
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

    private void OnMinutesChanged()
    {
        MinutesLabel.Text = $"{(int)MinutesSlider.Value:0} min";
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