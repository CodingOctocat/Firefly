using System.Threading;
using System.Threading.Tasks;

using Firefly.Models.Responses;
using Firefly.Services.Requests;

namespace Firefly.Services.Abstractions;

public interface ICccfService
{
    Task<QueryResponse<Cccf>> QueryAsync(CccfRequest cccfRequest, CancellationToken cancellationToken = default);
}
