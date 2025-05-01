using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.Models;

public partial class FireTableColumnVisibility : ObservableObject
{
    [ObservableProperty]
    public partial bool C1 { get; set; } = false;

    [ObservableProperty]
    public partial bool Cccf { get; set; } = false;

    [ObservableProperty]
    public partial bool CertificateNumber { get; set; } = true;

    [ObservableProperty]
    public partial bool Count { get; set; } = false;

    [ObservableProperty]
    public partial bool EnterpriseName { get; set; } = true;

    [ObservableProperty]
    public partial bool ManufactureDate { get; set; } = true;

    [ObservableProperty]
    public partial bool Model { get; set; } = true;

    [ObservableProperty]
    public partial bool ProductName { get; set; } = true;

    [ObservableProperty]
    public partial bool Remark { get; set; } = true;

    [ObservableProperty]
    public partial bool ReportNumber { get; set; } = true;

    [ObservableProperty]
    public partial bool Status { get; set; } = false;
}
