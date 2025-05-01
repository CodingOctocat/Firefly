namespace Firefly.Models;

public interface IPageFindableInfo
{
    int Index { get; set; }

    bool IsMatch { get; set; }

    int ItemIndex { get; set; }

    string Text { get; set; }
}
