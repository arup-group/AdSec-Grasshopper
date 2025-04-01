using AdSecCore;

namespace AdSecCoreTests {
  public class StringExtensionsTests {
    [Fact]
    public void ReplaceNonAlphanumeric_ShouldRemoveDigits() {
      const string input = "Hello123World456";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldRemovePeriods() {
      const string input = "Hello.World.";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldRemoveHyphens() {
      const string input = "Hello-World-";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldReturnAlphanumericString() {
      const string input = "HelloWorld";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldReturnEmptyString() {
      string input = string.Empty;
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldReturnEmptyForNonAlphanumericString() {
      const string input = "!!!---...###";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal("!!!###", result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldReturnEmptyForNumericString() {
      const string input = "1234567890";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldRemoveMultipleSpecialCharacters() {
      const string input = "Hello!@#$%^&*()World";
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal("Hello!@#$%^&*()World", result);
    }

    [Fact]
    public void ReplaceNonAlphanumeric_ShouldHandleLongStrings() {
      string input = new string('a', 1000) + "!" + new string('b', 1000);
      string result = input.ReplaceNonAlphanumeric();

      Assert.Equal(new string('a', 1000) + "!" + new string('b', 1000), result);
    }
  }
}
