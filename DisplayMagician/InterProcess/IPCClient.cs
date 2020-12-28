using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace DisplayMagician.InterProcess
{
    internal class IPCClient : ClientBase<IService>, IService
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IPCClient(Process process)
            : base(
                new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IService)),
                    new NetNamedPipeBinding(),
                    new EndpointAddress(
                        $"net.pipe://localhost/DisplayMagician_IPC{process.Id}/Service")))
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
                catch (Exception ex)
                {
                    Console.WriteLine($"IPCClient/QueryAll exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
                    IPCClient processChannel = null;
                    try
                    {
                        processChannel = new IPCClient(process);

                        if (processChannel.Status == status)
                        {
                            return processChannel;
                        }
                    }
                    catch (Exception ex)
                    {
                        processChannel = null;
                        logger.Error(ex, $"IPCClient/QueryByStatus: Couldn't create an IPC Client");
                        Console.WriteLine($"IPCClient/QueryByStatus exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                        // ignored
                    }
                }
            }

            return null;
        }
    }
}