using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class BusinessComponentNameTest {
    private readonly Dummy component;
    private readonly DummyBusiness dummyBusiness;

    public BusinessComponentNameTest() {
      dummyBusiness = new DummyBusiness();
      component = new Dummy();
    }

    [Fact]
    public void ShouldHaveNameFromBusiness() {
      Assert.Equal(dummyBusiness.Metadata.Name, component.Name);
    }

    [Fact]
    public void ShouldHaveNicknameFromBusiness() {
      Assert.Equal(dummyBusiness.Metadata.NickName, component.NickName);
    }

    [Fact]
    public void ShouldHaveDescriptionFromBusiness() {
      Assert.Equal(dummyBusiness.Metadata.Description, component.Description);
    }

    [Fact]
    public void ShouldBeAtTheRightCategory() {
      Assert.Equal(dummyBusiness.Organisation.Category, component.Category);
    }

    [Fact]
    public void ShouldBeAtTheRightSubCategory() {
      Assert.Equal(dummyBusiness.Organisation.SubCategory, component.SubCategory);
    }
  }
}
