using System;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Firefly.Extensions;

namespace Firefly.Models;

public partial class ComboBoxItemWrapper<T> : ObservableObject
{
    [ObservableProperty]
    public partial string Display { get; set; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial object? Tag { get; set; }

    [ObservableProperty]
    public partial T Value { get; set; }

    public ComboBoxItemWrapper(T value, string display, bool isEnabled = true, bool isVisible = true)
    {
        Value = value;
        Display = display;
        IsEnabled = isEnabled;
        IsVisible = isVisible;
    }
}

public class ComboBoxItemWrapper : ComboBoxItemWrapper<object>
{
    public ComboBoxItemWrapper(object value, string display, bool isEnabled = true, bool isVisible = true) : base(value, display, isEnabled, isVisible)
    { }

    public static ComboBoxItemWrapper<TEnum>[] CreateByValueDescription<TEnum>(Predicate<TEnum>? isEnabledPredicate = null, Predicate<TEnum>? isVisiblePredicate = null)
        where TEnum : struct, Enum
    {
        return [.. Enum.GetValues<TEnum>()
            .Where(x => isVisiblePredicate?.Invoke(x) ?? true)
            .OrderBy(x => Convert.ToInt32(x))
            .Select(x => new ComboBoxItemWrapper<TEnum>(
                x,
                x.GetDescription(),
                isEnabledPredicate?.Invoke(x) ?? true))];
    }
}
