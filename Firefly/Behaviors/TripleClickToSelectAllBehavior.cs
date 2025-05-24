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

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewMouseLeftButtonDown += OnTextBoxMouseDown;
            }
            else
            {
                textBox.PreviewMouseLeftButtonDown -= OnTextBoxMouseDown;
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
