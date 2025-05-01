using System.ComponentModel;

namespace Firefly.Models;

public interface IFireCheckSettings : INotifyPropertyChanging, INotifyPropertyChanged
{
    bool CheckManufactureDate { get; set; }

    bool CheckReportNumber { get; set; }

    bool StrictMode { get; set; }
}
