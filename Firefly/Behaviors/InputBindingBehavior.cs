using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Firefly.Behaviors;

/// <summary>
/// forked from: <see href="https://stackoverflow.com/a/55903060/4380178">InputBindings work only when focused</see>
/// </summary>
public class InputBindingBehavior
{
    public static readonly DependencyProperty PropagateInputBindingsToWindowProperty =
        DependencyProperty.RegisterAttached(
            "PropagateInputBindingsToWindow",
            typeof(bool),
            typeof(InputBindingBehavior),
            new PropertyMetadata(false, OnPropagateInputBindingsToWindowChanged));

    private static readonly Dictionary<int, Tuple<WeakReference<FrameworkElement>, List<InputBinding>>> _trackedFrameWorkElementsToBindings = [];

    public static bool GetPropagateInputBindingsToWindow(FrameworkElement obj)
    {
        return (bool)obj.GetValue(PropagateInputBindingsToWindowProperty);
    }

    public static void SetPropagateInputBindingsToWindow(FrameworkElement obj, bool value)
    {
        obj.SetValue(PropagateInputBindingsToWindowProperty, value);
    }

    private static void AddInputBindings(FrameworkElement sender)
    {
        var window = Window.GetWindow(sender);

        if (window is not null)
        {
            // transfer InputBindings into our control
            if (!_trackedFrameWorkElementsToBindings.TryGetValue(sender.GetHashCode(), out var trackingData))
            {
                trackingData = Tuple.Create(
                    new WeakReference<FrameworkElement>(sender),
                    sender.InputBindings.Cast<InputBinding>().ToList());

                _trackedFrameWorkElementsToBindings.Add(sender.GetHashCode(), trackingData);
            }

            // apply Bindings to Window
            foreach (var inputBinding in trackingData.Item2)
            {
                window.InputBindings.Add(inputBinding);
            }

            sender.InputBindings.Clear();
        }
    }

    private static void CleanupBindingsDictionary(Window window, Dictionary<int, Tuple<WeakReference<FrameworkElement>, List<InputBinding>>> bindingsDictionary)
    {
        foreach (int hashCode in bindingsDictionary.Keys.ToList())
        {
            if (bindingsDictionary.TryGetValue(hashCode, out var trackedData)
                && !trackedData.Item1.TryGetTarget(out _))
            {
                Debug.WriteLine($"InputBindingBehavior: FrameWorkElement {hashCode} did never unload but was GCed, cleaning up leftover KeyBindings");

                foreach (var binding in trackedData.Item2)
                {
                    window.InputBindings.Remove(binding);
                }

                trackedData.Item2.Clear();
                bindingsDictionary.Remove(hashCode);
            }
        }
    }

    private static void OnFrameworkElementIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;

        if (e.NewValue is true)
        {
            AddInputBindings(frameworkElement);
        }
        else
        {
            RemoveInputBindings(frameworkElement, false);
        }
    }

    private static void OnFrameworkElementLoaded(object sender, RoutedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        AddInputBindings(frameworkElement);
        frameworkElement.IsVisibleChanged += OnFrameworkElementIsVisibleChanged;
    }

    private static void OnFrameworkElementUnloaded(object sender, RoutedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        RemoveInputBindings(frameworkElement);
        frameworkElement.IsVisibleChanged -= OnFrameworkElementIsVisibleChanged;
    }

    private static void OnPropagateInputBindingsToWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)d;
        frameworkElement.Loaded += OnFrameworkElementLoaded;
        frameworkElement.Unloaded += OnFrameworkElementUnloaded;
    }

    private static void RemoveInputBindings(FrameworkElement sender, bool track = true)
    {
        var window = Window.GetWindow(sender);
        int hashCode = sender.GetHashCode();

        // remove Bindings from Window
        if (window is not null)
        {
            if (_trackedFrameWorkElementsToBindings.TryGetValue(hashCode, out var trackedData))
            {
                foreach (var binding in trackedData.Item2)
                {
                    sender.InputBindings.Add(binding);
                    window.InputBindings.Remove(binding);
                    Debug.WriteLine(binding.Command);
                }

                if (track)
                {
                    trackedData.Item2.Clear();
                    _trackedFrameWorkElementsToBindings.Remove(hashCode);

                    // catch removed and orphaned entries
                    CleanupBindingsDictionary(window, _trackedFrameWorkElementsToBindings);
                }
            }
        }
    }
}
