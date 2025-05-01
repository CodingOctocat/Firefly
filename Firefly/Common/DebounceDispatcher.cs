using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Firefly.Common;

/// <summary>
/// Debounce Dispatcher.
/// <para>
/// forked from: <seealso href="https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Uwp/Extensions/DispatcherQueueTimerExtensions.cs">Microsoft.Toolkit.Uwp.Extensions.DispatcherQueueTimerExtensions</seealso>.
/// </para>
/// </summary>
public static class DebounceDispatcher
{
    private static readonly ConcurrentDictionary<DispatcherTimer, Action> _debounceActionDispatcherTimerInstances = new();

    private static readonly ConcurrentDictionary<DispatcherTimer, Func<Task>> _debounceAsyncActionDispatcherTimerInstances = new();

    /// <summary>
    /// Cancels any pending debounce operation for the given <see cref="DispatcherTimer"/>.
    /// This method stops the timer, removes associated event handlers,
    /// and clears any stored (asynchronous)actions to prevent them from executing.
    /// </summary>
    /// <param name="timer">The <see cref="DispatcherTimer"/> instance to cancel.</param>
    public static void Cancel(DispatcherTimer timer)
    {
        if (timer.IsEnabled)
        {
            timer.Stop();
        }

        timer.Tick -= Timer_Tick;
        timer.Tick -= Timer_TickAsync;

        _debounceActionDispatcherTimerInstances.TryRemove(timer, out _);
        _debounceAsyncActionDispatcherTimerInstances.TryRemove(timer, out _);
    }

    /// <summary>
    /// Cancels any pending debounce operation for current <see cref="DispatcherTimer"/>.
    /// This method stops the timer, removes associated event handlers,
    /// and clears any stored (asynchronous)actions to prevent them from executing.
    /// </summary>
    /// <param name="timer">The <see cref="DispatcherTimer"/> instance to cancel.</param>
    public static void CancelDebounce(this DispatcherTimer timer)
    {
        Cancel(timer);
    }

    /// <summary>
    /// <para>Used to debounce (rate-limit) an event.  The action will be postponed and executed after the interval has elapsed.  At the end of the interval, the function will be called with the arguments that were passed most recently to the debounced function.</para>
    /// <para>Use this method to control the timer instead of calling Start/Interval/Stop manually.</para>
    /// <para>A scheduled debounce can still be stopped by calling the stop method on the timer instance.</para>
    /// <para>Each timer can only have one debounced function limited at a time.</para>
    /// </summary>
    /// <param name="timer">Timer instance, only one debounced function can be used per timer.</param>
    /// <param name="action">Action to execute at the end of the interval.</param>
    /// <param name="interval">Interval to wait before executing the action.</param>
    /// <param name="immediate">Determines if the action execute on the leading edge instead of trailing edge.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <example>
    /// <code>
    /// private DispatcherTimer _typeTimer = new();
    ///
    /// _typeTimer.Debounce(async () =>
    ///     {
    ///         // Only executes this code after 0.3 seconds have elapsed since last trigger.
    ///     }, TimeSpan.FromSeconds(0.3));
    /// </code>
    /// </example>
    public static void Debounce(this DispatcherTimer timer, Action action, TimeSpan interval, bool immediate = false, CancellationToken cancellationToken = default)
    {
        Cancel(timer);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Check and stop any existing timer
        bool timeout = timer.IsEnabled;

        if (timeout)
        {
            timer.Stop();
        }

        // Reset timer parameters
        timer.Tick -= Timer_Tick;
        timer.Interval = interval;

        if (immediate)
        {
            // If we're in immediate mode then we only execute if the timer wasn't running beforehand
            if (!timeout)
            {
                action();
            }
        }
        else
        {
            // If we're not in immediate mode, then we'll execute when the current timer expires.
            timer.Tick += Timer_Tick;

            // Store/Update function
            _debounceActionDispatcherTimerInstances.AddOrUpdate(timer, action, (k, v) => action);

            cancellationToken.Register(() => Cancel(timer));
        }

        // Start the timer to keep track of the last call here.
        timer.Start();
    }

    /// <summary>
    /// <para>Used to debounce (rate-limit) an event.  The asyncAction will be postponed and executed after the interval has elapsed.  At the end of the interval, the function will be called with the arguments that were passed most recently to the debounced function.</para>
    /// <para>Use this method to control the timer instead of calling Start/Interval/Stop manually.</para>
    /// <para>A scheduled debounce can still be stopped by calling the stop method on the timer instance.</para>
    /// <para>Each timer can only have one debounced function limited at a time.</para>
    /// </summary>
    /// <param name="timer">Timer instance, only one debounced function can be used per timer.</param>
    /// <param name="asyncAction">Asynchronous action to execute at the end of the interval.</param>
    /// <param name="interval">Interval to wait before executing the asyncAction.</param>
    /// <param name="immediate">Determines if the asyncAction execute on the leading edge instead of trailing edge.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <example>
    /// <code>
    /// private DispatcherTimer _typeTimer = new();
    ///
    /// await _typeTimer.DebounceAsync(async () =>
    ///     {
    ///         // Only executes this code after 0.3 seconds have elapsed since last trigger.
    ///     }, TimeSpan.FromSeconds(0.3));
    /// </code>
    /// </example>
    public static async Task DebounceAsync(this DispatcherTimer timer, Func<Task> asyncAction, TimeSpan interval, bool immediate = false, CancellationToken cancellationToken = default)
    {
        Cancel(timer);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Check and stop any existing timer
        bool timeout = timer.IsEnabled;

        if (timeout)
        {
            timer.Stop();
        }

        // Reset timer parameters
        timer.Tick -= Timer_TickAsync;
        timer.Interval = interval;

        if (immediate)
        {
            // If we're in immediate mode then we only execute if the timer wasn't running beforehand
            if (!timeout)
            {
                await asyncAction();
            }
        }
        else
        {
            // If we're not in immediate mode, then we'll execute when the current timer expires.
            timer.Tick += Timer_TickAsync;

            // Store/Update function
            _debounceAsyncActionDispatcherTimerInstances.AddOrUpdate(timer, asyncAction, (k, v) => asyncAction);

            cancellationToken.Register(() => Cancel(timer));
        }

        // Start the timer to keep track of the last call here.
        timer.Start();
    }

    private static void Timer_Tick(object? sender, object e)
    {
        // This event is only registered/run if we weren't in immediate mode above
        if (sender is DispatcherTimer timer)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();

            if (_debounceActionDispatcherTimerInstances.TryRemove(timer, out var action))
            {
                action();
            }
        }
    }

    private static async void Timer_TickAsync(object? sender, object e)
    {
        // This event is only registered/run if we weren't in immediate mode above
        if (sender is DispatcherTimer timer)
        {
            timer.Tick -= Timer_TickAsync;
            timer.Stop();

            if (_debounceAsyncActionDispatcherTimerInstances.TryRemove(timer, out var func))
            {
                await func();
            }
        }
    }
}
