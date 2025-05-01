using Firefly.Models;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;

namespace Firefly.Factories;

public class FireCheckContextFactory : IFireCheckContextFactory
{
    private readonly ICccfServiceFactory _cccfServiceFactory;

    private readonly IFireChecker<Cccf> _fireChecker;

    public FireCheckContextFactory(ICccfServiceFactory cccfServiceFactory, IFireChecker<Cccf> fireChecker)
    {
        _cccfServiceFactory = cccfServiceFactory;
        _fireChecker = fireChecker;
    }

    public FireCheckContext Create(FireProduct fireProduct)
    {
        return new FireCheckContext(fireProduct, _cccfServiceFactory, _fireChecker);
    }
}
