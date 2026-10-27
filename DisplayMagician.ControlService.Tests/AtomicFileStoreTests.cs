using System;
using System.IO;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class AtomicFileStoreTests
{
    [Theory]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(1175)]
    public void TransientReplacementFailuresRetryAndPreserveTheBackup(int error)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            string destination = Path.Combine(root, "state.json");
            string temporary = Path.Combine(root, "state.tmp");
            string backup = destination + ".bak";
            File.WriteAllText(destination, "original");
            File.WriteAllText(temporary, "updated");
            int attempts = 0;
            AtomicFileStore.ReplaceWithRetry(temporary, destination, backup, (source, target, old) =>
            {
                if (++attempts < 3) throw new IOException("Simulated transient file contention", unchecked((int)0x80070000) | error);
                File.Replace(source, target, old);
            });
            Assert.Equal(3, attempts);
            Assert.Equal("updated", File.ReadAllText(destination));
            Assert.Equal("original", File.ReadAllText(backup));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(32, 3)]
    [InlineData(112, 1)]
    public void ExhaustedOrNonTransientErrorsKeepThePreviousFile(int error, int expectedAttempts)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            string destination = Path.Combine(root, "state.json");
            string temporary = Path.Combine(root, "state.tmp");
            File.WriteAllText(destination, "original");
            File.WriteAllText(temporary, "updated");
            int attempts = 0;
            Assert.Throws<IOException>(() => AtomicFileStore.ReplaceWithRetry(temporary, destination, null, (_, _, _) =>
            {
                attempts++;
                throw new IOException("Simulated write failure", unchecked((int)0x80070000) | error);
            }));
            Assert.Equal(expectedAttempts, attempts);
            Assert.Equal("original", File.ReadAllText(destination));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
