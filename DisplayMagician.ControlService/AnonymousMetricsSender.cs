using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class AnonymousMetricsSender
{
    private static readonly Uri HeartbeatUri = new Uri("https://www.displaymagician.com/metrics/v1/heartbeat");
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _httpClient;
    private readonly MachineScheduleCoordinator _machineScheduleCoordinator;

    public AnonymousMetricsSender(MachineScheduleCoordinator machineScheduleCoordinator)
        : this(new HttpClient { Timeout = TimeSpan.FromSeconds(15) }, machineScheduleCoordinator)
    {
    }

    public AnonymousMetricsSender(HttpClient httpClient, MachineScheduleCoordinator machineScheduleCoordinator)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _machineScheduleCoordinator = machineScheduleCoordinator ?? throw new ArgumentNullException(nameof(machineScheduleCoordinator));
    }

    public async Task TrySendAsync(CancellationToken cancellationToken)
    {
        MachineScheduleState state = _machineScheduleCoordinator.GetState();
        if (!state.ShareAnonymousUsageMetrics || string.IsNullOrWhiteSpace(state.CurrentAppVersion) || !_machineScheduleCoordinator.IsMetricsHeartbeatDue(DateTime.UtcNow, state.CurrentAppVersion))
        {
            return;
        }

        var payload = new
        {
            schemaVersion = 1,
            installId = state.InstallId,
            appVersion = state.CurrentAppVersion,
            updateChannel = state.UpdateChannel,
            launches = state.TotalAnonymousMetricLaunches,
            activeMinutes = state.TotalAnonymousMetricActiveMinutes,
            graphicsLibrary = GetGraphicsLibrary(),
            connectedScreenCount = GetConnectedScreenCount(),
            windowsBuild = GetWindowsBuild()
        };

        try
        {
            using StringContent content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _httpClient.PostAsync(HeartbeatUri, content, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                _machineScheduleCoordinator.RecordMetricsHeartbeatSuccess(DateTime.UtcNow, state.CurrentAppVersion);
                return;
            }

            Logger.Warn("AnonymousMetricsSender/TrySendAsync: Heartbeat was not accepted (statusCode={0}).", (int)response.StatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is InvalidOperationException)
        {
            Logger.Warn(ex, "AnonymousMetricsSender/TrySendAsync: Anonymous metrics heartbeat failed.");
        }
    }

    private static string GetWindowsBuild()
    {
        Version version = Environment.OSVersion.Version;
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private static string GetGraphicsLibrary()
    {
        try
        {
            for (uint index = 0; ; index++)
            {
                DisplayDevice displayDevice = new DisplayDevice();
                displayDevice.cb = Marshal.SizeOf<DisplayDevice>();
                if (!EnumDisplayDevices(null, index, ref displayDevice, 0))
                {
                    break;
                }

                string deviceId = displayDevice.DeviceID ?? string.Empty;
                string deviceName = displayDevice.DeviceString ?? string.Empty;
                if (deviceId.Contains("VEN_10DE", StringComparison.OrdinalIgnoreCase) || deviceName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) return "nvidia";
                if (deviceId.Contains("VEN_1002", StringComparison.OrdinalIgnoreCase) || deviceName.Contains("AMD", StringComparison.OrdinalIgnoreCase) || deviceName.Contains("ATI", StringComparison.OrdinalIgnoreCase)) return "amd";
                if (deviceId.Contains("VEN_8086", StringComparison.OrdinalIgnoreCase) || deviceName.Contains("INTEL", StringComparison.OrdinalIgnoreCase)) return "intel";
            }
        }
        catch (Exception ex) when (ex is ExternalException || ex is InvalidOperationException)
        {
            Logger.Debug(ex, "AnonymousMetricsSender/GetGraphicsLibrary: Could not enumerate display adapters.");
        }

        return "unknown";
    }

    private static int GetConnectedScreenCount()
    {
        int count = 0;
        try
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (_, _, _, _) =>
            {
                count++;
                return count < 16;
            }, IntPtr.Zero);
        }
        catch (Exception ex) when (ex is ExternalException || ex is InvalidOperationException)
        {
            Logger.Debug(ex, "AnonymousMetricsSender/GetConnectedScreenCount: Could not enumerate monitors.");
        }

        return count;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(string? deviceName, uint deviceNumber, ref DisplayDevice displayDevice, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(IntPtr deviceContext, IntPtr clippingRectangle, MonitorEnumProc callback, IntPtr data);

    private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr deviceContext, IntPtr rectangle, IntPtr data);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
    }
}
