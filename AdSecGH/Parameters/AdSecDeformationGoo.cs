using System;
using System.Linq;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using OasysGH.Units;
using OasysUnits;

namespace AdSecGH.Parameters
{
  public class AdSecDeformationGoo : GH_Goo<IDeformation>
  {
    public AdSecDeformationGoo(IDeformation deformation)
    : base(deformation)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Deformation";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecDeformationGoo(this.Value);
    }
    public override string ToString()
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      IQuantity curvature = new Curvature(0, DefaultUnits.CurvatureUnit);
      string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
      return "AdSec " + TypeName + " {"
          + Math.Round(this.Value.X.As(DefaultUnits.StrainUnitResult), 4) + strainUnitAbbreviation + ", "
          + Math.Round(this.Value.YY.As(DefaultUnits.CurvatureUnit), 4) + curvatureUnitAbbreviation + ", "
          + Math.Round(this.Value.ZZ.As(DefaultUnits.CurvatureUnit), 4) + curvatureUnitAbbreviation + "}";
    }
  }
}
