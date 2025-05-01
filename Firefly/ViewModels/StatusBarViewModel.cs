using System;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Firefly.ViewModels;

public partial class StatusBarViewModel : ObservableObject
{
    public string ReleasedLongTime => ReleasedTime.ToString("编译于 yyyy/MM/dd dddd tt h:mm:ss.fff");

    public DateTime ReleasedTime => File.GetLastWriteTime(GetType().Assembly.Location);
}
