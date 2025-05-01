using System.Windows.Documents;
using System.Windows.Navigation;

using Firefly.Helpers;

namespace Firefly.Controls;

/// <summary>
/// <see href="https://stackoverflow.com/a/27609749/4380178"/>
/// </summary>
public class ExternalBrowserHyperlink : Hyperlink
{
    public ExternalBrowserHyperlink()
    {
        RequestNavigate += OnRequestNavigate;
    }

    private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        UriHelper.OpenUri(e.Uri.AbsoluteUri);

        e.Handled = true;
    }
}
