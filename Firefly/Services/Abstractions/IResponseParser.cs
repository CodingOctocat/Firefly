using System.Threading.Tasks;

namespace Firefly.Services.Abstractions;

public interface IResponseParser<T>
{
    Task<T> ParseAsync(string text);
}
