using System.IO;

namespace DisplayMagician.UserAgent;

/// <summary>The service accepted a status in memory but has not durably acknowledged it.</summary>
internal sealed class OperationStatusPersistenceException : IOException
{
    internal OperationStatusPersistenceException(string message) : base(message) { }
}
