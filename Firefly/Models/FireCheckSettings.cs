using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.Models;

public partial class FireCheckSettings : ObservableObject, IFireCheckSettings
{
    [ObservableProperty]
    public partial bool CheckManufactureDate { get; set; }

    [ObservableProperty]
    public partial bool CheckReportNumber { get; set; }

    [ObservableProperty]
    public partial bool StrictMode { get; set; }
}
