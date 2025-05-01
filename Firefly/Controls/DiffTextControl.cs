using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Firefly.Helpers;

namespace Firefly.Controls;

public class DiffTextControl : TextBlock
{
    private class DiffSegment
    {
        public bool IsSame { get; set; }

        public string Text { get; set; }

        public DiffSegment(string text, bool isSame)
        {
            Text = text;
            IsSame = isSame;
        }
    }

    public static readonly DependencyProperty BackgroundDiffProperty =
            DependencyProperty.Register(
            "BackgroundDiff",
            typeof(Brush),
            typeof(DiffTextControl),
            new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty BackgroundSameProperty =
        DependencyProperty.Register(
            "BackgroundSame",
            typeof(Brush),
            typeof(DiffTextControl),
            new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty ForegroundDiffProperty =
        DependencyProperty.Register(
            "ForegroundDiff",
            typeof(Brush),
            typeof(DiffTextControl),
            new PropertyMetadata(Brushes.Red));

    public static readonly DependencyProperty ForegroundSameProperty =
        DependencyProperty.Register(
            "ForegroundSame",
            typeof(Brush),
            typeof(DiffTextControl),
            new PropertyMetadata(Brushes.Black));

    public static readonly DependencyProperty Text2Property =
                DependencyProperty.Register(
            "Text2",
            typeof(string),
            typeof(DiffTextControl),
            new PropertyMetadata("", OnTextChanged));

    public new static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(DiffTextControl),
            new PropertyMetadata("", OnTextChanged));

    public Brush BackgroundDiff
    {
        get => (Brush)GetValue(BackgroundDiffProperty);
        set => SetValue(BackgroundDiffProperty, value);
    }

    public Brush BackgroundSame
    {
        get => (Brush)GetValue(BackgroundSameProperty);
        set => SetValue(BackgroundSameProperty, value);
    }

    public Brush ForegroundDiff
    {
        get => (Brush)GetValue(ForegroundDiffProperty);
        set => SetValue(ForegroundDiffProperty, value);
    }

    public Brush ForegroundSame
    {
        get => (Brush)GetValue(ForegroundSameProperty);
        set => SetValue(ForegroundSameProperty, value);
    }

    public new string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    static DiffTextControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffTextControl), new FrameworkPropertyMetadata(typeof(DiffTextControl)));
    }

    private static DiffSegment[] DiffStrings(string text, string text2)
    {
        if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(text2))
        {
            return [new DiffSegment(text, false)];
        }

        var diffList = new List<DiffSegment>();

        var comparedChars = text2.Distinct();

        var continuous = new List<char>();
        bool lastIsSame = comparedChars.Contains(text[0]);

        foreach (char c in text)
        {
            bool isSame = comparedChars.Contains(c);

            if (isSame != lastIsSame)
            {
                diffList.Add(new DiffSegment(new string([.. continuous]), lastIsSame));
                continuous.Clear();
            }

            continuous.Add(c);

            lastIsSame = isSame;
        }

        diffList.Add(new DiffSegment(new string([.. continuous]), lastIsSame));

        return [.. diffList];
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (ThemeManager.IsThemeChanging)
        {
            return;
        }

        var control = d as DiffTextControl;

        if (e.OldValue?.ToString() != e.NewValue?.ToString())
        {
            control?.UpdateText();
        }
    }

    private void UpdateText()
    {
        Inlines.Clear();

        var diff = DiffStrings(Text, Text2);

        foreach (var segment in diff)
        {
            var run = new Run {
                Text = segment.Text,
                Foreground = segment.IsSame ? ForegroundSame : ForegroundDiff,
                Background = segment.IsSame ? BackgroundSame : BackgroundDiff
            };

            Inlines.Add(run);
        }
    }
}
