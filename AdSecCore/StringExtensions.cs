using System;
using System.Text.RegularExpressions;

namespace AdSecCore {
  public static class StringExtensions {
    public static string ReplaceNonAlphanumeric(this string input) {
      return Regex.Replace(input, "[0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5))
       .Replace(".", string.Empty).Replace("-", string.Empty);
    }
  }
}
