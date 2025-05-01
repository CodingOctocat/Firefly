using System;
using System.Threading.Tasks;

using Firefly.Services.Navigation;

namespace Firefly.Services.Abstractions;

public interface INavigationService<T> where T : NavigationSession<T>
{
    bool CanGoBack { get; }

    bool CanGoForward { get; }

    T? Current { get; }

    bool IsNavigating { get; }

    event EventHandler<bool> IsNavigatingChanged;

    event EventHandler<NavigationEventArgs<T>> Navigated;

    event EventHandler<NavigationEventArgs<T>> Navigating;

    T GoBack(int steps = 1);

    bool GoBackTo(T session);

    T GoForward(int steps = 1);

    bool GoForwardTo(T session);

    Task NavigateAsync(Func<Task<T>> perform, bool replace = false);
}
