using Firefly.Models.Requests;

namespace Firefly.Models.Messages;

public class ManualQueryAsyncRequestMessage : AsyncTaskRequestMessage<ICccfRequest>
{
    public ManualQueryAsyncRequestMessage(ICccfRequest request) : base(request)
    { }
}
