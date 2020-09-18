using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsDisplayAPI;
using EDIDParser;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.GPU;
using Microsoft.Win32;

namespace HeliosPlus.Shared.DisplayIdentification
{
    class DisplayIdentifier
    {
        public static List<string> GetCurrentDisplayIdentification()
        {
            List<string> DisplayIdentificationData = new List<string>();
            List<Display> ConnectedDisplays = Display.GetDisplays().ToList<Display>();
            foreach (Display ConnectedDisplay in ConnectedDisplays)
            {
                byte[] edidData;
                using (RegistryKey key = ConnectedDisplay.OpenDevicePnPKey())
                {
                    using (RegistryKey subkey = key.OpenSubKey("Device Parameters"))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        edidData = (byte[])subkey.GetValue("EDID", null);
                    }
                }
                EDID edid = new EDID(edidData);

                List<EDIDDescriptor> descriptors = edid.Descriptors.ToList();
                foreach (EDIDDescriptor descriptor in descriptors)
                {
                    Console.WriteLine($"EDID-Descriptor IsValid:{descriptor.IsValid}");
                    Console.WriteLine($"EDID-Descriptor String:{descriptor.ToString()}");
                }

                Console.WriteLine($"EDID-DisplayParameters:{edid.DisplayParameters}");
                Console.WriteLine($"EDID-EDIDVersion:{edid.EDIDVersion}");
                Console.WriteLine($"EDID-ManufacturerDate:{edid.ManufactureDate}");
                Console.WriteLine($"EDID-ManufacturerCode:{edid.ManufacturerCode}");
                Console.WriteLine($"EDID-ManufacturerID:{edid.ManufacturerId}");
                Console.WriteLine($"EDID-ManufacturerWeek:{edid.ManufactureWeek}");
                Console.WriteLine($"EDID-ManufacturerYear:{edid.ManufactureYear}");
                Console.WriteLine($"EDID-ManufacturerID:{edid.NumberOfExtensions}");
                Console.WriteLine($"EDID-ProductCode:{edid.ProductCode}");
                Console.WriteLine($"EDID-ProductYear:{edid.ProductYear}");
                Console.WriteLine($"EDID-SerialNumber:{edid.SerialNumber}");

                List<ITiming> timingList = edid.Timings.ToList();
                foreach (ITiming timing in timingList)
                {
                    Console.WriteLine($"EDID-Timing Width:{timing.Width}");
                    Console.WriteLine($"EDID-Timing Width:{timing.Height}");
                    Console.WriteLine($"EDID-Timing Width:{timing.Frequency}");
                    Console.WriteLine($"EDID-Timing ToString:{timing.ToString()}");
                }

                List<EDIDExtension> exts = edid.Extensions.ToList();
                foreach (EDIDExtension ext in exts)
                {
                    Console.WriteLine($"EDID-Extension Type:{ext.Type}");
                    Console.WriteLine($"EDID-Extension String:{ext.ToString()}");
                }

                Console.WriteLine($"EDID-AdapterDeviceKey:{ConnectedDisplay.Adapter.DeviceName}");
                Console.WriteLine($"EDID-AdapterDeviceName:{ConnectedDisplay.Adapter.DeviceName}");
                Console.WriteLine($"EDID-AdapterDevicePath:{ConnectedDisplay.Adapter.DevicePath}");
                Console.WriteLine($"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}");
                string DisplayIdentificationHash = $"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}";
                DisplayIdentificationData.Add(DisplayIdentificationHash);
            }
            return DisplayIdentificationData;

        }

        public static string GetCurrentDisplayIdentification(Display display)
        {
            byte[] edidData;
            using (RegistryKey key = display.OpenDevicePnPKey())
            {
                using (RegistryKey subkey = key.OpenSubKey("Device Parameters"))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    edidData = (byte[])subkey.GetValue("EDID", null);
                }
            }
            EDID edid = new EDID(edidData);
            Console.WriteLine($"EDID-ManufacturerID:{edid.ManufacturerId}");
            Console.WriteLine($"EDID-ManufacturerCode:{edid.ManufacturerCode}");
            Console.WriteLine($"EDID-ManufacturerYear:{edid.ManufactureYear}");
            Console.WriteLine($"EDID-ManufacturerWeek:{edid.ManufactureWeek}");
            Console.WriteLine($"EDID-ManufacturerDate:{edid.ManufactureDate}");
            Console.WriteLine($"EDID-ProductCode:{edid.ProductCode}");
            Console.WriteLine($"EDID-ProductYear:{edid.ProductYear}");
            Console.WriteLine($"EDID-SerialNumber:{edid.SerialNumber}");
            Console.WriteLine($"EDID-Timings:{edid.Timings}");
            Console.WriteLine($"EDID-AdapterDeviceKey:{display.Adapter.DeviceName}");
            Console.WriteLine($"EDID-AdapterDeviceName:{display.Adapter.DeviceName}");
            Console.WriteLine($"EDID-AdapterDevicePath:{display.Adapter.DevicePath}");
            Console.WriteLine($"EDID-{display.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}");
            return $"EDID-{display.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}";

        }

        public static bool IsDisplayStillHere(string DisplayIdentificationHash, Display display)
        {
            byte[] edidData;
            using (RegistryKey key = display.OpenDevicePnPKey())
            {
                using (RegistryKey subkey = key.OpenSubKey("Device Parameters"))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    edidData = (byte[])subkey.GetValue("EDID", null);
                }
            }
            // Parse the EDID data into a useful structure
            EDID edid = new EDID(edidData);
            string thisDisplayIdentificationHash = $"EDID-{display.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}";
            // Compare the supplied hash with the 
            if (DisplayIdentificationHash == thisDisplayIdentificationHash)
                return true;
            return false;
        }

        public static bool IsDisplayStillHere(string DisplayIdentificationHash)
        {
            List<Display> ConnectedDisplays = Display.GetDisplays().ToList<Display>();
            foreach (Display ConnectedDisplay in ConnectedDisplays)
            {
                byte[] edidData;
                using (RegistryKey key = ConnectedDisplay.OpenDevicePnPKey())
                {
                    using (RegistryKey subkey = key.OpenSubKey("Device Parameters"))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        edidData = (byte[])subkey.GetValue("EDID", null);
                    }
                }
                // Parse the EDID data into a useful structure
                EDID edid = new EDID(edidData);
                string thisDisplayIdentificationHash = $"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}";
                // Compare the supplied hash with the 
                if (DisplayIdentificationHash == thisDisplayIdentificationHash)
                    return true;
            }
            return false;
        }

        public static List<string> GetAllDisplayIdentification()
        {

            const string currentControlDisplayKeyName = "SYSTEM\\CurrentControlSet\\Enum\\DISPLAY";

            // Get access to the display key
            RegistryKey displayKey = Registry.LocalMachine.OpenSubKey(currentControlDisplayKeyName, RegistryKeyPermissionCheck.ReadSubTree);

            // Enumerate the displays subkey (one per display)
            foreach (string displayItemKeyName in displayKey.GetSubKeyNames())
            {
                RegistryKey displayItemKey = displayKey.OpenSubKey(displayItemKeyName);

                if (displayItemKey == null)
                    continue;

                // Enumerate the subkey under that (its an ID of some kind)
                foreach (string displayIDKeyName in displayItemKey.GetSubKeyNames())
                {

                    RegistryKey deviceParameters = displayItemKey.OpenSubKey($"{displayIDKeyName}\\Device Parameters");
                    if (deviceParameters == null)
                        continue;

                    // Access the EDID information if it's there 
                    byte[] edidData = (byte[]) deviceParameters.GetValue("EDID",null);

                    if (edidData == null)
                        continue;

                    EDID edid = new EDID(edidData);
                    Console.WriteLine($"EDID-ManufacturerID:{edid.ManufacturerId}");
                    Console.WriteLine($"EDID-ManufacturerCode:{edid.ManufacturerCode}");
                    Console.WriteLine($"EDID-ManufacturerYear:{edid.ManufactureYear}");
                    Console.WriteLine($"EDID-ManufacturerWeek:{edid.ManufactureWeek}");
                    Console.WriteLine($"EDID-ManufacturerDate:{edid.ManufactureDate}");
                    Console.WriteLine($"EDID-ProductCode:{edid.ProductCode}");
                    Console.WriteLine($"EDID-ProductYear:{edid.ProductYear}");
                    Console.WriteLine($"EDID-SerialNumber:{edid.SerialNumber}");
                    Console.WriteLine($"EDID-Timings:{edid.Timings}");
                    //Console.WriteLine($"EDID-AdapterDeviceKey:{ConnectedDisplay.Adapter.DeviceName}");
                    //Console.WriteLine($"EDID-AdapterDeviceName:{ConnectedDisplay.Adapter.DeviceName}");
                    //Console.WriteLine($"EDID-AdapterDevicePath:{ConnectedDisplay.Adapter.DevicePath}");
                    //Console.WriteLine($"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}");
                    //string DisplayIdentificationHash = $"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}";
                    //DisplayIdentificationData.Add(DisplayIdentificationHash);



                }

            }



                List<string> DisplayIdentificationData = new List<string>();
            List<Display> ConnectedDisplays = Display.GetDisplays().ToList<Display>();
            foreach (Display ConnectedDisplay in ConnectedDisplays)
            {
                byte[] edidData;
                using (RegistryKey key = ConnectedDisplay.OpenDevicePnPKey())
                {
                    using (RegistryKey subkey = key.OpenSubKey("Device Parameters"))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        edidData = (byte[])subkey.GetValue("EDID", null);
                    }
                }
                EDID edid = new EDID(edidData);
                Console.WriteLine($"EDID-ManufacturerID:{edid.ManufacturerId}");
                Console.WriteLine($"EDID-ManufacturerCode:{edid.ManufacturerCode}");
                Console.WriteLine($"EDID-ManufacturerYear:{edid.ManufactureYear}");
                Console.WriteLine($"EDID-ManufacturerWeek:{edid.ManufactureWeek}");
                Console.WriteLine($"EDID-ManufacturerDate:{edid.ManufactureDate}");
                Console.WriteLine($"EDID-ProductCode:{edid.ProductCode}");
                Console.WriteLine($"EDID-ProductYear:{edid.ProductYear}");
                Console.WriteLine($"EDID-SerialNumber:{edid.SerialNumber}");
                Console.WriteLine($"EDID-Timings:{edid.Timings}");
                Console.WriteLine($"EDID-AdapterDeviceKey:{ConnectedDisplay.Adapter.DeviceName}");
                Console.WriteLine($"EDID-AdapterDeviceName:{ConnectedDisplay.Adapter.DeviceName}");
                Console.WriteLine($"EDID-AdapterDevicePath:{ConnectedDisplay.Adapter.DevicePath}");
                Console.WriteLine($"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}");
                string DisplayIdentificationHash = $"EDID-{ConnectedDisplay.Adapter.DevicePath}-{edid.ManufacturerId}-{edid.ProductCode}-{edid.SerialNumber}";
                DisplayIdentificationData.Add(DisplayIdentificationHash);
            }
            return DisplayIdentificationData;

        }
    }
}
