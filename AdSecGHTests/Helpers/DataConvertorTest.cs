using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class DataConvertorTest {
    private readonly DummyComponent _component;

    public DataConvertorTest() {
      _component = new DummyComponent();
    }

    [Fact]
    public void ShouldHaveTheSameNumberOfInputs() {
      Assert.Single(_component.Params.Input);
    }

    [Fact]
    public void ShouldPassTheName() {
      Assert.Equal("Alpha", _component.Params.Input[0].Name);
    }

    [Fact]
    public void ShouldPassTheNickname() {
      Assert.Equal("A", _component.Params.Input[0].NickName);
    }
  }

}
