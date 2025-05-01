namespace Firefly.Models.Abstractions;

public interface IDeepCloneable<out T> where T : class
{
    T DeepClone();
}
