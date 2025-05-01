using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;

using Microsoft.Extensions.DependencyInjection;

using HcTextBox = HandyControl.Controls.TextBox;

namespace Firefly.Controls;

public class FindableTextBox : HcTextBox
{
    #region DependencyProperties

    // Using a DependencyProperty as the backing store for CccfFieldType.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CccfFieldTypeProperty =
        DependencyProperty.Register(
            "CccfFieldType",
            typeof(CccfFieldType),
            typeof(FindableTextBox),
            new PropertyMetadata(CccfFieldType.SmartMode));

    public CccfFieldType CccfFieldType
    {
        get => (CccfFieldType)GetValue(CccfFieldTypeProperty);
        set => SetValue(CccfFieldTypeProperty, value);
    }

    #endregion DependencyProperties

    private readonly IViewSwitcher _viewSwitcher;

    public IRelayCommand FindTextCommand => field ??= new RelayCommand(FindText, CanFind);

    public IAsyncRelayCommand QueryTextCommand => field ??= new AsyncRelayCommand(QueryTextAsync, CanQuery);

    public FindableTextBox()
    {
        _viewSwitcher = App.Current.Services.GetRequiredService<IViewSwitcher>();

        var contextMenu = new ContextMenu();

        contextMenu.Items.Add(new MenuItem {
            Header = "剪切",
            Command = ApplicationCommands.Cut,
            CommandTarget = this
        });

        contextMenu.Items.Add(new MenuItem {
            Header = "复制",
            Command = ApplicationCommands.Copy,
            CommandTarget = this
        });

        contextMenu.Items.Add(new MenuItem {
            Header = "粘贴",
            Command = ApplicationCommands.Paste,
            CommandTarget = this
        });

        ContextMenu = contextMenu;
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        ContextMenu.Items
            .Cast<FrameworkElement>()
            .Where(x => x.Name.StartsWith("__"))
            .ToList()
            .ForEach(ContextMenu.Items.Remove);

        string targetText = SelectedText;

        if (String.IsNullOrWhiteSpace(SelectedText))
        {
            targetText = Text;
        }

        var queryMenuItem = new MenuItem {
            Name = "__QueryText",
            Header = $"查询: {(String.IsNullOrWhiteSpace(targetText) ? "N/A" : targetText)}",
            Icon = Application.Current.Resources["SearchIcon"],
            InputGestureText = "Ctrl+F",
            Command = QueryTextCommand
        };

        ContextMenu.Items.Add(new Separator() { Name = "__Separator0" });
        ContextMenu.Items.Add(queryMenuItem);
        ContextMenu.Items.Add(new Separator() { Name = "__Separator1" });

        var findMenuItem = new MenuItem {
            Name = "__FindText",
            Header = $"在页面上查找: {(String.IsNullOrEmpty(targetText) ? "N/A" : targetText)}",
            Icon = Application.Current.Resources["FindInPageIcon"],
            InputGestureText = "Ctrl+Shift+F",
            Command = FindTextCommand
        };

        ContextMenu.Items.Add(findMenuItem);

        base.OnContextMenuOpening(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        Select(0, 0);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (String.IsNullOrEmpty(SelectedText))
        {
            SelectAll();
        }
    }

    private bool CanFind()
    {
        if (String.IsNullOrEmpty(SelectedText))
        {
            return false;
        }

        return true;
    }

    private bool CanQuery()
    {
        if (String.IsNullOrWhiteSpace(SelectedText))
        {
            return false;
        }

        return true;
    }

    private void FindText()
    {
        WeakReferenceMessenger.Default.Send(new FindInPageMessage(null));
        Select(0, 0);
    }

    private async Task QueryTextAsync()
    {
        var request = new CccfSmartRequest(1, SelectedText, CccfFieldType);

        await _viewSwitcher.SwitchToAsync(ActiveView.CccfQuery);
        await WeakReferenceMessenger.Default.Send(new ManualQueryAsyncRequestMessage(request));
    }
}
