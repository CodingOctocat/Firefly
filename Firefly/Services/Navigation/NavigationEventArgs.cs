using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Firefly.Services.Navigation;

public class NavigationEventArgs<T> : EventArgs
{
    public ImmutableStack<T> BackStack { get; }

    public T Current { get; }

    public ImmutableStack<T> ForwardStack { get; }

    public bool IsNavigating { get; }

    public NavigationMode NavigationMode { get; }

    public NavigationEventArgs(T current, NavigationMode navigationMode, bool isNavigating, Stack<T> backStack, Stack<T> forwardStack)
    {
        Current = current;
        NavigationMode = navigationMode;
        IsNavigating = isNavigating;
        BackStack = [.. backStack];
        ForwardStack = [.. forwardStack];
    }
}
