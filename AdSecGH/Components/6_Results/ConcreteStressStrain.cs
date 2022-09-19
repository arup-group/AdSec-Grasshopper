using System;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using AdSecGH.Parameters;
using Oasys.AdSec;
using Oasys.Units;
using OasysGH;
using OasysGH.Components;
using UnitsNet;
using UnitsNet.GH;

namespace AdSecGH.Components
{
  public class ConcreteStressStrain : GH_OasysComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("542fc96d-d90a-4301-855f-d14507cc9753");
    public ConcreteStressStrain()
      : base("Concrete Stress/Strain", "CSS", "Calculate the Concrete Stress/Strain at a point on the Section for a given Load or Deformation.",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat7())
    { this.Hidden = true; } // sets the initial state of the component to hidden

    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainConcrete;

    public override OasysPluginInfo PluginInfo => AdSecGHPluginInfo.Instance;
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    #endregion

    #region Input and output

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Vertex Point", "Vx", "A 2D vertex in the section's local yz-plane for where to calculate strain.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      string strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(Units.StrainUnit);
      IQuantity stress = new Pressure(0, Units.StressUnit);
      string stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));

      pManager.AddGenericParameter("ULS Strain [" + strainUnitAbbreviation + "]", "εd", "ULS strain at Vertex Point", GH_ParamAccess.item);
      pManager.AddGenericParameter("ULS Stress [" + stressUnitAbbreviation + "]", "σd", "ULS stress at Vertex Point", GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Strain [" + strainUnitAbbreviation + "]", "εk", "SLS strain at Vertex Point", GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Stress [" + stressUnitAbbreviation + "]", "σk", "SLS stress at Vertex Point", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // get solution input
      AdSecSolutionGoo solution = GetInput.Solution(this, DA, 0);

      IStrengthResult uls = null;
      IServiceabilityResult sls = null;

      // get load - can be either load or deformation
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(1, ref gh_typ))
      {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecLoadGoo)
        {
          AdSecLoadGoo load = (AdSecLoadGoo)gh_typ.Value;
          uls = solution.Value.Strength.Check(load.Value);
          sls = solution.Value.Serviceability.Check(load.Value);
        }
        else if (gh_typ.Value is AdSecDeformationGoo)
        {
          AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typ.Value;
          uls = solution.Value.Strength.Check(def.Value);
          sls = solution.Value.Serviceability.Check(def.Value);
        }
        else
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
          return;
        }
      }
      else
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
        return;
      }

      // ULS strain
      Strain strainULS = uls.Deformation.StrainAt(GetInput.IPoint(this, DA, 2, false));
      GH_UnitNumber outStrainULS = new GH_UnitNumber(new Strain(strainULS.As(Units.StrainUnit), Units.StrainUnit));
      DA.SetData(0, outStrainULS);

      // ULS stress in concrete material from strain
      Pressure stressULS = solution.m_section.Section.Material.Strength.StressAt(strainULS);
      GH_UnitNumber outStressULS = new GH_UnitNumber(new Pressure(stressULS.As(Units.StressUnit), Units.StressUnit));
      DA.SetData(1, outStressULS);

      // SLS strain
      Strain strainSLS = sls.Deformation.StrainAt(GetInput.IPoint(this, DA, 2, false));
      GH_UnitNumber outStrainSLS = new GH_UnitNumber(new Strain(strainSLS.As(Units.StrainUnit), Units.StrainUnit));
      DA.SetData(2, outStrainSLS);

      // SLS stress in concrete material from strain
      Pressure stressSLS = solution.m_section.Section.Material.Serviceability.StressAt(strainSLS);
      GH_UnitNumber outStressSLS = new GH_UnitNumber(new Pressure(stressSLS.As(Units.StressUnit), Units.StressUnit));

      DA.SetData(3, outStressSLS);
    }
  }
}
