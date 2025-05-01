using System.Net.Http;

using Firefly.Models.Responses;
using Firefly.Services;
using Firefly.Services.Abstractions;

using Microsoft.Extensions.Configuration;

namespace Firefly.Factories;

public class CccfServiceFactory : ICccfServiceFactory
{
    public const string AutoPipeline = nameof(AutoPipeline);

    public const string ManualPipeline = nameof(ManualPipeline);

    public const string ScraperPipeline = nameof(ScraperPipeline);

    private readonly IConfiguration _configuration;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly IResponseParser<QueryResponse<Cccf>> _responseParser;

    public CccfServiceFactory(IHttpClientFactory httpClientFactory, IResponseParser<QueryResponse<Cccf>> parser, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _responseParser = parser;
        _configuration = configuration;
    }

    public ICccfService Create(string resiliencePipelineKey)
    {
        return new CccfService(_httpClientFactory.CreateClient(resiliencePipelineKey), _responseParser, _configuration);
    }
}
