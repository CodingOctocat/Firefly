using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firefly.Models.Messages;

namespace Firefly.ViewModels;

public partial class ShutdownViewModel : ObservableRecipient
{
    #region Fields

    public const int InitialCountdown = 60;

    #endregion Fields

    #region Properties

    [ObservableProperty]
    public partial int Countdown { get; set; } = InitialCountdown;

    #endregion Properties

    #region Commands

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task DelayShutdownAsync(CancellationToken cancellationToken = default)
    {
        for (int i = InitialCountdown; i > 0; i--)
        {
            Countdown = i;

            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Messenger.Send(new CancelShutdownMessage(), "ShutdownViewModel");

                return;
            }
        }

        Shutdown();
    }

    #endregion Commands

    #region Methods

    private static void Shutdown()
    {
        var psi = new ProcessStartInfo("shutdown", "/s /t 0") {
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process.Start(psi);
    }

    #endregion Methods
}
