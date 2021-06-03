using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician
{

    class ProcessInfo : IComparable<ProcessInfo>
    {
        public Process TheProcess;
        public ProcessInfo Parent;
        public List<ProcessInfo> Children = new List<ProcessInfo>();

        public ProcessInfo(Process the_process)
        {
            TheProcess = the_process;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]",
                TheProcess.ProcessName, TheProcess.Id);
        }

        public int CompareTo(ProcessInfo other)
        {
            return TheProcess.ProcessName.CompareTo(
                other.TheProcess.ProcessName);
        }
    }

    static class ProcessUtils
    {
        private static Dictionary<int, ProcessInfo> allProcessInfosDict;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Initialise()
        {
            allProcessInfosDict = new Dictionary<int, ProcessInfo>();

            try
            {
                // Get the parent/child info.
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                   "SELECT ProcessId, ParentProcessId FROM Win32_Process");
                ManagementObjectCollection collection = searcher.Get();

                // Get the processes.
                foreach (Process process in Process.GetProcesses())
                {
                    allProcessInfosDict.Add(process.Id, new ProcessInfo(process));
                }

                // Create the child lists.
                foreach (var item in collection)
                {
                    // Find the parent and child in the dictionary.
                    int child_id = Convert.ToInt32(item["ProcessId"]);
                    int parent_id = Convert.ToInt32(item["ParentProcessId"]);

                    ProcessInfo child_info = null;
                    ProcessInfo parent_info = null;
                    if (allProcessInfosDict.ContainsKey(child_id))
                        child_info = allProcessInfosDict[child_id];
                    if (allProcessInfosDict.ContainsKey(parent_id))
                        parent_info = allProcessInfosDict[parent_id];

                    if (child_info == null)
                        Console.WriteLine(
                            "Cannot find child " + child_id.ToString() +
                            " for parent " + parent_id.ToString());

                    if (parent_info == null)
                        Console.WriteLine(
                            "Cannot find parent " + parent_id.ToString() +
                            " for child " + child_id.ToString());

                    if ((child_info != null) && (parent_info != null))
                    {
                        parent_info.Children.Add(child_info);
                        child_info.Parent = parent_info;
                    }
                }

            }
            catch(Exception ex)
            {
                logger.Error(ex,$"ProcessUtils/Initialise: Exception (re)initialising the process information to figure out process hierarchy");
            }

        }

        public static List<Process> FindChildProcesses(Process parentProcess)
        {
            List<Process> childProcesses = new List<Process>() { };

            try
            {
                int parentId = parentProcess.Id;
                // TODO: We *possibly* could walk the tree to find the program hierarchy, to get the full list of the 
                // process tree, but this seems like the best way at this stage. I'm expecting I'll find an edge case in the future
                // that requires some sort of modification, but this is working well in 2 days of testing so far!
                if (allProcessInfosDict.ContainsKey(parentId))
                {
                    foreach (ProcessInfo childProcess in allProcessInfosDict[parentId].Children)
                    {
                        childProcesses.Add(childProcess.TheProcess);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ProcessUtils/FindChildProcesses: Exception finding the child processes of the parentProcess");
            }
            
            return childProcesses;
        }

        public static bool StopProcess(Process processToStop)
        {
            try
            {
                // Stop the process
                processToStop.CloseMainWindow();
                if (!processToStop.WaitForExit(5000))
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} wouldn't stop cleanly. Forcing program close.");
                    processToStop.Kill();
                    if (!processToStop.WaitForExit(5000))
                    {
                        logger.Error($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                        return false;
                    }
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} was successfully killed.");
                }
                processToStop.Close();
                return true;
            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex, $"ProcessUtils/StopProcess: Win32Exception! Couldn't access the wait status for a named process we're trying to stop. So now just killing the process.");
                processToStop.Kill();
                if (!processToStop.WaitForExit(5000))
                {
                    logger.Error($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                    return false;
                }
                logger.Trace($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} was successfully killed.");
                processToStop.Close();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't kill the named process as the process appears to have closed already.");
            }
            catch (SystemException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't WaitForExit the named process as there is no process associated with the Process object (or cannot get the ID from the named process handle).");
            }

            catch (AggregateException ae)
            {
                logger.Error(ae, $"ProcessUtils/StopProcess: Got an AggregateException.");
            }
            return false;
        }
    }       
}
