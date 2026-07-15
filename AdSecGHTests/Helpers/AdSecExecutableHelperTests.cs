using System;
using System.Diagnostics;
using System.IO;

using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  public class AdSecExecutableHelperTests : IDisposable {
    private const string samplePath = @"C:\Oasys\AdSec 1.0\AdSec.exe";
    private readonly string _tempRoot;

    public AdSecExecutableHelperTests() {
      _tempRoot = Path.Combine(Path.GetTempPath(), $"AdSecExecutableHelperTests_{Guid.NewGuid()}");
      Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose() {
      if (Directory.Exists(_tempRoot)) {
        Directory.Delete(_tempRoot, true);
      }
    }

    [Fact]
    public void FindLatestExePath_ReturnsNull_WhenNoInstallationsExist() {
      var locator = new AdSecExecutableLocator();

      var result = locator.FindLatestExePath(new[] { _tempRoot });

      Assert.Null(result);
    }

    [Fact]
    public void FindLatestExePath_ReturnsHighestVersion_WhenMultipleVersionsExist() {
      var locator = new AdSecExecutableLocator();

      string rootA = Path.Combine(_tempRoot, "A");
      string rootB = Path.Combine(_tempRoot, "B");
      string exe100 = CreateInstall(rootA, "10.0", true);
      string exe102 = CreateInstall(rootB, "10.2", true);

      var result = locator.FindLatestExePath(new[] { rootA, rootB });

      Assert.Equal(exe102, result);
    }

    [Fact]
    public void FindLatestExePath_IgnoresInvalidAndMissingExeFolders() {
      var locator = new AdSecExecutableLocator();
      string root = Path.Combine(_tempRoot, "R");

      Directory.CreateDirectory(Path.Combine(root, "Oasys", "AdSec Beta"));
      Directory.CreateDirectory(Path.Combine(root, "Oasys", "AdSec 11.0"));

      string validExe = CreateInstall(root, "10.5", true);

      var result = locator.FindLatestExePath(new[] { root });

      Assert.Equal(validExe, result);
    }

    [Fact]
    public void FindLatestExePath_HandlesDuplicateAndEmptyRoots() {
      var locator = new AdSecExecutableLocator();
      string root = Path.Combine(_tempRoot, "R");
      string exe = CreateInstall(root, "10.1", true);

      var result = locator.FindLatestExePath(new[] { root, root, "", " ", null });

      Assert.Equal(exe, result);
    }

    [Fact]
    public void AdSecLauncher_Throws_WhenLocatorIsNull() {
      var exception = Assert.Throws<ArgumentNullException>(() => new AdSecLauncher(null, new FakeProcessStarter()));

      Assert.Equal("locator", exception.ParamName);
    }

    [Fact]
    public void AdSecLauncher_Throws_WhenProcessStarterIsNull() {
      var exception = Assert.Throws<ArgumentNullException>(() => new AdSecLauncher(new FakeLocator("x"), null));

      Assert.Equal("processStarter", exception.ParamName);
    }

    [Fact]
    public void StartLatest_ReturnsNull_WhenLocatorReturnsNullPath() {
      var starter = new FakeProcessStarter();
      var launcher = new AdSecLauncher(new FakeLocator(null), starter);

      var result = launcher.StartLatest("model.ads");

      Assert.Null(result);
      Assert.Equal(0, starter.CallCount);
    }

    [Fact]
    public void StartLatest_ReturnsProcess_WhenStartSucceeds() {
      var expected = new Process();
      var starter = new FakeProcessStarter {
        ProcessToReturn = expected,
      };
      var launcher = new AdSecLauncher(new FakeLocator(samplePath), starter);

      var result = launcher.StartLatest("model.ads");

      Assert.Same(expected, result);
      Assert.Equal(1, starter.CallCount);
      Assert.Equal(samplePath, starter.LastFileName);
      Assert.Equal("model.ads", starter.LastArguments);
    }

    [Fact]
    public void StartLatest_ReturnsNull_WhenProcessStarterThrows() {
      var starter = new FakeProcessStarter {
        ShouldThrow = true,
      };
      var launcher = new AdSecLauncher(new FakeLocator(samplePath), starter);

      var result = launcher.StartLatest("model.ads");

      Assert.Null(result);
      Assert.Equal(1, starter.CallCount);
    }

    private static string CreateInstall(string root, string version, bool withExe) {
      string dir = Path.Combine(root, "Oasys", $"AdSec {version}");
      Directory.CreateDirectory(dir);

      string exePath = Path.Combine(dir, "AdSec.exe");
      if (withExe) {
        File.WriteAllText(exePath, string.Empty);
      }

      return exePath;
    }

    private sealed class FakeLocator : IAdSecExecutableLocator {
      private readonly string _path;

      public FakeLocator(string path) {
        _path = path;
      }

      public string FindLatestExePath() {
        return _path;
      }
    }

    private sealed class FakeProcessStarter : IProcessStarter {
      public int CallCount { get; private set; }
      public string LastFileName { get; private set; }
      public string LastArguments { get; private set; }
      public bool ShouldThrow { get; set; }
      public Process ProcessToReturn { get; set; }

      public Process Start(string fileName, string arguments) {
        CallCount++;
        LastFileName = fileName;
        LastArguments = arguments;

        if (ShouldThrow) {
          throw new InvalidOperationException("Start failed.");
        }

        return ProcessToReturn;
      }
    }
  }
}
