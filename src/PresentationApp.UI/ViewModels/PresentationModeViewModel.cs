using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.ViewModels;

public sealed class SlideChangedEventArgs : EventArgs
{
    public SlideChangedEventArgs(int oldIndex, int newIndex) { OldIndex = oldIndex; NewIndex = newIndex; }
    public int OldIndex { get; }
    public int NewIndex { get; }
}

/// <summary>ViewModel du diaporama plein écran (navigation, minuterie, mode présentateur).</summary>
public partial class PresentationModeViewModel : ObservableObject
{
    private readonly Presentation _presentation;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private DateTime _startTime;

    public event EventHandler<SlideChangedEventArgs>? SlideChanged;
    public event EventHandler? ExitRequested;

    [ObservableProperty] private string _elapsedText = "00:00";
    [ObservableProperty] private bool _isPresenterVisible;

    private int _currentIndex;

    public PresentationModeViewModel(Presentation presentation, int startIndex = 0)
    {
        _presentation = presentation;
        _currentIndex = Math.Clamp(startIndex, 0, Math.Max(0, presentation.Slides.Count - 1));
        _timer.Tick += (_, _) => ElapsedText = (DateTime.Now - _startTime).ToString(@"mm\:ss");
    }

    public int Count => _presentation.Slides.Count;
    public int CurrentIndex => _currentIndex;
    public Slide? CurrentSlide => Count > 0 ? _presentation.Slides[_currentIndex] : null;
    public Slide? NextSlide => _currentIndex + 1 < Count ? _presentation.Slides[_currentIndex + 1] : null;
    public string SlideNumberText => Count == 0 ? "0 / 0" : $"{_currentIndex + 1} / {Count}";

    public void Start() { _startTime = DateTime.Now; _timer.Start(); }
    public void Stop() => _timer.Stop();

    public void GoTo(int index)
    {
        if (Count == 0) return;
        index = Math.Clamp(index, 0, Count - 1);
        if (index == _currentIndex) return;

        int old = _currentIndex;
        _currentIndex = index;
        OnPropertyChanged(nameof(CurrentIndex));
        OnPropertyChanged(nameof(CurrentSlide));
        OnPropertyChanged(nameof(NextSlide));
        OnPropertyChanged(nameof(SlideNumberText));
        SlideChanged?.Invoke(this, new SlideChangedEventArgs(old, index));
    }

    [RelayCommand] private void Next() => GoTo(_currentIndex + 1);
    [RelayCommand] private void Previous() => GoTo(_currentIndex - 1);
    [RelayCommand] private void First() => GoTo(0);
    [RelayCommand] private void Last() => GoTo(Count - 1);
    [RelayCommand] private void TogglePresenter() => IsPresenterVisible = !IsPresenterVisible;
    [RelayCommand] private void Exit() => ExitRequested?.Invoke(this, EventArgs.Empty);
}
