using System;
using System.IO;
using System.Text;
using System.Threading;

namespace DisplayMagician.ControlService;

public static class AtomicFileStore
{
    public static void WriteAllText(string destinationPath, string content, string? backupPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(content);

        WriteBytes(destinationPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(content), backupPath);
    }

    public static void WriteBytes(string destinationPath, byte[] content, string? backupPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(content);

        string? destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            throw new ArgumentException("The destination path must include a directory.", nameof(destinationPath));
        }

        Directory.CreateDirectory(destinationDirectory);
        string temporaryPath = Path.Combine(destinationDirectory, $".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                stream.Write(content, 0, content.Length);
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(destinationPath))
            {
                if (!string.IsNullOrWhiteSpace(backupPath))
                {
                    string? backupDirectory = Path.GetDirectoryName(backupPath);
                    if (!string.IsNullOrWhiteSpace(backupDirectory))
                    {
                        Directory.CreateDirectory(backupDirectory);
                    }
                }

                ReplaceWithRetry(temporaryPath, destinationPath, backupPath);
            }
            else
            {
                File.Move(temporaryPath, destinationPath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    internal static void ReplaceWithRetry(string temporaryPath, string destinationPath, string? backupPath, Action<string, string, string?>? replace = null)
    {
        replace ??= (source, destination, backup) => File.Replace(source, destination, backup, ignoreMetadataErrors: true);
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                replace(temporaryPath, destinationPath, backupPath);
                return;
            }
            catch (IOException ex) when ((ex.HResult & 0xffff) is 32 or 33 or 1175 &&
                File.Exists(temporaryPath) && File.Exists(destinationPath))
            {
                if (attempt >= 3)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex, "AtomicFileStore/ReplaceWithRetry: Stopped replacing {0} after {1} failed attempts; the write failed.", destinationPath, attempt);
                    throw;
                }
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "AtomicFileStore/ReplaceWithRetry: Transient replacement failure for {0} on attempt {1}; retrying.", destinationPath, attempt);
                Thread.Sleep(attempt * 100);
            }
        }
    }
}
