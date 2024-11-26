using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.Profiles;

using OasysGH;
using OasysGH.Components;
using OasysGH.Parameters;
using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Components {
  public class RebarStressStrain : GH_OasysComponent {

    public RebarStressStrain() : base("Rebar Stress/Strain", "RSS",
      "Calculate the Rebar Stress/Strains in the Section for a given Load or Deformation.", CategoryName.Name(),
      SubCategoryName.Cat7()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bb9fe65b-76e1-466b-be50-3dd9c7a3283f");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainRebar;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Load", "Ld",
        "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      string lengthUnitAbbreviation = Length.GetAbbreviation(DefaultUnits.LengthUnitResult);
      string strainUnitAbbreviation = Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(DefaultUnits.StressUnitResult);
      pManager.AddGenericParameter("Position [" + lengthUnitAbbreviation + "]", "Vx",
        "Rebar position as 2D vertex in the section's local yz-plane ", GH_ParamAccess.list);
      pManager.AddGenericParameter("ULS Strain [" + strainUnitAbbreviation + "]", "εd",
        "ULS strain for each rebar position", GH_ParamAccess.list);
      pManager.AddGenericParameter("ULS Stress [" + stressUnitAbbreviation + "]", "σd",
        "ULS stress for each rebar position", GH_ParamAccess.list);
      pManager.AddGenericParameter("SLS Strain [" + strainUnitAbbreviation + "]", "εk",
        "SLS strain for each rebar position", GH_ParamAccess.list);
      pManager.AddGenericParameter("SLS Stress [" + stressUnitAbbreviation + "]", "σk",
        "SLS stress for each rebar position", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get solution input
      var solution = AdSecInput.Solution(this, DA, 0);

      IStrengthResult uls;
      IServiceabilityResult sls;

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
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
            "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
          return;
        }
      } else {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
        return;
      }

      // create flattened section to extract rebars
      var flat = FlatSection(solution);

      var pointGoos = new List<AdSecPointGoo>();
      var outStressULS = new List<GH_UnitNumber>();
      var outStrainULS = new List<GH_UnitNumber>();
      var outStrainSLS = new List<GH_UnitNumber>();
      var outStressSLS = new List<GH_UnitNumber>();

      // loop through rebar groups in flattened section
      foreach (var rebargrp in flat.ReinforcementGroups) {
        try // first try if not a link group type
        {
          switch (rebargrp) {
            case ISingleBars singleBars:
              var positions = singleBars.Positions;

              if (positions == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Positions are null for the rebar group.");
                continue;
              }

              foreach (var pos in positions) {
                // position
                pointGoos.Add(new AdSecPointGoo(pos));

                // ULS strain
                var strainULS = uls.Deformation.StrainAt(pos);
                outStrainULS.Add(new GH_UnitNumber(strainULS.ToUnit(DefaultUnits.StrainUnitResult)));

                // ULS stress in bar material from strain
                var stressULS = singleBars.BarBundle.Material.Strength.StressAt(strainULS);
                outStressULS.Add(new GH_UnitNumber(stressULS.ToUnit(DefaultUnits.StressUnitResult)));

                // SLS strain
                var strainSLS = sls.Deformation.StrainAt(pos);
                outStrainSLS.Add(new GH_UnitNumber(strainSLS.ToUnit(DefaultUnits.StrainUnitResult)));

                // SLS stress in bar material from strain
                var stressSLS = singleBars.BarBundle.Material.Serviceability.StressAt(strainSLS);
                outStressSLS.Add(new GH_UnitNumber(stressSLS.ToUnit(DefaultUnits.StressUnitResult)));
              }

              break;
          }
        } catch (Exception) {
          // do nothing if rebar is link
        }
      }

      DA.SetDataList(0, pointGoos);
      DA.SetDataList(1, outStrainULS);
      DA.SetDataList(2, outStressULS);
      DA.SetDataList(3, outStrainSLS);
      DA.SetDataList(4, outStressSLS);
    }

    private static ISection FlatSection(AdSecSolutionGoo solution) {
      ISection flat;
      if (solution.m_section.DesignCode != null) {
        var adSec = IAdSec.Create(solution.m_section.DesignCode);
        flat = adSec.Flatten(solution.m_section.Section);
      } else {
        var prof = IPerimeterProfile.Create(solution.m_section.Section.Profile);
        flat = ISection.Create(prof, solution.m_section.Section.Material);
      }

      return flat;
    }
  }
}
