using System;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.Models;

public partial class FireTableSkipRowRule : ObservableObject
{
    public bool HasError => TargetColumn < 1 || String.IsNullOrWhiteSpace(MatchText);

    [ObservableProperty]
    [JsonPropertyOrder(0)]
    public partial bool IsEnabled { get; set; } = true;

    [ObservableProperty]
    [JsonPropertyOrder(3)]
    public partial bool MatchCase { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [JsonPropertyOrder(2)]
    public partial string MatchText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [JsonPropertyOrder(1)]
    public partial int TargetColumn { get; set; }

    [ObservableProperty]
    [JsonPropertyOrder(4)]
    public partial bool UseFuzzyMatching { get; set; } = false;

    public bool IsSkip(string targetText)
    {
        if (String.IsNullOrWhiteSpace(MatchText) || String.IsNullOrWhiteSpace(targetText))
        {
            return false;
        }

        var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        return UseFuzzyMatching
            ? targetText.Contains(MatchText, comparison)
            : String.Equals(targetText, MatchText, comparison);
    }

    public void RaiseTargetColumn()
    {
        OnPropertyChanged(nameof(TargetColumn));
    }
}
