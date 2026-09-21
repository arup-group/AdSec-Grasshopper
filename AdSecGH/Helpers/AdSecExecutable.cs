using System;
using System.Diagnostics;
using System.IO;

namespace AdSecGH.Helpers {
  internal static class AdSecExecutable {
    internal static string GetLatestPath() {
      string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string oasysPath = Path.Combine(programFiles, "Oasys");
      return GetLatestPath(oasysPath, GetVersion);
    }

    internal static string GetLatestPath(string oasysPath, Func<string, Version> getVersion) {
      if (!Directory.Exists(oasysPath)) {
        return null;
      }

      string fullPath = null;
      Version latestVersion = null;

      foreach (string directory in Directory.GetDirectories(oasysPath, "AdSec*")) {
        string exePath = Path.Combine(directory, "AdSec.exe");

        if (!File.Exists(exePath)) {
          continue;
        }

        Version version = getVersion(exePath);
        if (version == null) {
          continue;
        }

        if (latestVersion == null || version > latestVersion) {
          latestVersion = version;
          fullPath = exePath;
        }
      }

      return fullPath;
    }

    private static Version GetVersion(string exePath) {
      var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
      return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart);
    }
  }
}
