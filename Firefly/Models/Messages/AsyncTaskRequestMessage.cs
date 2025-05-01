using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Firefly.Models.Messages;

/// <summary>
/// 用法: 务必调用 2 次 await。
/// <code>await await Messenger.Send(AsyncTaskRequestMessage)</code>
/// </summary>
public class AsyncTaskRequestMessage : AsyncRequestMessage<Task>;

/// <summary>
/// 用法: 务必调用 2 次 await。
/// <code>await await Messenger.Send(AsyncTaskRequestMessage)</code>
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public class AsyncTaskRequestMessage<TRequest> : AsyncTaskRequestMessage
{
    [MaybeNull]
    public TRequest Request { get; set; }

    public AsyncTaskRequestMessage([MaybeNull] TRequest request)
    {
        Request = request;
    }
}

/// <summary>
/// 用法: 务必调用 2 次 await。
/// <code>await await Messenger.Send(AsyncTaskRequestMessage)</code>
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class AsyncTaskRequestMessage<TRequest, TResponse> : AsyncRequestMessage<Task<TResponse>>
{
    [MaybeNull]
    public TRequest Request { get; set; }

    public AsyncTaskRequestMessage([MaybeNull] TRequest request)
    {
        Request = request;
    }
}
