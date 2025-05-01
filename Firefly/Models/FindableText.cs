using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.Models;

/// <summary>
/// 表示可以在视图中导航查找的文本。
/// </summary>
public partial class FindableText : ObservableObject, IPageFindableInfo
{
    [ObservableProperty]
    public partial int Index { get; set; } = -1;

    [ObservableProperty]
    public partial bool IsMatch { get; set; }

    public int ItemIndex { get; set; }

    public string Text { get; set; }

    public FindableText(string text)
    {
        Text = text;
    }

    public static implicit operator FindableText(string text)
    {
        return new FindableText(text);
    }

    public static implicit operator string(FindableText findableText)
    {
        return findableText.Text;
    }

    public override string ToString()
    {
        return Text;
    }
}
