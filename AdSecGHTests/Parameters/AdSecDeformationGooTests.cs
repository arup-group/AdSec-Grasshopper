using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

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
      Assert.Equal(1, vec.Value.X);
      Assert.Equal(2, vec.Value.Y);
      Assert.Equal(3, vec.Value.Z);
    }

  }
}
