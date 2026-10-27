using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using DisplayMagician.Contracts;
using DisplayMagician.ConfigurationDefinitions;

namespace DisplayMagician.UserAgent.Runtime
{
    public static class AtomicFile
    {
        internal static RepositorySnapshot GetSnapshot(string path, RepositoryKind repository)
        {
            byte[] content = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            return new RepositorySnapshot
            {
                Repository = repository,
                Revision = content.Length == 0 ? 0 : BitConverter.ToInt64(SHA256.HashData(content), 0),
                Json = content.Length == 0 ? string.Empty : Encoding.Unicode.GetString(content).TrimStart('\uFEFF')
            };
        }

        // The owning repository holds its lock across revision comparison, replacement and cache refresh.
        internal static RepositoryCommitResult CommitSnapshot(string path, RepositoryCommitRequest request, Action<string>? validate = null)
        {
            RepositorySnapshot current = GetSnapshot(path, request.Repository);
            if (current.Revision != request.ExpectedRevision)
                return new RepositoryCommitResult { WasConflict = true, Snapshot = current };
            RepositoryDocumentValidation.Validate(request.Json, request.Repository);
            validate?.Invoke(request.Json);
            WriteAllText(path, request.Json, Encoding.Unicode);
            return new RepositoryCommitResult { Snapshot = GetSnapshot(path, request.Repository) };
        }

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
