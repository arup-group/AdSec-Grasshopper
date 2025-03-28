using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using OasysGH.Components;

using Xunit;

namespace AdSecGHTests.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateConcreteCrackCalculationParametersTests {

    public static GH_OasysDropDownComponent ComponentMother() {
      var comp = new CreateConcreteCrackParameters();
      comp.CreateAttributes();

      comp.SetSelected(0, 0); // change dropdown to ?

      ComponentTestHelper.SetInput(comp, 130);
      ComponentTestHelper.SetInput(comp, 1700, 1);
      ComponentTestHelper.SetInput(comp, 1200, 2);

      return comp;
    }

    [Fact]
    public void ChangeDropDownTest() {
      var comp = ComponentMother();
      OasysDropDownComponentTestHelper.ChangeDropDownTest(comp);
    }

    [Fact]
    public void CreateComponent() {
      var comp = ComponentMother();
      comp.SetSelected(0, 0); // change dropdown to ?
      var output = (AdSecConcreteCrackCalculationParametersGoo)ComponentTestHelper.GetOutput(comp);
      //Assert.Equal(0, output.Value.StartPosition.Value);
      //Assert.Equal(130, output.Value.OverallDepth.Millimeters);
      //Assert.Equal(1.7, output.Value.AvailableWidthLeft.Meters);
      //Assert.Equal(1.2, output.Value.AvailableWidthRight.Meters);
      //Assert.False(output.Value.TaperedToNext);
    }

    //[Fact]
    //public void CreateComponentWithInputs1()
    //{
    //  var comp = ComponentMother();

    //  comp.SetSelected(0, 2); // change dropdown to m

    //  ComponentTestHelper.SetInput(comp, 1.5, 4);
    //  ComponentTestHelper.SetInput(comp, 1.0, 5);
    //  ComponentTestHelper.SetInput(comp, true, 6);

    //  SlabDimensionGoo output = (SlabDimensionGoo)ComponentTestHelper.GetOutput(comp);
    //  Assert.Equal(1.5, output.Value.EffectiveWidthLeft.Meters);
    //  Assert.Equal(1.0, output.Value.EffectiveWidthRight.Meters);
    //  Assert.True(output.Value.TaperedToNext);
    //}

    [Fact]
    public void DeserializeTest() {
      var comp = ComponentMother();
      Assert.Empty(OasysDropDownComponentTestHelper.DeserializeTest(comp));
    }
  }
}
