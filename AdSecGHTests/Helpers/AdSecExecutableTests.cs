using System;
using System.Collections.Generic;
using System.IO;

using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  public class AdSecExecutableTests {
    [Fact]
    public void GetLatestPathReturnsNullWhenOasysFolderDoesNotExist() {
      string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

      string fullPath = AdSecExecutable.GetLatestPath(missingPath, _ => new Version(10, 0));

      Assert.Null(fullPath);
    }

    [Fact]
    public void GetLatestPathReturnsHighestMajorMinorVersion() {
      string rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

      try {
        var versionsByPath = CreateFakeInstall(rootPath, ("AdSec 10.0", new Version(10, 0)));
        foreach (var item in CreateFakeInstall(rootPath, ("AdSec 10.1", new Version(10, 1)))) {
          versionsByPath[item.Key] = item.Value;
        }

        Version GetVersionFromDictionary(string exePath) {
          return versionsByPath[exePath];
        }

        string fullPath = AdSecExecutable.GetLatestPath(rootPath, GetVersionFromDictionary);

        Assert.Equal(Path.Combine(rootPath, "AdSec 10.1", "AdSec.exe"), fullPath);
      } finally {
        if (Directory.Exists(rootPath)) {
          Directory.Delete(rootPath, true);
        }
      }
    }

    private static Dictionary<string, Version> CreateFakeInstall(string rootPath, (string FolderName, Version Version) install) {
      string installPath = Path.Combine(rootPath, install.FolderName);
      Directory.CreateDirectory(installPath);

      string exePath = Path.Combine(installPath, "AdSec.exe");
      File.WriteAllText(exePath, string.Empty);

      return new Dictionary<string, Version> {
        { exePath, install.Version },
      };
    }

  }
}
