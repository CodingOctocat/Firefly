using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Firefly.Helpers;

public static class VisualTreeHelperEx
{
    public static T? FindVisualChild<T>(DependencyObject parent, Predicate<T>? predicate = null) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild && (predicate is null || predicate(typedChild)))
            {
                return typedChild;
            }

            var descendent = FindVisualChild<T>(child, predicate);

            if (descendent is not null)
            {
                return descendent;
            }
        }

        return null;
    }

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent, Predicate<T>? predicate = null) where T : DependencyObject
    {
        if (parent is null)
        {
            yield break;
        }

        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild && (predicate is null || predicate(typedChild)))
            {
                yield return typedChild;
            }

            foreach (var descendant in FindVisualChildren(child, predicate))
            {
                yield return descendant;
            }
        }
    }

    public static T? FindVisualParent<T>(DependencyObject child, Predicate<T>? predicate = null) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T parent && (predicate is null || predicate(parent)))
            {
                return parent;
            }

            while (child is not (Visual or Visual3D))
            {
                child = LogicalTreeHelper.GetParent(child);
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}
