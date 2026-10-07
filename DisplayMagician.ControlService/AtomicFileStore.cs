using System;
using System.IO;
using System.Text;

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

                File.Replace(temporaryPath, destinationPath, backupPath, ignoreMetadataErrors: true);
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
}
