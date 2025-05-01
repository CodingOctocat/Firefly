using Firefly.Services.Abstractions;

namespace Firefly.Factories;

public interface ICccfServiceFactory
{
    ICccfService Create(string resiliencePipelineKey);
}
