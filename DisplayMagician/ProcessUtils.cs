using System;
using System.Collections.Generic;
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

    }       
}
