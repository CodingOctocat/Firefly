using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Firefly.Services.Abstractions;

namespace Firefly.Services.Navigation;

public class NavigationService<T> : INavigationService<T> where T : NavigationSession<T>
{
    private readonly Stack<T> _backStack = [];

    private readonly Stack<T> _forwardStack = [];

    public bool CanGoBack => _backStack.Count > 1;

    public bool CanGoForward => _forwardStack.Count > 0;

    public T? Current => _backStack.Count > 0 ? _backStack.Peek() : default;

    public bool IsNavigating
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                IsNavigatingChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<bool>? IsNavigatingChanged;

    public event EventHandler<NavigationEventArgs<T>>? Navigated;

    public event EventHandler<NavigationEventArgs<T>>? Navigating;

    public T GoBack(int steps = 1)
    {
        OnNavigating(NavigationMode.Back);

        for (int i = 0; i < steps; i++)
        {
            var current = _backStack.Pop();
            _forwardStack.Push(current);
        }

        OnNavigated(NavigationMode.Back);

        return Current!;
    }

    public bool GoBackTo(T session)
    {
        int index = Array.IndexOf([.. _backStack], session);

        if (index > 0)
        {
            GoBack(index);

            return true;
        }

        return false;
    }

    public T GoForward(int steps = 1)
    {
        OnNavigating(NavigationMode.Forward);

        for (int i = 0; i < steps; i++)
        {
            var next = _forwardStack.Pop();
            _backStack.Push(next);
        }

        OnNavigated(NavigationMode.Forward);

        return Current!;
    }

    public bool GoForwardTo(T session)
    {
        int index = Array.IndexOf([.. _forwardStack], session);

        if (index >= 0)
        {
            GoForward(index + 1);

            return true;
        }

        return false;
    }

    public async Task NavigateAsync(Func<Task<T>> perform, bool replace = false)
    {
        OnNavigating(replace ? NavigationMode.Refresh : NavigationMode.New);

        var session = await perform();

        if (replace && _backStack.Count > 0)
        {
            _backStack.Pop();
        }

        _backStack.Push(session.DeepClone());
        _forwardStack.Clear();
        OnNavigated(replace ? NavigationMode.Refresh : NavigationMode.New);
    }

    protected virtual void OnNavigated(NavigationMode navigationMode)
    {
        try
        {
            Navigated?.Invoke(this, new(Current!, navigationMode, IsNavigating, _backStack, _forwardStack));
        }
        finally
        {
            IsNavigating = false;
        }
    }

    protected virtual void OnNavigating(NavigationMode navigationMode)
    {
        IsNavigating = true;
        Navigating?.Invoke(this, new(Current!, navigationMode, IsNavigating, _backStack, _forwardStack));
    }
}
