using AdSecCore.Functions;

using AdSecGHTests;

namespace AdSecCoreTests.Functions {
  public class EditSectionFunctionTests {

    private readonly EditSectionFunction function;

    public EditSectionFunctionTests() {
      function = new EditSectionFunction();
      function.Section.Value = SampleData.GetSectionDesign();
    }

    [Fact]
    public void ShouldPassProfile() {
      function.Compute();
      Assert.NotNull(function.ProfileOut.Value);
    }

    [Fact]
    public void ShouldHaveSevenOutputs() {
      function.Compute();
      Assert.Equal(7, function.GetAllOutputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveSixInputs() {
      function.Compute();
      Assert.Equal(6, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveAllButFirstParametersOptional() {
      Assert.True(AllButFirstOptional());
    }

    private static bool AllButFirstOptional() {
      var function = new EditSectionFunction();
      var inputAttributes = function.GetAllInputAttributes();
      for (int i = 1; i < inputAttributes.Length; i++) {
        var attribute = inputAttributes[i];
        if (!attribute.Optional) {
          return false;
        }
      }

      return true;
    }
  }
}
