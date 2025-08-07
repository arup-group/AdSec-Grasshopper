using AdSecCore.Constants;

using AdSecGH;

using Xunit;

namespace AdSecGHTests {
  [Collection("GrasshopperFixture collection")]
  public class AddReferencePropertyTests {

    public AddReferencePropertyTests() {
      var priority = new AddReferencePriority();
      priority.PriorityLoad();
    }

    [Fact]
    public void ShouldHaveAValidAdSecAssembly() {
      Assert.NotNull(AddReferencePriority.AdSecAPI);
    }

    [Fact]
    public void ShouldHaveTheSameReference() {
      Assert.Equal(AddReferencePriority.AdSecAPI, AdSecFileHelper.Custom);
    }

  }
}
