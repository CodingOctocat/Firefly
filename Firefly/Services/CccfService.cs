using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Firefly.Models.Responses;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;

using Microsoft.Extensions.Configuration;

namespace Firefly.Services;

public sealed class CccfService : ICccfService
{
    private readonly IConfiguration _configuration;

    private readonly HttpClient _httpClient;

    private readonly IResponseParser<QueryResponse<Cccf>> _responseParser;

    public CccfService(HttpClient httpClient, IResponseParser<QueryResponse<Cccf>> parser, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _responseParser = parser;
        _configuration = configuration;
    }

    public async Task<QueryResponse<Cccf>> QueryAsync(CccfRequest cccfRequest, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(cccfRequest.ToNameValueCollection());

        var resp = await _httpClient.PostAsync(_configuration["CccfApi:Endpoints:Query"], content, cancellationToken);
        resp.EnsureSuccessStatusCode();

        string page = await resp.Content.ReadAsStringAsync(cancellationToken);
        var result = await _responseParser.ParseAsync(page);

        return result;
    }
}
