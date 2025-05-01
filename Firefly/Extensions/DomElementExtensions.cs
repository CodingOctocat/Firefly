using AngleSharp.Dom;

namespace Firefly.Extensions;

public static class DomElementExtensions
{
    public static string GetInnerTextBySelectors(this IElement element, string selectors, bool trim = true)
    {
        string content = element.QuerySelector(selectors)?.InnerHtml ?? "";

        return trim ? content.Trim() : content;
    }

    public static string GetTextContentBySelectors(this IElement element, string selectors, bool trim = true)
    {
        string content = element.QuerySelector(selectors)?.TextContent ?? "";

        return trim ? content.Trim() : content;
    }
}
