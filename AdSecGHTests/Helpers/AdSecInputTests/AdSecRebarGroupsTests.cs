using System.Collections.Generic;

using AdSecCore.Builders;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecRebarGroupsTests {
    private readonly List<AdSecRebarGroup> _groups;
    private readonly List<int> invalidIds;

    public AdSecRebarGroupsTests() {
      _groups = new List<AdSecRebarGroup>();
      invalidIds = new List<int>();
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsFalseWhenInvalidIdsListIsNullInitialised() {
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(null, _groups, null);

      Assert.False(castSuccessful);
      Assert.Empty(_groups);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsFalseWhenNull() {
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(null, _groups, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_groups);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsFalseWhenEmptySections() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(objectWrappers, _groups, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_groups);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsFalseWhenValueIsNull() {
      AdSecRebarGroupGoo group = null;
      var objectWrappers = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(group),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(objectWrappers, _groups, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_groups);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsEmptyWhenNullItemSections() {
      var objectWrapper = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper> {
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(objectWrappers, _groups, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_groups);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsFalseWhenSecondItemIncorrect() {
      var rebarGroup = new AdSecRebarGroupGoo();
      var objectWrappers = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(rebarGroup),
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(objectWrappers, _groups, invalidIds);

      Assert.False(castSuccessful);
      Assert.Single(_groups);
      Assert.Single(invalidIds);
      Assert.Equal(1, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsCorrectDataFromRebarLayerGoo() {
      var rebarGroup = new AdSecRebarGroupGoo();

      var objwrap = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(rebarGroup),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(objwrap, _groups, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_groups);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupsReturnsCorrectDataFromIGroup() {
      var rebarGroup = new BuilderSingleBar().Build();

      var objwrap = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(rebarGroup),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroups(objwrap, _groups, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_groups);
      Assert.Empty(invalidIds);
    }
  }
}
