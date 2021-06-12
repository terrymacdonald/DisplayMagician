using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ATI.ADL;

namespace DisplayMagicianShared.AMD
{
    class ADLWrapper
    {
        public ADLWrapper()
        {
            int ADLRet = ADL.ADL_FAIL;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            if (null != ADL.ADL2_Main_Control_Create)
                // Second parameter is 1: Get only the present adapters
                ADLRet = ADL.ADL2_Main_Control_Create(ADL.ADL2_Main_Memory_Alloc, 1);
            if (ADL.ADL_SUCCESS == ADLRet)
            {
                if (null != ADL.ADL2_Adapter_NumberOfAdapters_Get)
                {
                    ADL.ADL2_Adapter_NumberOfAdapters_Get(ref NumberOfAdapters);
                }
                Console.WriteLine("Number Of Adapters: " + NumberOfAdapters.ToString() + "\n");

                if (0 < NumberOfAdapters)
                {
                    // Get OS adpater info from ADL
                    ADLAdapterInfoArray OSAdapterInfoData;
                    OSAdapterInfoData = new ADLAdapterInfoArray();

                    if (null != ADL.ADL2_Adapter_AdapterInfo_Get)
                    {
                        IntPtr AdapterBuffer = IntPtr.Zero;
                        int size = Marshal.SizeOf(OSAdapterInfoData);
                        AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                        Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                        if (null != ADL.ADL2_Adapter_AdapterInfo_Get)
                        {
                            ADLRet = ADL.ADL2_Adapter_AdapterInfo_Get(AdapterBuffer, size);
                            if (ADL.ADL_SUCCESS == ADLRet)
                            {
                                OSAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                                int IsActive = 0;

                                for (int i = 0; i < NumberOfAdapters; i++)
                                {
                                    // Check if the adapter is active
                                    if (null != ADL.ADL2_Adapter_Active_Get)
                                        ADLRet = ADL.ADL2_Adapter_Active_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref IsActive);

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

                                        if (null != ADL.ADL2_Display_DisplayInfo_Get)
                                        {
                                            IntPtr DisplayBuffer = IntPtr.Zero;
                                            int j = 0;

                                            // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                            ADLRet = ADL.ADL2_Display_DisplayInfo_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 1);
                                            if (ADL.ADL_SUCCESS == ADLRet)
                                            {
                                                List<ADLDisplayInfo> DisplayInfoData = new List<ADLDisplayInfo>();
                                                for (j = 0; j < NumberOfDisplays; j++)
                                                {
                                                    oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(DisplayBuffer.ToInt32() + j * Marshal.SizeOf(oneDisplayInfo)), oneDisplayInfo.GetType());
                                                    DisplayInfoData.Add(oneDisplayInfo);
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
                if (null != ADL.ADL2_Main_Control_Destroy)
                    ADL.ADL2_Main_Control_Destroy();
            }
            else
            {
                Console.WriteLine("ADL_Main_Control_Create() returned error code " + ADLRet.ToString());
                Console.WriteLine("\nCheck if ADL is properly installed!\n");
            }
        }
    }
}
