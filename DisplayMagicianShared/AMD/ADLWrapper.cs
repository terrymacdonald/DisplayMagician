using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ATI.ADL;
using Microsoft.Win32.SafeHandles;
using DisplayMagicianShared;

namespace DisplayMagicianShared.AMD
{
    public class ADLWrapper : IDisposable
    {
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static ADLWrapper _instance = new ADLWrapper();

        private bool _initialised = false;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        

        static ADLWrapper() { }
        public ADLWrapper()
        {
            int ADLRet = ADL.ADL_FAIL;

            SharedLogger.logger.Trace("ADLWrapper/ADLWrapper: Intialising ADL library");
            try
            {
                if (ADL.ADL_Main_Control_Create != null)
                {
                    // Second parameter is 1: Get only the present adapters
                    ADLRet = ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);
                }

                if (ADLRet == ADL.ADL_SUCCESS)
                {
                    _initialised = true;
                    SharedLogger.logger.Trace("ADLWrapper/ADLWrapper: ADL library was initialised successfully");
                }
                else
                {
                    SharedLogger.logger.Error("ADLWrapper/ADLWrapper: Error intialising ADL library. ADL_Main_Control_Create() returned error code " + ADLRet.ToString());
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error("ADLWrapper/ADLWrapper: Exception intialising ADL library. ADL_Main_Control_Create() caused an exception");
            }
            
        }

        ~ADLWrapper()
        {
            // If the ADL library was initialised, then we need to free it up.
            if (_initialised)
            {
                if (null != ADL.ADL_Main_Control_Destroy)
                    ADL.ADL_Main_Control_Destroy();
            }            
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose() => Dispose(true);

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (null != ADL.ADL_Main_Control_Destroy)
                    ADL.ADL_Main_Control_Destroy();

                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            _disposed = true;
        }


        public bool IsInstalled
        {
            get { return _initialised; }
        }

        public static ADLWrapper GetLibrary()
        {
            return _instance;
        }

        public List<string> GenerateDisplayProfilesIdentifiers()
        {
            SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_FAIL;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            if (null != ADL.ADL_Adapter_NumberOfAdapters_Get)
            {
                ADL.ADL_Adapter_NumberOfAdapters_Get(ref NumberOfAdapters);
                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Number Of Adapters: {NumberOfAdapters.ToString()} ");
            }

            if (NumberOfAdapters > 0)
            {
                // Get OS adpater info from ADL
                ADLAdapterInfoArray OSAdapterInfoData;
                OSAdapterInfoData = new ADLAdapterInfoArray();

                if (ADL.ADL_Adapter_AdapterInfo_Get != null)
                {
                    IntPtr AdapterBuffer = IntPtr.Zero;
                    int size = Marshal.SizeOf(OSAdapterInfoData);
                    AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                    Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                    if (null != ADL.ADL_Adapter_AdapterInfo_Get)
                    {
                        ADLRet = ADL.ADL_Adapter_AdapterInfo_Get(AdapterBuffer, size);
                        if (ADLRet == ADL.ADL_SUCCESS)
                        {
                            OSAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                            int IsActive = 1; // We only want to search for active adapters

                            for (int i = 0; i < NumberOfAdapters; i++)
                            {
                                // Check if the adapter is active
                                if (null != ADL.ADL_Adapter_Active_Get)
                                    ADLRet = ADL.ADL_Adapter_Active_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref IsActive);

                                if (ADL.ADL_SUCCESS == ADLRet)
                                {
                                    Console.WriteLine("Adapter is   : " + (0 == IsActive ? "DISABLED" : "ENABLED"));
                                    Console.WriteLine("Adapter Index: " + OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex.ToString());
                                    Console.WriteLine("Adapter UDID : " + OSAdapterInfoData.ADLAdapterInfo[i].UDID);
                                    Console.WriteLine("Bus No       : " + OSAdapterInfoData.ADLAdapterInfo[i].BusNumber.ToString());
                                    Console.WriteLine("Driver No    : " + OSAdapterInfoData.ADLAdapterInfo[i].DriverNumber.ToString());
                                    Console.WriteLine("Function No  : " + OSAdapterInfoData.ADLAdapterInfo[i].FunctionNumber.ToString());
                                    Console.WriteLine("Vendor ID    : " + OSAdapterInfoData.ADLAdapterInfo[i].VendorID.ToString());
                                    Console.WriteLine("Adapter Name : " + OSAdapterInfoData.ADLAdapterInfo[i].AdapterName);
                                    Console.WriteLine("Display Name : " + OSAdapterInfoData.ADLAdapterInfo[i].DisplayName);
                                    Console.WriteLine("Present      : " + (0 == OSAdapterInfoData.ADLAdapterInfo[i].Present ? "No" : "Yes"));
                                    Console.WriteLine("Exist        : " + (0 == OSAdapterInfoData.ADLAdapterInfo[i].Exist ? "No" : "Yes"));
                                    Console.WriteLine("Driver Path  : " + OSAdapterInfoData.ADLAdapterInfo[i].DriverPath);
                                    Console.WriteLine("Driver Path X: " + OSAdapterInfoData.ADLAdapterInfo[i].DriverPathExt);
                                    Console.WriteLine("PNP String   : " + OSAdapterInfoData.ADLAdapterInfo[i].PNPString);

                                    // Obtain information about displays
                                    ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                    if (null != ADL.ADL_Display_DisplayInfo_Get)
                                    {
                                        IntPtr DisplayBuffer = IntPtr.Zero;
                                        int j = 0;

                                        // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                        ADLRet = ADL.ADL_Display_DisplayInfo_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 0);
                                        if (ADL.ADL_SUCCESS == ADLRet)
                                        {

                                            List<ADLDisplayInfo> DisplayInfoData = new List<ADLDisplayInfo>();

                                            try
                                            {

                                                for (j = 0; j < NumberOfDisplays; j++)
                                                {
                                                    // NOTE: the ToInt64 work on 64 bit, need to change to ToInt32 for 32 bit OS
                                                    oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(DisplayBuffer.ToInt64() + (j * Marshal.SizeOf(oneDisplayInfo))), oneDisplayInfo.GetType());
                                                    DisplayInfoData.Add(oneDisplayInfo);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Exception caused trying to access attached displays");
                                                continue;
                                            }
                                            Console.WriteLine("\nTotal Number of Displays supported: " + NumberOfDisplays.ToString());
                                            Console.WriteLine("\nDispID  AdpID  Type OutType  CnctType Connected  Mapped  InfoValue DisplayName ");

                                            for (j = 0; j < NumberOfDisplays; j++)
                                            {
                                                int InfoValue = DisplayInfoData[j].DisplayInfoValue;
                                                string StrConnected = (1 == (InfoValue & 1)) ? "Yes" : "No ";
                                                string StrMapped = (2 == (InfoValue & 2)) ? "Yes" : "No ";
                                                int AdpID = DisplayInfoData[j].DisplayID.DisplayLogicalAdapterIndex;
                                                string StrAdpID = (AdpID < 0) ? "--" : AdpID.ToString("d2");

                                                Console.WriteLine(DisplayInfoData[j].DisplayID.DisplayLogicalIndex.ToString() + "        " +
                                                                        StrAdpID + "      " +
                                                                        DisplayInfoData[j].DisplayType.ToString() + "      " +
                                                                        DisplayInfoData[j].DisplayOutputType.ToString() + "      " +
                                                                        DisplayInfoData[j].DisplayConnector.ToString() + "        " +
                                                                        StrConnected + "        " +
                                                                        StrMapped + "      " +
                                                                        InfoValue.ToString("x4") + "   " +
                                                                        DisplayInfoData[j].DisplayName.ToString());
                                            }
                                            Console.WriteLine();
                                        }
                                        else
                                        {
                                            Console.WriteLine("ADL_Display_DisplayInfo_Get() returned error code " + ADLRet.ToString());
                                        }
                                        // Release the memory for the DisplayInfo structure
                                        if (IntPtr.Zero != DisplayBuffer)
                                            Marshal.FreeCoTaskMem(DisplayBuffer);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("ADL_Adapter_AdapterInfo_Get() returned error code " + ADLRet.ToString());
                        }
                    }
                    // Release the memory for the AdapterInfo structure
                    if (IntPtr.Zero != AdapterBuffer)
                        Marshal.FreeCoTaskMem(AdapterBuffer);
                }
            }
            else
            {
                SharedLogger.logger.Error($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }


            /*foreach (NvAPIWrapper.GPU.PhysicalGPU myPhysicalGPU in myPhysicalGPUs)
            {
                // get a list of all physical outputs attached to the GPUs
                NvAPIWrapper.GPU.GPUOutput[] myGPUOutputs = myPhysicalGPU.ActiveOutputs;
                foreach (NvAPIWrapper.GPU.GPUOutput aGPUOutput in myGPUOutputs)
                {
                    SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: We were able to detect {myGPUOutputs.Length} outputs");
                    // Figure out the displaydevice attached to the output
                    NvAPIWrapper.Display.DisplayDevice aConnectedDisplayDevice = myPhysicalGPU.GetDisplayDeviceByOutput(aGPUOutput);

                    // Create an array of all the important display info we need to record
                    List<string> displayInfo = new List<string>();
                    displayInfo.Add("NVIDIA");
                    try
                    {
                        displayInfo.Add(myPhysicalGPU.ArchitectInformation.ShortName.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Architecture ShortName from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(myPhysicalGPU.ArchitectInformation.Revision.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Architecture Revision from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(myPhysicalGPU.Board.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Board details from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(myPhysicalGPU.Foundry.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Foundry from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(myPhysicalGPU.GPUId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA GPUId from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(myPhysicalGPU.GPUType.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA GPUType from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(aConnectedDisplayDevice.ConnectionType.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Connection from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(aConnectedDisplayDevice.DisplayId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA DisplayID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }

                    // Create a display identifier out of it
                    string displayIdentifier = String.Join("|", displayInfo);
                    // Add it to the list of display identifiers so we can return it
                    displayIdentifiers.Add(displayIdentifier);

                    SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                }

            }*/

            return null;
        }
    }
}
