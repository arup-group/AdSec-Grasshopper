using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {

  [Collection("GrasshopperFixture collection")]
  public class DataConvertorTest {
    private readonly DummyBusiness dummyBusiness;
    private readonly DummyOasysDropdown oasysDropdown;

    public DataConvertorTest() {
      dummyBusiness = new DummyBusiness();
      oasysDropdown = new DummyOasysDropdown();
    }

    [Fact]
    public void ShouldHaveTheSameNumberOfInputs() {
      Assert.Single(oasysDropdown.Params.Input);
    }

    [Fact]
    public void ShouldPassTheName() {
      Assert.Equal("Alpha", oasysDropdown.Params.Input[0].Name);
    }

    [Fact]
    public void ShouldHaveDefaultValues() {
      oasysDropdown.SetDefaultValues();
      oasysDropdown.CollectData();
      oasysDropdown.ExpireSolution(true);
      dynamic actual = oasysDropdown.Params.Input[0].VolatileData.get_Branch(0)[0]; // as GH_Number;
      Assert.Equal((float)dummyBusiness.Alpha.Default, actual.Value, 0.01f);
    }

    [Fact]
    public void ShouldPassTheNickname() {
      Assert.Equal("A", oasysDropdown.Params.Input[0].NickName);
    }
  }

}
