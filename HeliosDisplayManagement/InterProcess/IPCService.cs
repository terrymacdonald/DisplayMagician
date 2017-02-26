using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows.Forms;

namespace HeliosDisplayManagement.InterProcess
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class IPCService : IService
    {
        private static ServiceHost _serviceHost;

        private IPCService()
        {
            Status = InstanceStatus.Busy;
        }

        public InstanceStatus Status { get; set; }
        public int HoldProcessId { get; set; } = 0;

        public void StopHold()
        {
            Application.Exit();
        }

        public static bool StartService()
        {
            if (_serviceHost == null)
                try
                {
                    var process = Process.GetCurrentProcess();
                    var service = new IPCService();
                    _serviceHost = new ServiceHost(
                        service,
                        new Uri($"net.pipe://localhost/HeliosDisplayManagement_IPC{process.Id}"));

                    _serviceHost.AddServiceEndpoint(typeof(IService), new NetNamedPipeBinding(), "Service");
                    _serviceHost.Open();
                    return true;
                }
                catch (Exception)
                {
                    try
                    {
                        _serviceHost?.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                    _serviceHost = null;
                }
            return false;
        }

        public static IPCService GetInstance()
        {
            if ((_serviceHost != null) || StartService())
                return _serviceHost?.SingletonInstance as IPCService;
            return null;
        }
    }
}