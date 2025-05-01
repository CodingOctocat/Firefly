namespace Firefly.Models.Messages;

public class FindInPageMessage
{
    public int FindIndex { get; }

    public string? Text { get; }

    public FindInPageMessage(string? text, int findIndex = -1)
    {
        Text = text;
        FindIndex = findIndex;
    }
}
