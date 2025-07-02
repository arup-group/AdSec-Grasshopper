using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class BusinessComponentNameTest {
    private readonly FakeDropdownComponent _dropdownComponent;
    private readonly FakeBusiness fakeBusiness;

    public BusinessComponentNameTest() {
      fakeBusiness = new FakeBusiness();
      _dropdownComponent = new FakeDropdownComponent();
    }

    [Fact]
    public void ShouldHaveNameFromBusiness() {
      Assert.Equal(fakeBusiness.Metadata.Name, _dropdownComponent.Name);
    }

    [Fact]
    public void ShouldHaveNicknameFromBusiness() {
      Assert.Equal(fakeBusiness.Metadata.NickName, _dropdownComponent.NickName);
    }

    [Fact]
    public void ShouldHaveDescriptionFromBusiness() {
      Assert.Equal(fakeBusiness.Metadata.Description, _dropdownComponent.Description);
    }

    [Fact]
    public void ShouldBeAtTheRightCategory() {
      Assert.Equal(fakeBusiness.Organisation.Category, _dropdownComponent.Category);
    }

    [Fact]
    public void ShouldBeAtTheRightSubCategory() {
      Assert.Equal(fakeBusiness.Organisation.SubCategory, _dropdownComponent.SubCategory);
    }
  }
}
