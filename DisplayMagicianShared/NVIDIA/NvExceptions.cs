using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     Represents errors that raised by NVIDIA Api
    /// </summary>
    public class NVIDIAApiException : Exception
    {
        internal NVIDIAApiException(Status status)
        {
            Status = status;
        }

        /// <inheritdoc />
        public override string Message
        {
            get => NVAPI.GetErrorMessage(Status) ?? Status.ToString();
        }

        /// <summary>
        ///     Gets NVIDIA Api exception status code
        /// </summary>
        public Status Status { get; }
    }

    /// <summary>
    ///     Represents errors that raised by NvAPIWrapper
    /// </summary>
    public class NVIDIANotSupportedException : NotSupportedException
    {
        internal NVIDIANotSupportedException(string message) : base(message)
        {
        }
    }
}
