namespace Firefly.Models.Messages;

public class SwitchViewMessage
{
    public ActiveView Page { get; }

    public object? Tag { get; set; }

    public SwitchViewMessage(ActiveView page)
    {
        Page = page;
    }
}
