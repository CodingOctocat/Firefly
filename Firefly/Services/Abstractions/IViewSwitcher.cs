using System.Threading.Tasks;

using Firefly.Models;

namespace Firefly.Services.Abstractions;

public interface IViewSwitcher
{
    void NotifyRenderCompleted(ActiveView renderedView);

    ValueTask SwitchToAsync(ActiveView targetView);
}
