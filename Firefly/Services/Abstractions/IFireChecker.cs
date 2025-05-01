using Firefly.Models;
using Firefly.Models.Responses;

namespace Firefly.Services.Abstractions;

public interface IFireChecker<TTarget>
{
    FireErrors Check(FireProduct fireProduct, Cccf? target);

    bool PreCheck(FireProduct fireProduct, out FireErrors fireErrors);
}
