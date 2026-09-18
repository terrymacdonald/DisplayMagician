using System;
using System.Threading;
using System.Threading.Tasks;

namespace DisplayMagician.ControlService;

internal static class Program
{
    private static async Task Main()
    {
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        NamedPipeControlServer pipeServer = new NamedPipeControlServer(coordinator);
        await pipeServer.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }
}
