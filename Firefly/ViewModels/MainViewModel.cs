using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Firefly.Controls;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Models.Requests;
using Firefly.Properties;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    #region Fields

    private readonly IViewSwitcher _viewSwitcher;

    #endregion Fields

    #region Properties

    public FindInPageBarViewModel? ActiveFindInPageBarViewModel => ActiveView switch {
        ActiveView.Firefly => FireflyViewModel.FindInPageBarViewModel,
        ActiveView.CccfQuery => CccfMainQueryViewModel.FindInPageBarViewModel,
        _ => null
    };

    [ObservableProperty]
    public partial ActiveView ActiveView { get; set; } = ActiveView.Firefly;

    public bool CanFindInPage => ActiveView switch {
        ActiveView.Firefly => FireflyViewModel.FindInPageBarViewModel.CanExecuteFindCommand,
        ActiveView.CccfQuery => CccfMainQueryViewModel.FindInPageBarViewModel.CanExecuteFindCommand,
        _ => false
    };

    public CccfMainQueryViewModel CccfMainQueryViewModel { get; }

    public FireflyViewModel FireflyViewModel { get; }

    #endregion Properties

    #region Constructors & Recipients

    public MainViewModel(FireflyViewModel fireflyViewModel, CccfMainQueryViewModel cccfMainQueryViewModel, IViewSwitcher viewSwitcher)
    {
        IsActive = true;

        FireflyViewModel = fireflyViewModel;
        CccfMainQueryViewModel = cccfMainQueryViewModel;
        _viewSwitcher = viewSwitcher;

        FireflyViewModel.FindInPageBarViewModel.PropertyChanged += ActiveFindInPageBarViewModel_PropertyChanged;
        CccfMainQueryViewModel.FindInPageBarViewModel.PropertyChanged += ActiveFindInPageBarViewModel_PropertyChanged;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Register<SwitchViewMessage>(this,
            (r, m) => ActiveView = m.Page);

        Messenger.Register<FindInPageMessage>(this,
            (r, m) => FindInPage(m.Text));
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand]
    public static async Task DisclaimerAsync()
    {
        string disclaimer = await ResourceHelper.ReadAllTextAsync("免责声明.txt");
        HcMessageBox.Show(disclaimer, $"{App.AppName} - 免责声明");
    }

    [RelayCommand(CanExecute = nameof(CanFindInPage))]
    public void FindInPage(string? findText = null)
    {
        if (ActiveFindInPageBarViewModel is null)
        {
            return;
        }

        ActiveFindInPageBarViewModel.EnableFind();

        if (findText is null)
        {
            ActiveFindInPageBarViewModel.FindSelectedText();
        }
        else
        {
            ActiveFindInPageBarViewModel.Find(findText);
        }

        ActiveFindInPageBarViewModel.FocusOnFindInPageBar();
    }

    [RelayCommand]
    private void Closing(CancelEventArgs e)
    {
        if (FireflyViewModel.IsBusy)
        {
            var result = HcMessageBox.Ask("当前任务正在运行，确定要退出吗？", App.AppName);

            e.Cancel = result != MessageBoxResult.OK;
        }
        else if (FireflyViewModel.CanExport)
        {
            var result = HcMessageBox.Ask("确定要退出吗？", App.AppName);

            e.Cancel = result != MessageBoxResult.OK;
        }
    }

    [RelayCommand]
    private void DragDrop(bool enter)
    {
        Messenger.Send(new ValueChangedMessage<bool>(enter), "IsDragging");
    }

    [RelayCommand]
    private void DragEnter(DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            Messenger.Send(new ValueChangedMessage<bool>(true), "IsDragging");
        }
    }

    [RelayCommand]
    private async Task LoadedAsync()
    {
        Settings.Default.LastLaunchTime = DateTime.Now;

        if (Settings.Default.IsFirstLaunch || DateTime.Now - Settings.Default.LastLaunchTime > TimeSpan.FromDays(30))
        {
            Settings.Default.IsFirstLaunch = false;

            await _viewSwitcher.SwitchToAsync(ActiveView.Firefly);
            await DisclaimerAsync();
        }
    }

    [RelayCommand]
    private async Task ManualQueryCccfAsync()
    {
        ICccfRequest? request = null;

        if (Keyboard.FocusedElement is FindableTextBox findableTextBox && !String.IsNullOrWhiteSpace(findableTextBox.SelectedText))
        {
            request = new CccfSmartRequest(1, findableTextBox.SelectedText, findableTextBox.CccfFieldType);
        }
        else if (Keyboard.FocusedElement is TextBox textBox && !String.IsNullOrWhiteSpace(textBox.SelectedText))
        {
            request = new CccfSmartRequest(1, textBox.SelectedText, CccfFieldType.SmartMode);
        }

        if (ActiveView != ActiveView.CccfQuery)
        {
            await _viewSwitcher.SwitchToAsync(ActiveView.CccfQuery);
        }

        bool needsQuery = request is not null;

        if (needsQuery)
        {
            await await WeakReferenceMessenger.Default.Send(new ManualQueryAsyncRequestMessage(request!));
        }

        Messenger.Send(new FocusOnQueryBarMessage(!needsQuery, !needsQuery));
    }

    [RelayCommand]
    private void SwitchActiveView(ActiveView page)
    {
        ActiveView = page;
    }

    #endregion Commands

    #region Event Handlers

    private void ActiveFindInPageBarViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FindInPageBarViewModel.CanExecuteFindCommand))
        {
            OnPropertyChanged(nameof(CanFindInPage));
            FindInPageCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion Event Handlers
}
