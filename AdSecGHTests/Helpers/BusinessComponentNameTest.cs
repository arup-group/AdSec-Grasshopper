using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class BusinessComponentNameTest {
    private readonly FakeComponent component;
    private readonly FakeBusiness fakeBusiness;

    public BusinessComponentNameTest() {
      fakeBusiness = new FakeBusiness();
      component = new FakeComponent();
    }

    [Fact]
    public void ShouldHaveNameFromBusiness() {
      Assert.Equal(fakeBusiness.Metadata.Name, component.Name);
    }

    [Fact]
    public void ShouldHaveNicknameFromBusiness() {
      Assert.Equal(fakeBusiness.Metadata.NickName, component.NickName);
    }

    [Fact]
    public void ShouldHaveDescriptionFromBusiness() {
      Assert.Equal(fakeBusiness.Metadata.Description, component.Description);
    }

    [Fact]
    public void ShouldBeAtTheRightCategory() {
      Assert.Equal(fakeBusiness.Organisation.Category, component.Category);
    }

    [Fact]
    public void ShouldBeAtTheRightSubCategory() {
      Assert.Equal(fakeBusiness.Organisation.SubCategory, component.SubCategory);
    }
  }
}
