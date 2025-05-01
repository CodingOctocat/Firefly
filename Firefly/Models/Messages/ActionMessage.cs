namespace Firefly.Models.Messages;

public class ActionMessage;

public class ActionMessage<T> : ActionMessage
{
    public T Tag { get; }

    public ActionMessage(T tag)
    {
        Tag = tag;
    }
}
