using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Firefly.Behaviors;

/// <summary>
/// <see href="https://stackoverflow.com/a/42297391/4380178">Why does WPF textbox not support triple-click to select all text</see>
/// </summary>
public class TripleClickToSelectAllBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TripleClickToSelectAllBehavior),
            new PropertyMetadata(false, OnPropertyChanged));

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox tb)
        {
            if ((bool)e.NewValue)
            {
                tb.PreviewMouseLeftButtonDown += OnTextBoxMouseDown;
            }
            else
            {
                tb.PreviewMouseLeftButtonDown -= OnTextBoxMouseDown;
            }
        }
    }

    private static void OnTextBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBox textBox && e.ClickCount == 3)
        {
            textBox.SelectAll();
            textBox.ScrollToHome();
        }
    }
}
