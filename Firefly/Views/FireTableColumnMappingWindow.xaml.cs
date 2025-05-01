using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models.Messages;
using Firefly.ViewModels;

using HcWindow = HandyControl.Controls.Window;

namespace Firefly.Views;

/// <summary>
/// FireTableColumnMappingWindow.xaml 的交互逻辑
/// </summary>
public partial class FireTableColumnMappingWindow : HcWindow
{
    public FireTableColumnMappingViewModel ViewModel { get; }

    public FireTableColumnMappingWindow(FireTableColumnMappingViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
        ViewModel = vm;

        WeakReferenceMessenger.Default.Register<ActionMessage, string>(this, "CloseFireTableColumnMappingWindow",
            (r, m) => Close());
    }
}
