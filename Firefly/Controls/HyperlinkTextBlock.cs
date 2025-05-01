using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

using Firefly.Helpers;

namespace Firefly.Controls;

public class HyperlinkTextBlock : TextBlock
{
    // Using a DependencyProperty as the backing store for DisplayText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(
            "DisplayText",
            typeof(string),
            typeof(HyperlinkTextBlock),
            new PropertyMetadata(null, OnNavigateUriChanged));

    // Using a DependencyProperty as the backing store for NavigateUri.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NavigateUriProperty =
        DependencyProperty.Register(
            "NavigateUri",
            typeof(string),
            typeof(HyperlinkTextBlock),
            new PropertyMetadata(null, OnNavigateUriChanged));

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty); set => SetValue(DisplayTextProperty, value);
    }

    public string NavigateUri
    {
        get => (string)GetValue(NavigateUriProperty); set => SetValue(NavigateUriProperty, value);
    }

    private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        UriHelper.OpenUri(e.Uri.AbsoluteUri);

        e.Handled = true;
    }

    private static void OnNavigateUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HyperlinkTextBlock textBlock && textBlock.NavigateUri is not null)
        {
            textBlock.Inlines.Clear();

            var hyperlink = new Hyperlink {
                NavigateUri = new Uri(textBlock.NavigateUri, UriKind.RelativeOrAbsolute)
            };

            hyperlink.Inlines.Add(new TextBlock() { Text = textBlock.DisplayText ?? textBlock.NavigateUri });
            hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            textBlock.Inlines.Add(hyperlink);
        }
    }
}
