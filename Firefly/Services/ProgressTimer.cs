using System;
using System.Timers;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.Services;

public sealed partial class ProgressTimer : ObservableObject, IDisposable
{
    private readonly Timer _timer;

    private DateTime _startTime;

    /// <summary>
    /// 值为 <see langword="null"/> 表示计时器已开始工作，且处于第一个间隔周期。可用于指示 “正在估算剩余时间”。"/>
    /// </summary>
    [ObservableProperty]
    public partial TimeSpan? Countdown { get; private set; } = TimeSpan.Zero;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    public partial int CurrentProgress { get; private set; }

    [ObservableProperty]
    public partial int Interval { get; set; } = 1000;

    public double ProgressPercentage => TotalTasks == 0 ? 0d : (double)CurrentProgress / TotalTasks;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    public partial int TotalTasks { get; set; }

    [ObservableProperty]
    public partial TimeSpan UsedTime { get; private set; }

    public ProgressTimer(int totalTasks, int interval = 1000)
    {
        _timer = new Timer(interval);
        TotalTasks = totalTasks;
        Interval = interval;
    }

    public ProgressTimer() : this(0)
    { }

    public void Dispose()
    {
        Stop();
        _timer.Dispose();
    }

    public void Reset()
    {
        UsedTime = TimeSpan.Zero;
        Countdown = TimeSpan.Zero;
        CurrentProgress = 0;
    }

    public void Start(bool countDownToZero = false)
    {
        Countdown = countDownToZero ? TimeSpan.Zero : null;
        _startTime = DateTime.Now;
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();
    }

    public void Stop(bool countDownToZero = false)
    {
        _timer.Stop();
        _timer.Elapsed -= Timer_Elapsed;

        if (countDownToZero)
        {
            Countdown = TimeSpan.Zero;
        }
    }

    public void UpdateProgress(int progress)
    {
        CurrentProgress = progress;
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        UsedTime = DateTime.Now - _startTime;

        if (CurrentProgress == 0)
        {
            Countdown = null;

            return;
        }

        double avg = UsedTime.TotalSeconds / CurrentProgress;
        Countdown = TimeSpan.FromSeconds(avg * Math.Abs(TotalTasks - CurrentProgress));
    }
}
