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

        public List<string> GenerateProfileDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_FAIL;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            List<string> displayIdentifiers = new List<string>();

            // Keep a list of things we want to track
            List<ADLDisplayMap> allDisplayMaps = new List<ADLDisplayMap>();
            List<ADLDisplayTarget> allDisplayTargets = new List<ADLDisplayTarget>();

            if (ADL.ADL_Display_DisplayMapConfig_Get != null)
            {
                IntPtr DisplayMapBuffer = IntPtr.Zero;
                IntPtr DisplayTargetBuffer = IntPtr.Zero;
                int numDisplayMaps = 0;
                int numDisplayTargets = 0;                         

                // Get the DisplayMap info for all adapters on the machine in one go
                ADLRet = ADL.ADL_Display_DisplayMapConfig_Get(-1, out numDisplayMaps, out DisplayMapBuffer, out numDisplayTargets, out DisplayTargetBuffer, 0);
                if (ADLRet == ADL.ADL_SUCCESS)
                {
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Number Of DisplayMaps: {numDisplayMaps.ToString()} ");
                    
                    // Marshal the Display Maps
                    ADLDisplayMap oneDisplayMap = new ADLDisplayMap();                    

                    for (int displayMapNum = 0; displayMapNum < numDisplayMaps; displayMapNum++)
                    {
                        // NOTE: the ToInt64 work on 64 bit, need to change to ToInt32 for 32 bit OS
                        oneDisplayMap = (ADLDisplayMap)Marshal.PtrToStructure(new IntPtr(DisplayMapBuffer.ToInt64() + (displayMapNum * Marshal.SizeOf(oneDisplayMap))), oneDisplayMap.GetType());
                        allDisplayMaps.Add(oneDisplayMap);
                    }

                    //Marshall the DisplayTargets
                    ADLDisplayTarget oneDisplayTarget = new ADLDisplayTarget();                  

                    for (int displayTargetNum = 0; displayTargetNum < numDisplayTargets; displayTargetNum++)
                    {
                        // NOTE: the ToInt64 work on 64 bit, need to change to ToInt32 for 32 bit OS
                        oneDisplayTarget = (ADLDisplayTarget)Marshal.PtrToStructure(new IntPtr(DisplayMapBuffer.ToInt64() + (displayTargetNum * Marshal.SizeOf(oneDisplayTarget))), oneDisplayTarget.GetType());
                        allDisplayTargets.Add(oneDisplayTarget);
                    }

                }
            }

            if (ADL.ADL_Display_DisplayMapConfig_PossibleAddAndRemove != null)
            {
                int numDisplayMap = 1;
                int numDisplayTarget = 1;

                IntPtr PossibleAddTargetBuffer = IntPtr.Zero;
                IntPtr PossibleRemoveTargetBuffer = IntPtr.Zero;
                int numPossibleAddTargets = 0;
                int numPossibleRemoveTargets = 0;

                List<ADLDisplayTarget> allPossibleAddDisplayTargets = new List<ADLDisplayTarget>();
                List<ADLDisplayTarget> allPossibleRemoveDisplayTargets = new List<ADLDisplayTarget>();

                // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                ADLRet = ADL.ADL_Display_DisplayMapConfig_PossibleAddAndRemove(0, numDisplayMap, allDisplayMaps[0], numDisplayTarget, allDisplayTargets[0], out numPossibleAddTargets, out PossibleAddTargetBuffer, out numPossibleRemoveTargets, out PossibleRemoveTargetBuffer);
                if (ADLRet == ADL.ADL_SUCCESS)
                {
                    // Marshal the Possible Add Targets
                    ADLDisplayTarget oneDisplayTarget = new ADLDisplayTarget();

                    for (int displayTargetNum = 0; displayTargetNum < numPossibleAddTargets; displayTargetNum++)
                    {
                        // NOTE: the ToInt64 work on 64 bit, need to change to ToInt32 for 32 bit OS
                        oneDisplayTarget = (ADLDisplayTarget)Marshal.PtrToStructure(new IntPtr(PossibleAddTargetBuffer.ToInt64() + (displayTargetNum * Marshal.SizeOf(oneDisplayTarget))), oneDisplayTarget.GetType());
                        allPossibleAddDisplayTargets.Add(oneDisplayTarget);
                    }

                    // Marshal the Possible Remove Targets
                    // oneDisplayTarget = new ADLDisplayTarget();

                    for (int displayTargetNum = 0; displayTargetNum < numPossibleRemoveTargets; displayTargetNum++)
                    {
                        // NOTE: the ToInt64 work on 64 bit, need to change to ToInt32 for 32 bit OS
                        oneDisplayTarget = (ADLDisplayTarget)Marshal.PtrToStructure(new IntPtr(PossibleRemoveTargetBuffer.ToInt64() + (displayTargetNum * Marshal.SizeOf(oneDisplayTarget))), oneDisplayTarget.GetType());
                        allPossibleAddDisplayTargets.Add(oneDisplayTarget);
                    }
                }
            }


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

                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL_Adapter_AdapterInfo_Get != null)
                {
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ADL_Adapter_AdapterInfo_Get DLL function exists.");
                    // Figure out the size of the AdapterBuffer we need to build                    
                    int size = Marshal.SizeOf(OSAdapterInfoData);
                    AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                    Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Running ADL_Adapter_AdapterInfo_Get to find all known AMD adapters.");
                    ADLRet = ADL.ADL_Adapter_AdapterInfo_Get(AdapterBuffer, size);
                    if (ADLRet == ADL.ADL_SUCCESS)
                    {
                        // Use the AdapterBuffer pointer to marshal the OS Adapter Info into a structure
                        OSAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Successfully run ADL_Adapter_AdapterInfo_Get to find information about all known AMD adapters.");

                        // Go through each adapter
                        for (int i = 0; i < NumberOfAdapters; i++)
                        {
                            // Check if the adapter is active
                            if (ADL.ADL_Adapter_Active_Get != null)
                                ADLRet = ADL.ADL_Adapter_Active_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref IsActive);

                            if (ADLRet == ADL.ADL_SUCCESS)
                            {
                                // Only continue if the adapter is enabled
                                if (IsActive != ADL.ADL_TRUE)
                                {
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} isn't active ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}).");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} is active! ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}).");

                                // Get the unique identifier from the Adapter
                                int AdapterID = 0;
                                if (ADL.ADL_Adapter_ID_Get != null)
                                {
                                    ADLRet = ADL.ADL_Adapter_ID_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref AdapterID);
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}) AdapterID is {AdapterID.ToString()}");
                                }

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL_AdapterX2_Caps(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, out AdapterCapabilities);
                                }

                                //ADLAdapterCapsX2 AdapterCapabilities = (ADLAdapterCapsX2)Marshal.PtrToStructure(AdapterCapabilitiesBuffer, typeof(ADLAdapterCapsX2));
                                Console.Write(AdapterCapabilities.AdapterID);

                                // Obtain information about displays
                                ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                if (ADL.ADL_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;
                                    int j = 0;

                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL_Display_DisplayInfo_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 0);
                                    if (ADLRet == ADL.ADL_SUCCESS)
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
                                            // Skip non connected displays
                                            if ((DisplayInfoData[j].DisplayInfoValue & 1) != 1)
                                            {
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}) AdapterID display ID#{j} is not connected");
                                                continue;
                                            }

                                            // Skip connected but non-mapped displays (not mapped in windows) - wae want all displays currently visible in the OS
                                            if ((DisplayInfoData[j].DisplayInfoValue & 2) != 2)
                                            {
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}) AdapterID display ID#{j} is not connected");
                                                continue;
                                            }

                                            /*ADLDisplayConfig DisplayConfig = new ADLDisplayConfig();
                                            if (ADL.ADL_Display_DeviceConfig_Get != null)
                                            {
                                                // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                                ADLRet = ADL.ADL_Display_DeviceConfig_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, DisplayInfoData[j].DisplayID.DisplayLogicalIndex, out DisplayConfig);
                                                if (ADLRet == ADL.ADL_SUCCESS)
                                                {

                                                }
                                            }*/

                                            // Create an array of all the important display info we need to record
                                            List<string> displayInfoIdentifierSection = new List<string>();
                                            displayInfoIdentifierSection.Add("AMD");
                                            try
                                            {
                                                displayInfoIdentifierSection.Add(OSAdapterInfoData.ADLAdapterInfo[i].AdapterName);
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Index from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(OSAdapterInfoData.ADLAdapterInfo[i].VendorID.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("1002");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(AdapterID.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                ADL.ADLConnectionType connector = (ADL.ADLConnectionType)DisplayInfoData[j].DisplayOutputType;
                                                displayInfoIdentifierSection.Add(connector.ToString("G"));
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(DisplayInfoData[j].DisplayManufacturerName);
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Display manufacturer from AMD video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(DisplayInfoData[j].DisplayName);
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Display Name from AMD video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            // Create a display identifier out of it
                                            string displayIdentifier = String.Join("|", displayInfoIdentifierSection);
                                            // Add it to the list of display identifiers so we can return it
                                            displayIdentifiers.Add(displayIdentifier);

                                            SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");

                                        }
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
                            else
                            {
                                Console.WriteLine("ADL_Adapter_Active_Get() returned error code " + ADLRet.ToString());
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
                {
                    Marshal.FreeCoTaskMem(AdapterBuffer);
                }

                // Return all the identifiers we've found
                return displayIdentifiers;
            }
            else
            {
                SharedLogger.logger.Error($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }

        public List<string> GenerateAllAvailableDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_FAIL;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            List<string> displayIdentifiers = new List<string>();


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

                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL_Adapter_AdapterInfo_Get != null)
                {
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ADL_Adapter_AdapterInfo_Get DLL function exists.");
                    // Figure out the size of the AdapterBuffer we need to build                    
                    int size = Marshal.SizeOf(OSAdapterInfoData);
                    AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                    Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Running ADL_Adapter_AdapterInfo_Get to find all known AMD adapters.");
                    ADLRet = ADL.ADL_Adapter_AdapterInfo_Get(AdapterBuffer, size);
                    if (ADLRet == ADL.ADL_SUCCESS)
                    {
                        // Use the AdapterBuffer pointer to marshal the OS Adapter Info into a structure
                        OSAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Successfully run ADL_Adapter_AdapterInfo_Get to find information about all known AMD adapters.");

                        // Go through each adapter
                        for (int i = 0; i < NumberOfAdapters; i++)
                        {
                            // Check if the adapter is active
                            if (ADL.ADL_Adapter_Active_Get != null)
                                ADLRet = ADL.ADL_Adapter_Active_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref IsActive);

                            if (ADLRet == ADL.ADL_SUCCESS)
                            {
                                // Only continue if the adapter is enabled
                                if (IsActive != ADL.ADL_TRUE)
                                {
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} isn't active ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}).");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} is active! ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}).");

                                // Get the unique identifier from the Adapter
                                int AdapterID = 0;
                                if (ADL.ADL_Adapter_ID_Get != null)
                                {
                                    ADLRet = ADL.ADL_Adapter_ID_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref AdapterID);
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}) AdapterID is {AdapterID.ToString()}");
                                }

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL_AdapterX2_Caps(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, out AdapterCapabilities);
                                }

                                //ADLAdapterCapsX2 AdapterCapabilities = (ADLAdapterCapsX2)Marshal.PtrToStructure(AdapterCapabilitiesBuffer, typeof(ADLAdapterCapsX2));
                                Console.Write(AdapterCapabilities.AdapterID);

                                // Obtain information about displays
                                ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                if (ADL.ADL_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;
                                    int j = 0;

                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL_Display_DisplayInfo_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 0);
                                    if (ADLRet == ADL.ADL_SUCCESS)
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
                                            // Skip non connected displays - we want only connected displays that could potentially be used now (thats both mapped and non-mapped displays)
                                            if ((DisplayInfoData[j].DisplayInfoValue & 1) != 1)
                                            {
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{i} ({OSAdapterInfoData.ADLAdapterInfo[i].AdapterName}) AdapterID display ID#{j} is not connected");
                                                continue;
                                            }

                                            ADLDisplayConfig DisplayConfig = new ADLDisplayConfig();
                                            if (ADL.ADL_Display_DeviceConfig_Get != null)
                                            {
                                                // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                                ADLRet = ADL.ADL_Display_DeviceConfig_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, DisplayInfoData[j].DisplayID.DisplayLogicalIndex, out DisplayConfig);
                                                if (ADLRet == ADL.ADL_SUCCESS)
                                                {

                                                }
                                            }

                                            // Create an array of all the important display info we need to record
                                            List<string> displayInfoIdentifierSection = new List<string>();
                                            displayInfoIdentifierSection.Add("AMD");
                                            try
                                            {
                                                displayInfoIdentifierSection.Add(OSAdapterInfoData.ADLAdapterInfo[i].AdapterName);
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Index from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(OSAdapterInfoData.ADLAdapterInfo[i].VendorID.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("1002");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(AdapterID.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                ADL.ADLConnectionType connector = (ADL.ADLConnectionType)DisplayInfoData[j].DisplayOutputType;
                                                displayInfoIdentifierSection.Add(connector.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(DisplayInfoData[j].DisplayManufacturerName);
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Display manufacturer from AMD video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            try
                                            {
                                                displayInfoIdentifierSection.Add(DisplayInfoData[j].DisplayName);
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Display Name from AMD video card. Substituting with a # instead");
                                                displayInfoIdentifierSection.Add("#");
                                            }

                                            // Create a display identifier out of it
                                            string displayIdentifier = String.Join("|", displayInfoIdentifierSection);
                                            // Add it to the list of display identifiers so we can return it
                                            displayIdentifiers.Add(displayIdentifier);

                                            SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");

                                        }
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
                            else
                            {
                                Console.WriteLine("ADL_Adapter_Active_Get() returned error code " + ADLRet.ToString());
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
                {
                    Marshal.FreeCoTaskMem(AdapterBuffer);
                }

                // Return all the identifiers we've found
                return displayIdentifiers;
            }
            else
            {
                SharedLogger.logger.Error($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }
    }
}
