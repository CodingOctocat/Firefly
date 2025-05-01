using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.Models;

public partial class FindInPageOptions : ObservableObject
{
    [ObservableProperty]
    public partial bool MatchCase { get; set; }

    [ObservableProperty]
    public partial bool MatchWholeWord { get; set; }

    [ObservableProperty]
    public partial bool UseRegex { get; set; }
}
