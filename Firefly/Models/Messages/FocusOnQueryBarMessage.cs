namespace Firefly.Models.Messages;

public class FocusOnCombinedQueryBarMessage : FocusOnQueryBarMessage
{
    public FocusOnCombinedQueryBarMessage(bool needsSelectAll = false, bool isRestore = true) : base(needsSelectAll, isRestore)
    { }
}

public class FocusOnQueryBarMessage
{
    public bool IsRestore { get; }

    public bool NeedsSelectAll { get; }

    public FocusOnQueryBarMessage(bool needsSelectAll = false, bool isRestore = true)
    {
        NeedsSelectAll = needsSelectAll;
        IsRestore = isRestore;
    }
}

public class FocusOnSmartQueryBarMessage : FocusOnQueryBarMessage
{
    public FocusOnSmartQueryBarMessage(bool needsSelectAll = false) : base(needsSelectAll)
    { }
}

public class FocusOnFindInPageBarMessage
{
    public object DataContext { get; }

    public FocusOnFindInPageBarMessage(object dataContext)
    {
        DataContext = dataContext;
    }
}
