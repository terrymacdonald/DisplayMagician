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
        public static List<string> GetDisplayIdentification()
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

        public static string GetDisplayIdentification(Display display)
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

        public static bool IsDisplayHere(string DisplayIdentificationHash, Display display)
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

        public static bool IsDisplayHere(string DisplayIdentificationHash)
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
    }
}
