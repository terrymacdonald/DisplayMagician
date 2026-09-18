using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

internal static class Program
{
    private static async Task Main()
    {
        // The current WinForms application still owns desktop work. This executable becomes its interactive-session host
        // once the existing profile and shortcut lifecycle is moved behind the Agent boundary.
        AgentRegistration registration = AgentIdentity.CreateRegistration("4.0.0-development", "manual");
        ControlServiceClient serviceClient = new ControlServiceClient();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(10000);
        await serviceClient.RegisterAsync(registration, cancellationTokenSource.Token).ConfigureAwait(false);
    }
}
