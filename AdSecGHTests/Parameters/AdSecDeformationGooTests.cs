using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecDeformationGooTests {

    [Fact]
    public void ShouldBeAbleToCastToVector3d() {
      var value = IDeformation.Create(Strain.From(1, StrainUnit.Ratio), Curvature.From(2, CurvatureUnit.PerMeter),
        Curvature.From(3, CurvatureUnit.PerMeter));
      var deformation = new AdSecDeformationGoo(value);
      var vec = new GH_Vector();
      Assert.True(deformation.CastTo(ref vec));
      Assert.Equal(Strain.From(1, StrainUnit.Ratio).As(DefaultUnits.StrainUnitResult), vec.Value.X);
      Assert.Equal(Curvature.From(2, CurvatureUnit.PerMeter).As(DefaultUnits.CurvatureUnit), vec.Value.Y);
      Assert.Equal(Curvature.From(3, CurvatureUnit.PerMeter).As(DefaultUnits.CurvatureUnit), vec.Value.Z);
    }

  }
}
