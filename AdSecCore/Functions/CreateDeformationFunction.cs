
using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCore.Functions {
  public class CreateDeformationFunction : Function {
    public StrainParameter StrainInput { get; set; } = new StrainParameter {
      Name = "εx",
      NickName = "X",
      Description = "The axial strain. Positive X indicates tension.",
      Access = Access.Item,
      Optional = true,
    };

    public CurvatureParameter CurvatureYInput { get; set; } = new CurvatureParameter {
      Name = "κyy",
      NickName = "YY",
      Description = "The curvature about local y-axis. It follows the right hand grip rule about the axis. Positive YY is anti-clockwise curvature about local y-axis.",
      Access = Access.Item,
      Optional = true,
    };

    public CurvatureParameter CurvatureZInput { get; set; } = new CurvatureParameter {
      Name = "κzz",
      NickName = "ZZ",
      Description = "The curvature about local z-axis. It follows the right hand grip rule about the axis. Positive ZZ is anti-clockwise curvature about local z-axis.",
      Access = Access.Item,
      Optional = true,
    };

    public DeformationParameter DeformationOutput { get; set; } = new DeformationParameter {
      Name = "Load",
      NickName = "Ld",
      Description = "AdSec Load",
      Access = Access.Item,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        StrainInput,
        CurvatureYInput,
        CurvatureZInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        DeformationOutput,
      };
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Deformation Load",
      NickName = "Deformation",
      Description = "Create an AdSec Deformation Load from an axial strain and biaxial curvatures",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat5(),
    };

    protected override void UpdateParameter() {
      base.UpdateParameter();
      StrainInput.Name = UnitExtensions.NameWithUnits("εx", StrainUnitResult);
      CurvatureYInput.Name = UnitExtensions.NameWithUnits("κyy", CurvatureUnit);
      CurvatureZInput.Name = UnitExtensions.NameWithUnits("κzz", CurvatureUnit);
    }

    public override void Compute() {

      var strain = Strain.From(StrainInput.Value.Value, StrainUnitResult);

      var curvatureY = Curvature.From(CurvatureYInput.Value.Value, CurvatureUnit);

      var curvatureZ = Curvature.From(CurvatureZInput.Value.Value, CurvatureUnit);

      var deformation = IDeformation.Create(strain, curvatureY, curvatureZ);

      DeformationOutput.Value = deformation;
    }
  }
}
