namespace AdSecGHCore.Constants {

  /// <summary>
  ///   Class containing ribbon category names
  ///   Call this class from all components to pick which category component should sit in
  ///   Sorting of categories in the ribbon is controlled with a number of spaces in front of the name
  ///   to avoid naming each category with a number in front. Spaces will automatically be removed when displayed
  /// </summary>
  public static class SubCategoryName {

    private static readonly string[] categories = {
      "File",
      "Material",
      "Profile",
      "Rebar",
      "Section",
      "Loads",
      "Solution",
      "Results",
      "Params",
    };

    private static string Pad(int index) {
      // Last category without space
      if (index == categories.Length - 1) {
        return categories[index];
      }

      int padding = categories.Length - index;
      return $"{new string(' ', padding)}{categories[index]}";
    }

    public static string Cat0() {
      return Pad(0);
    }

    public static string Cat1() {
      return Pad(1);
    }

    public static string Cat2() {
      return Pad(2);
    }

    public static string Cat3() {
      return Pad(3);
    }

    public static string Cat4() {
      return Pad(4);
    }

    public static string Cat5() {
      return Pad(5);
    }

    public static string Cat6() {
      return Pad(6);
    }

    public static string Cat7() {
      return Pad(7);
    }

    public static string Cat8() {
      return Pad(8);
    }
  }
}
