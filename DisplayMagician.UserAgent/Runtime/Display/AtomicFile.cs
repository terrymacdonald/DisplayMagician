using System;
using System.IO;
using System.Text;

namespace DisplayMagician.UserAgent.Runtime
{
    public static class AtomicFile
    {
        public static void WriteAllText(string fileName, string contents, Encoding encoding)
        {
            string? directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string temporaryFileName = $"{fileName}.{Guid.NewGuid():N}.tmp";
            string backupFileName = $"{fileName}.bak";

            try
            {
                using (FileStream stream = new FileStream(temporaryFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                using (StreamWriter writer = new StreamWriter(stream, encoding))
                {
                    writer.Write(contents);
                    writer.Flush();
                    stream.Flush(true);
                }

                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Replace(temporaryFileName, fileName, backupFileName, ignoreMetadataErrors: true);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(fileName, backupFileName, overwrite: true);
                        File.Move(temporaryFileName, fileName, overwrite: true);
                    }
                }
                else
                {
                    File.Move(temporaryFileName, fileName);
                }
            }
            finally
            {
                if (File.Exists(temporaryFileName))
                    File.Delete(temporaryFileName);
            }
        }
    }
}
