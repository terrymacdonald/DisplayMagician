using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace HeliosPlus.InterProcess
{
    internal class IPCClient : ClientBase<IService>, IService
    {
        public IPCClient(Process process)
            : base(
                new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IService)),
                    new NetNamedPipeBinding(),
                    new EndpointAddress(
                        $"net.pipe://localhost/HeliosPlus_IPC{process.Id}/Service")))
        {
        }

        public int HoldProcessId
        {
            get => Channel.HoldProcessId;
        }

        public InstanceStatus Status
        {
            get => Channel.Status;
        }

        public void StopHold()
        {
            Channel.StopHold();
        }

        public static IEnumerable<IPCClient> QueryAll()
        {
            var thisProcess = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(thisProcess.ProcessName))
            {
                if (process.Id == thisProcess.Id)
                {
                    continue;
                }

                IPCClient processChannel = null;

                try
                {
                    processChannel = new IPCClient(process);
                }
                catch
                {
                    // ignored
                }

                if (processChannel != null)
                {
                    yield return processChannel;
                }
            }
        }

        public static IPCClient QueryByStatus(InstanceStatus status)
        {
            var thisProcess = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(thisProcess.ProcessName))
            {
                if (process.Id != thisProcess.Id)
                {
                    try
                    {
                        var processChannel = new IPCClient(process);

                        if (processChannel.Status == status)
                        {
                            return processChannel;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return null;
        }
    }
}