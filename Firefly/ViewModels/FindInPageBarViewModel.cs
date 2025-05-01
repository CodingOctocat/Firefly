using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Services.Abstractions;

namespace Firefly.ViewModels;

public partial class FindInPageBarViewModel : ObservableRecipient, ISupportScrollFindableToCenterBehavior
{
    #region Fields

    private readonly Dictionary<int, int> _indexToItemIndexCache = [];

    #endregion Fields

    #region Properties

    public static string? SelectedText => Keyboard.FocusedElement is TextBox focused && !String.IsNullOrEmpty(focused.SelectedText) ? focused.SelectedText : null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnableFindCommand), nameof(FindCommand))]
    public partial bool CanExecuteFindCommand { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousFindResultCommand), nameof(GoToNextFindResultCommand))]
    public partial int FindCount { get; set; }

    /// <summary>
    /// 获取或设置当前高亮的结果索引。1-based。
    /// </summary>
    public int FindIndex
    {
        get => field;
        set
        {
            if (value > FindCount)
            {
                value = 1;
            }
            else if (value < 0)
            {
                value = FindCount;
            }

            if (SetProperty(ref field, value))
            {
                CenterScroller.ScrollToCenter(FindItemIndex, value);
                OnFindIndexChanged?.Invoke(value);
            }
        }
    }

    [ObservableProperty]
    public partial FindInPageOptions FindInPageOptions { get; set; } = new();

    /// <summary>
    /// 获取当前查找结果对应的数据项在容器中的索引。0-based。
    /// </summary>
    public int FindItemIndex => _indexToItemIndexCache.TryGetValue(FindIndex, out int itemIndex) ? itemIndex : -1;

    [ObservableProperty]
    public partial string FindText { get; set; } = "";

    public bool HasResult => FindCount > 0;

    [ObservableProperty]
    public partial bool IsFindEnabled { get; private set; }

    public Action<int>? OnFindIndexChanged { get; set; }

    #endregion Properties

    #region Constructors

    public FindInPageBarViewModel()
    {
        FindInPageOptions.PropertyChanged += FindInPageOptions_PropertyChanged;
    }

    #endregion Constructors

    #region Commands

    [RelayCommand]
    public void DisableFind()
    {
        IsFindEnabled = false;
        Reset();
    }

    [RelayCommand]
    public void EnableFind()
    {
        IsFindEnabled = true;
    }

    [RelayCommand]
    public void Find(string? findText = null)
    {
        if (findText is not null)
        {
            FindText = findText;
        }

        if (!IsFindEnabled || FindableScopes is null || String.IsNullOrEmpty(FindText))
        {
            Reset();

            return;
        }

        int resultIndex = 1;
        _indexToItemIndexCache.Clear();

        foreach (var item in FindableScopes)
        {
            InternalFind(item, FindText, ref resultIndex, FindInPageOptions);
        }

        FindCount = resultIndex - 1;

        foreach (var items in DataSource?.Index() ?? [])
        {
            foreach (var info in items.Item.GetFindableInfos())
            {
                info.ItemIndex = items.Index;
                _indexToItemIndexCache[info.Index] = items.Index;
            }
        }
    }

    [RelayCommand]
    public void FindSelectedText()
    {
        Find(SelectedText);

        if (Keyboard.FocusedElement is TextBox focused && focused.DataContext is IPageFindableInfo pageFindableValue)
        {
            FindIndex = pageFindableValue.Index;
        }
    }

    [RelayCommand]
    public void FocusOnFindInPageBar()
    {
        // 等待 PageFindBar 渲染后再发送消息，否则无法聚焦，也可以用 await Dispatcher.Yield()，但会产生异步传染
        Dispatcher.CurrentDispatcher.Invoke(() => Messenger.Send(new FocusOnFindInPageBarMessage(this)), DispatcherPriority.Render);
    }

    [RelayCommand(CanExecute = nameof(HasResult))]
    public void GoToNextFindResult()
    {
        FindIndex++;
    }

    [RelayCommand(CanExecute = nameof(HasResult))]
    public void GoToPreviousFindResult()
    {
        FindIndex--;
    }

    #endregion Commands

    #region OnPropertyChanged

    partial void OnFindableScopesChanged(IEnumerable<IPageFindable<IPageFindableInfo>>? value)
    {
        FindIndex = 0;
        Find();
    }

    partial void OnFindInPageOptionsChanged(FindInPageOptions oldValue, FindInPageOptions newValue)
    {
        oldValue.PropertyChanged -= FindInPageOptions_PropertyChanged;
        newValue.PropertyChanged += FindInPageOptions_PropertyChanged;
    }

    partial void OnFindTextChanged(string value)
    {
        FindIndex = 0;
        Find();
    }

    #endregion OnPropertyChanged

    #region Methods

    private static int InternalFind(IPageFindable<IPageFindableInfo> findable, string match, ref int resultIndex, FindInPageOptions options)
    {
        static bool IsMatch(string input, string match, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            if (String.IsNullOrWhiteSpace(input) || String.IsNullOrWhiteSpace(match))
            {
                return false;
            }

            var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

            string pattern;

            if (useRegex)
            {
                pattern = match;
            }
            else
            {
                pattern = Regex.Escape(match);

                if (matchWholeWord)
                {
                    pattern = $@"\b{pattern}\b";
                }
            }

            return Regex.IsMatch(input, pattern, options);
        }

        static int Match(IPageFindableInfo info, ref int index, string match, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            if (IsMatch(info.Text, match, matchCase, matchWholeWord, useRegex))
            {
                info.IsMatch = true;
                info.Index = index;
                index++;

                return 1;
            }

            info.IsMatch = false;
            info.Index = -1;

            return 0;
        }

        int count = 0;

        foreach (var info in findable.GetFindableInfos())
        {
            count += Match(
                info,
                ref resultIndex,
                match,
                options.MatchCase,
                options.MatchWholeWord,
                options.UseRegex);
        }

        return count;
    }

    private void Reset()
    {
        FindIndex = 0;

        if (FindableScopes is null)
        {
            FindCount = 0;

            return;
        }

        foreach (var items in FindableScopes)
        {
            foreach (var item in items.GetFindableInfos())
            {
                item.IsMatch = false;
                item.Index = -1;
            }
        }

        FindCount = 0;
    }

    #endregion Methods

    #region Event Handlers

    private void FindInPageOptions_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Find();
    }

    #endregion Event Handlers

    #region ISupportCenterScrollToFindableBehavior

    public IScrollFindableToCenter CenterScroller { get; set; } = null!;

    public IEnumerable<IPageFindable<IPageFindableInfo>>? DataSource { get; set; }

    [ObservableProperty]
    public partial IEnumerable<IPageFindable<IPageFindableInfo>>? FindableScopes { get; set; }

    #endregion ISupportCenterScrollToFindableBehavior
}
