using Firefly.Models.Abstractions;

namespace Firefly.Services.Navigation;

public abstract class NavigationSession<T> : IDeepCloneable<T> where T : NavigationSession<T>
{
    public abstract T DeepClone();
}
