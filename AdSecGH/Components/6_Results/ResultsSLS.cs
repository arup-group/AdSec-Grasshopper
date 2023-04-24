using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using OasysGH;
using OasysGH.Components;
using OasysGH.Units;
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;

namespace AdSecGH.Components {
  public class ResultsSLS : GH_OasysComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("27ba3ec5-b94c-43ad-8623-087540413628");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.SLS;

    public ResultsSLS() : base(
      "Serviceability Result",
      "SLS",
      "Performs serviceability analysis (SLS), for a given Load or Deformation.",
      CategoryName.Name(),
      SubCategoryName.Cat7()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      string strainUnitAbbreviation = Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      IQuantity curvature = new Curvature(0, DefaultUnits.CurvatureUnit);
      string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
      IQuantity axial = new AxialStiffness(0, DefaultUnits.AxialStiffnessUnit);
      string axialUnitAbbreviation = string.Concat(axial.ToString().Where(char.IsLetter));
      IQuantity bending = new BendingStiffness(0, DefaultUnits.BendingStiffnessUnit);
      string bendingUnitAbbreviation = string.Concat(bending.ToString().Where(char.IsLetter));
      IQuantity moment = new Moment(0, DefaultUnits.MomentUnit);
      string momentUnitAbbreviation = string.Concat(moment.ToString().Where(char.IsLetter));

      pManager.AddGenericParameter("Load", "Ld", "The section load under the applied action." +
          Environment.NewLine + "If the applied deformation is outside the capacity range of the section, the returned load will be zero.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Cracks", "Cks", "Crack results are calculated at bar positions or section surfaces depending on the Design Code specifications." +
          Environment.NewLine + "If the applied action is outside the capacity range of the section, the returned list will be empty. See MaximumCrack output " +
          "for the crack result corresponding to the maximum crack width.", GH_ParamAccess.item);

      pManager.AddGenericParameter("MaximumCrack", "Crk", "The crack result from Cracks that corresponds to the maximum crack width." +
          Environment.NewLine + "If the applied action is outside the capacity range of the section, the returned maximum width crack result will be maximum " +
          "double value.", GH_ParamAccess.item);

      pManager.AddNumberParameter("CrackUtil", "Uc", "The ratio of the applied load (moment and axial) to the load (moment and axial) in the same direction that would " +
          "cause the section to crack. Ratio > 1 means section is cracked." +
          Environment.NewLine + "The section is cracked when the cracking utilisation ratio is greater than 1. If the applied load is outside the capacity range" +
          " of the section, the cracking utilisation will be maximum double value.", GH_ParamAccess.item);

      pManager.AddVectorParameter("Deformation", "Def", "The section deformation under the applied action. The output is a vector representing:"
          + Environment.NewLine + "X: Strain [" + strainUnitAbbreviation + "],"
          + Environment.NewLine + "Y: Curvature around zz (so in local y-direction) [" + curvatureUnitAbbreviation + "],"
          + Environment.NewLine + "Z: Curvature around yy (so in local z-direction) [" + curvatureUnitAbbreviation + "]", GH_ParamAccess.item);

      pManager.AddVectorParameter("SecantStiffness", "Es", "The secant stiffness under the applied action. The output is a vector representing:"
          + Environment.NewLine + "X: Axial stiffness [" + axialUnitAbbreviation + "],"
          + Environment.NewLine + "Y: The bending stiffness about the y-axis in the local coordinate system [" + bendingUnitAbbreviation + "],"
          + Environment.NewLine + "Z: The bending stiffness about the z-axis in the local coordinate system [" + bendingUnitAbbreviation + "]", GH_ParamAccess.item);

      pManager.AddIntervalParameter("Uncracked Moment Ranges", "MRs", "The range of moments (in the direction of the applied moment, assuming constant axial force) " +
          "over which the section remains uncracked. Moment values are in [" + momentUnitAbbreviation + "]", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get solution input
      AdSecSolutionGoo solution = AdSecInput.Solution(this, DA, 0);

      IServiceabilityResult sls = null;

      // get load - can be either load or deformation
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(1, ref gh_typ)) {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecLoadGoo load) {
          sls = solution.Value.Serviceability.Check(load.Value);
        } else if (gh_typ.Value is AdSecDeformationGoo def) {
          sls = solution.Value.Serviceability.Check(def.Value);
        } else {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
          return;
        }
      } else {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
        return;
      }

      DA.SetData(0, new AdSecLoadGoo(sls.Load, solution.LocalPlane));

      var cracks = new List<AdSecCrackGoo>();
      foreach (ICrack crack in sls.Cracks) {
        cracks.Add(new AdSecCrackGoo(crack, solution.LocalPlane));
      }

      DA.SetDataList(1, cracks);

      if (sls.MaximumWidthCrack != null && sls.MaximumWidthCrack.Width.Meters < 1) {
        DA.SetData(2, new AdSecCrackGoo(sls.MaximumWidthCrack, solution.LocalPlane));
      }

      double util = sls.CrackingUtilisation.As(RatioUnit.DecimalFraction);
      DA.SetData(3, util);
      if (util > 1) {
        if (cracks.Count == 0) {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The section is failing and the cracks are so large we can't even compute them!");
        } else {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "The section is cracked");
        }
      }

      DA.SetData(4, new Vector3d(
        sls.Deformation.X.As(DefaultUnits.StrainUnitResult),
        sls.Deformation.YY.As(DefaultUnits.CurvatureUnit),
        sls.Deformation.ZZ.As(DefaultUnits.CurvatureUnit)));

      DA.SetData(5, new Vector3d(
        sls.SecantStiffness.X.As(DefaultUnits.AxialStiffnessUnit),
        sls.SecantStiffness.YY.As(DefaultUnits.BendingStiffnessUnit),
        sls.SecantStiffness.ZZ.As(DefaultUnits.BendingStiffnessUnit)));

      var momentRanges = new List<GH_Interval>();
      foreach (IMomentRange mrng in sls.UncrackedMomentRanges) {
        var interval = new Interval(
          mrng.Min.As(DefaultUnits.MomentUnit),
          mrng.Max.As(DefaultUnits.MomentUnit));
        momentRanges.Add(new GH_Interval(interval));
      }
      DA.SetDataList(6, momentRanges);
    }
  }
}
