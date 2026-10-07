using System;
using System.IO;
using DisplayMagician.UserAgent.Runtime;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class WallpaperCleanupTests
{
    [Fact]
    public void DeleteUnusedStoredWallpaperFiles_RemovesOnlyUnreferencedFilesInManagedFolder()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-WallpaperCleanup-{Guid.NewGuid():N}");
        try
        {
            string store = Path.Combine(root, "Wallpaper");
            string nested = Path.Combine(store, "Nested");
            Directory.CreateDirectory(nested);
            string unused = Path.Combine(store, "wallpaper-unused.png");
            string retained = Path.Combine(store, "wallpaper-retained.png");
            string nestedFile = Path.Combine(nested, "wallpaper-nested.png");
            string original = Path.Combine(root, "personal-wallpaper.png");
            File.WriteAllText(unused, "unused");
            File.WriteAllText(retained, "retained");
            File.WriteAllText(nestedFile, "nested");
            File.WriteAllText(original, "original");

            Wallpaper.DeleteUnusedStoredWallpaperFiles(store,
                new[] { unused, retained, nestedFile, original, Path.Combine(store, "..", "personal-wallpaper.png") },
                new[] { retained });

            Assert.False(File.Exists(unused));
            Assert.True(File.Exists(retained));
            Assert.True(File.Exists(nestedFile));
            Assert.True(File.Exists(original));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DeleteUnusedStoredWallpaperFiles_KeepsCandidatesWhenRetainedPathsCannotBeResolved()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-WallpaperCleanup-{Guid.NewGuid():N}");
        try
        {
            string store = Path.Combine(root, "Wallpaper");
            Directory.CreateDirectory(store);
            string candidate = Path.Combine(store, "wallpaper-candidate.png");
            File.WriteAllText(candidate, "candidate");

            Wallpaper.DeleteUnusedStoredWallpaperFiles(store, new[] { candidate }, new[] { "\0" });

            Assert.True(File.Exists(candidate));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
