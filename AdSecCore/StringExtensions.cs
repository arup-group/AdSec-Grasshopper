using System.Text.RegularExpressions;

namespace AdSecCore {
  public static class StringExtensions {
    public static string ReplaceNonAlphanumeric(this string input) {
      return Regex.Replace(input, "[0-9]", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
    }
  }
}
