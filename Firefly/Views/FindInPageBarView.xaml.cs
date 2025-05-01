using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models.Messages;

namespace Firefly.Views;

/// <summary>
/// FindInPageBarView.xaml 的交互逻辑
/// </summary>
public partial class FindInPageBarView : UserControl
{
    public FindInPageBarView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<FocusOnFindInPageBarMessage>(this,
            (r, m) => {
                if (ReferenceEquals(m.DataContext, DataContext))
                {
                    RestoreFocus();
                }
            });
    }

    private void RestoreFocus()
    {
        tbFindInPage.Focus();
        Keyboard.Focus(tbFindInPage);
        tbFindInPage.SelectAll();
    }
}
