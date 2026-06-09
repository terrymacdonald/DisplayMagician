using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DisplayMagician
{
    internal class ExecutionMode
    {

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetCurrentPackageFullName(ref int packageFullNameLength, ref StringBuilder packageFullName);

        internal static bool IsRunningWithIdentity()
        {
            return TryGetPackageFullName(out _, out _);
        }

        internal static bool TryGetPackageFullName(out string packageFullName, out int errorCode)
        {
            StringBuilder sb = new StringBuilder(1024);
            int length = sb.Capacity;
            int result = GetCurrentPackageFullName(ref length, sb);

            if (result == 122 && length > 0)
            {
                sb = new StringBuilder(length);
                result = GetCurrentPackageFullName(ref length, sb);
            }

            errorCode = result;
            packageFullName = result == 0 ? sb.ToString() : string.Empty;

            return result == 0;
        }
    }
}
