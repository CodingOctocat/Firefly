﻿using System.Windows;

namespace Firefly.Common;

/// <summary>
/// <see href="http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/"/>
/// </summary>
public class BindingProxy : Freezable
{
    // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            "Data",
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null));

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
