using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Services.Abstractions;

namespace Firefly.Services;

public class ViewSwitcher : IViewSwitcher
{
    private ActiveView? _currentView;

    private TaskCompletionSource<bool>? _pendingTcs;

    private ActiveView? _targetView;

    public void NotifyRenderCompleted(ActiveView renderedView)
    {
        if (_targetView == renderedView)
        {
            _pendingTcs?.TrySetResult(true);
            _pendingTcs = null;
        }

        _currentView = renderedView;
    }

    public async ValueTask SwitchToAsync(ActiveView targetPage)
    {
        _targetView = targetPage;

        if (_currentView == targetPage && _pendingTcs is null)
        {
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        _pendingTcs = tcs;

        WeakReferenceMessenger.Default.Send(new SwitchViewMessage(targetPage));

        await tcs.Task;
    }
}
