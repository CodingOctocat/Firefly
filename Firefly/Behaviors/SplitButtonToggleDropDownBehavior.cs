﻿using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using HandyControl.Controls;

using Microsoft.Xaml.Behaviors;

namespace Firefly.Behaviors;

/// <summary>
/// 使 SplitButton 的下拉显示表现为切换打开行为。
/// </summary>
public class SplitButtonToggleDropDownBehavior : Behavior<SplitButton>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;

        base.OnDetaching();
    }

    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var result = VisualTreeHelper.HitTest(AssociatedObject, e.GetPosition(AssociatedObject));

        // 如果 VisualHit 为 Path，则鼠标在点击 SplitButton 的 DropDown 位置
        // 如果为 TextBlock，则鼠标在点击 SplitButton 的非 DropDown 位置
        // 如果为 Border，并且其 Child 不为 null，则鼠标在点击 DropDown 位置，否则，鼠标在点击 非 DropDown 位置
        // 如果为 null，则鼠标在点击其他位置
        // 如果不判断 VisualHit，则在点击下拉项前，此事件将被调用，下拉项被关闭，导致下拉项无法被点击
        if (AssociatedObject.IsDropDownOpen && result?.VisualHit is Border or Path or TextBlock)
        {
            AssociatedObject.IsDropDownOpen = false;
            e.Handled = true;
        }
    }
}
