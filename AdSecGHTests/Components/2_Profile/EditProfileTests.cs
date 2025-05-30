using AdSecGH.Components;

using Xunit;

namespace AdSecGHTests.Components._2_Profile {
  [Collection("GrasshopperFixture collection")]
  public class EditProfileTests {
    private readonly EditProfile _component;
    public EditProfileTests() {
      _component = new EditProfile();
    }

    [Fact]
    public void ShouldHaveBusinessComponent() {
      Assert.NotNull(_component.BusinessComponent);
    }
  }
}
