using System;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysGH;
using OasysGH.Components;
using OasysGH.Parameters;
using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Components {
  public class ConcreteStressStrain : GH_OasysComponent {

    public ConcreteStressStrain() : base("Concrete Stress/Strain", "CSS",
      "Calculate the Concrete Stress/Strain at a point on the Section for a given Load or Deformation.",
      CategoryName.Name(), SubCategoryName.Cat7()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("542fc96d-d90a-4301-855f-d14507cc9753");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainConcrete;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Load", "Ld",
        "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Vertex Point", "Vx",
        "A 2D vertex in the section's local yz-plane for where to calculate strain.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      string strainUnitAbbreviation = Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(DefaultUnits.StressUnitResult);

      pManager.AddGenericParameter($"ULS Strain [{strainUnitAbbreviation}]", "εd", "ULS strain at Vertex Point",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"ULS Stress [{stressUnitAbbreviation}]", "σd", "ULS stress at Vertex Point",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"SLS Strain [{strainUnitAbbreviation}]", "εk", "SLS strain at Vertex Point",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"SLS Stress [{stressUnitAbbreviation}]", "σk", "SLS stress at Vertex Point",
        GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get solution input
      var solution = AdSecInput.Solution(this, DA, 0);

      IStrengthResult uls = null;
      IServiceabilityResult sls = null;

      // get load - can be either load or deformation
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(1, ref gh_typ)) {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecLoadGoo load) {
          uls = solution.Value.Strength.Check(load.Value);
          sls = solution.Value.Serviceability.Check(load.Value);
        } else if (gh_typ.Value is AdSecDeformationGoo def) {
          uls = solution.Value.Strength.Check(def.Value);
          sls = solution.Value.Serviceability.Check(def.Value);
        } else {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unable to convert {Params.Input[1].NickName} to AdSec Load");
          return;
        }
      } else {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {Params.Input[1].NickName} failed to collect data!");
        return;
      }

      var point = this.GetAdSecPointGoo(DA, 2).AdSecPoint;
      // ULS strain
      var strainULS = uls.Deformation.StrainAt(point);
      var outStrainULS = new GH_UnitNumber(strainULS.ToUnit(DefaultUnits.StrainUnitResult));
      DA.SetData(0, outStrainULS);

      // ULS stress in concrete material from strain
      var stressULS = solution.m_section.Section.Material.Strength.StressAt(strainULS);
      var outStressULS = new GH_UnitNumber(stressULS.ToUnit(DefaultUnits.StressUnitResult));
      DA.SetData(1, outStressULS);

      // SLS strain
      var strainSLS = sls.Deformation.StrainAt(point);
      var outStrainSLS = new GH_UnitNumber(strainSLS.ToUnit(DefaultUnits.StrainUnitResult));
      DA.SetData(2, outStrainSLS);

      // SLS stress in concrete material from strain
      var stressSLS = solution.m_section.Section.Material.Serviceability.StressAt(strainSLS);
      var outStressSLS = new GH_UnitNumber(stressSLS.ToUnit(DefaultUnits.StressUnitResult));

      DA.SetData(3, outStressSLS);
    }
  }
}
