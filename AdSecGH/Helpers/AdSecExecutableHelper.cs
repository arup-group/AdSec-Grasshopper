using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdSecGH.Helpers {
  internal interface IAdSecExecutableLocator {
    string FindLatestExePath();
  }

  internal interface IProcessStarter {
    Process Start(string fileName, string arguments);
  }

  internal interface IAdSecLauncher {
    Process StartLatest(string filePath);
  }

  internal sealed class AdSecExecutableLocator : IAdSecExecutableLocator {
    public string FindLatestExePath() {
      return FindLatestExePath(GetStandardInstallationPaths());
    }

    internal string FindLatestExePath(IEnumerable<string> roots) {
      Version latest = null;
      string latestExe = null;

      foreach (string root in roots.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()) {
        string oasys = Path.Combine(root, "Oasys");
        if (!Directory.Exists(oasys)) {
          continue;
        }

        string[] dirs;
        try {
          dirs = Directory.GetDirectories(oasys, "AdSec *");
        } catch {
          continue;
        }

        foreach (string dir in dirs) {
          if (!TryGetVersion(dir, out Version version)) {
            continue;
          }

          string exe = Path.Combine(dir, "AdSec.exe");
          if (!File.Exists(exe)) {
            continue;
          }

          if (latest == null || version > latest) {
            latest = version;
            latestExe = exe;
          }
        }
      }

      return latestExe;
    }

    private static IEnumerable<string> GetStandardInstallationPaths() {
      return new[] {
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
      };
    }

    private static bool TryGetVersion(string directoryPath, out Version version) {
      string name = Path.GetFileName(directoryPath);
      const string prefix = "AdSec ";
      if (string.IsNullOrWhiteSpace(name) || !name.StartsWith(prefix)) {
        version = null;
        return false;
      }

      return Version.TryParse(name.Substring(prefix.Length).Trim(), out version);
    }
  }

  internal sealed class ProcessStarter : IProcessStarter {
    public Process Start(string fileName, string arguments) {
      return Process.Start(fileName, arguments);
    }
  }

  internal sealed class AdSecLauncher : IAdSecLauncher {
    private readonly IAdSecExecutableLocator _locator;
    private readonly IProcessStarter _processStarter;

    public AdSecLauncher(IAdSecExecutableLocator locator, IProcessStarter processStarter) {
      _locator = locator ?? throw new ArgumentNullException(nameof(locator));
      _processStarter = processStarter ?? throw new ArgumentNullException(nameof(processStarter));
    }

    public Process StartLatest(string filePath) {
      string exePath = _locator.FindLatestExePath();
      if (string.IsNullOrEmpty(exePath)) {
        return null;
      }

      try {
        return _processStarter.Start(exePath, filePath);
      } catch {
        return null;
      }
    }
  }
}
