using Firefly.Models;

namespace Firefly.Factories;

public interface IFireCheckContextFactory
{
    FireCheckContext Create(FireProduct fireProduct);
}
