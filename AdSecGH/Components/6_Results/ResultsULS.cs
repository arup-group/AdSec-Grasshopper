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
using OasysGH.Parameters;
using OasysGH.Units;
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;

namespace AdSecGH.Components {
  public class ResultsULS : GH_OasysComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("146bd264-66ac-4484-856f-8557be762a33");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.ULS;
    private Line _failureLine;

    public ResultsULS() : base("Strength Result",
      "ULS",
      "Performs strength checks (ULS), for a given Load or Deformation.",
      CategoryName.Name(),
      SubCategoryName.Cat7()) {
      Hidden = false;  // sets the initial state of the component to hidden
    }

    public override void DrawViewportWires(IGH_PreviewArgs args) {
      base.DrawViewportWires(args);
      if (_failureLine != null) {
        args.Display.DrawDottedLine(_failureLine, Attributes.Selected ? args.WireColour_Selected : args.WireColour);
      }
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform strenght check on.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      string strainUnitAbbreviation = Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      IQuantity curvature = new Curvature(0, DefaultUnits.CurvatureUnit);
      string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
      IQuantity moment = new Moment(0, DefaultUnits.MomentUnit);
      string momentUnitAbbreviation = string.Concat(moment.ToString().Where(char.IsLetter));

      pManager.AddGenericParameter("Load", "Ld", "The section load under the applied action." +
          Environment.NewLine + "If the applied deformation is outside the capacity range of the section, the returned load will be zero.", GH_ParamAccess.item);

      pManager.AddNumberParameter("LoadUtil", "Ul", "The strength load utilisation is the ratio of the applied load to the load in the same direction that would cause the " +
          "section to reach its capacity. Utilisation > 1 means the applied load exceeds the section capacity." +
          Environment.NewLine + "If the applied load is outside the capacity range of the section, the utilisation will be greater than 1. Whereas, if the applied " +
          "deformation exceeds the capacity, the load utilisation will be zero.", GH_ParamAccess.item);

      pManager.AddVectorParameter("Deformation", "Def", "The section deformation under the applied action. The output is a vector representing:"
          + Environment.NewLine + "X: Strain [" + strainUnitAbbreviation + "],"
          + Environment.NewLine + "Y: Curvature around zz (so in local y-direction) [" + curvatureUnitAbbreviation + "],"
          + Environment.NewLine + "Z: Curvature around yy (so in local z-direction) [" + curvatureUnitAbbreviation + "]", GH_ParamAccess.item);

      pManager.AddNumberParameter("DeformationUtil", "Ud", "The strength deformation utilisation is the ratio of the applied deformation to the deformation in the same direction" +
          " that would cause the section to reach its capacity. Utilisation > 1 means capacity has been exceeded." +
          Environment.NewLine + "Capacity has been exceeded when the utilisation is greater than 1. If the applied load is outside the capacity range of the section, the " +
          "deformation utilisation will be the maximum double value.", GH_ParamAccess.item);

      pManager.AddIntervalParameter("Moment Ranges", "MRs", "The range of moments (in the direction of the applied moment, assuming constant axial force) that are within the " +
                "section's capacity. Moment values are in [" + momentUnitAbbreviation + "]", GH_ParamAccess.list);

      pManager.AddLineParameter("Neutral Axis", "NaL", "Line of Neutral Axis", GH_ParamAccess.item);

      pManager.AddGenericParameter("Neutral Axis Offset", "NaO", "The Offset of the Neutral Axis from the Sections centroid", GH_ParamAccess.item);
      pManager.AddNumberParameter("Neutral Axis Angle", "NaA", "The Angle [rad] of the Neutral Axis from the Sections centroid", GH_ParamAccess.item);

      pManager.AddVectorParameter("Failure Deformation", "DU", "The section deformation at failure. The output is a vector representing:"
          + Environment.NewLine + "X: Strain [" + strainUnitAbbreviation + "],"
          + Environment.NewLine + "Y: Curvature around zz (so in local y-direction) [" + curvatureUnitAbbreviation + "],"
          + Environment.NewLine + "Z: Curvature around yy (so in local z-direction) [" + curvatureUnitAbbreviation + "]", GH_ParamAccess.item);

      pManager.AddLineParameter("Failure Neutral Axis", "FaL", "Line of Neutral Axis at failure", GH_ParamAccess.item);
      pManager.HideParameter(9);

      pManager.AddGenericParameter("Failure Neutral Axis Offset", "FaO", "The Offset of the Neutral Axis at failure from the Sections centroid", GH_ParamAccess.item);
      pManager.AddNumberParameter("Failure Neutral Axis Angle", "FaA", "The Angle [rad] of the Neutral Axis at failure from the Sections centroid", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get solution input
      AdSecSolutionGoo solution = AdSecInput.Solution(this, DA, 0);

      IStrengthResult uls = null;
      IStrengthResult failure = null;

      // get load - can be either load or deformation
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(1, ref gh_typ)) {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecLoadGoo load) {
          uls = solution.Value.Strength.Check(load.Value);
          var failureLoad = ILoad.Create(
            load.Value.X / uls.LoadUtilisation.DecimalFractions * 0.999,
            load.Value.YY / uls.LoadUtilisation.DecimalFractions * 0.999,
            load.Value.ZZ / uls.LoadUtilisation.DecimalFractions * 0.999);
          failure = solution.Value.Strength.Check(failureLoad);
        } else if (gh_typ.Value is AdSecDeformationGoo def) {
          uls = solution.Value.Strength.Check(def.Value);
          var failureLoad = IDeformation.Create(
            def.Value.X / uls.LoadUtilisation.DecimalFractions * 0.999,
            def.Value.YY / uls.LoadUtilisation.DecimalFractions * 0.999,
            def.Value.ZZ / uls.LoadUtilisation.DecimalFractions * 0.999);
          failure = solution.Value.Strength.Check(failureLoad);
        } else {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
          return;
        }
      } else {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
        return;
      }

      DA.SetData(0, new AdSecLoadGoo(uls.Load, solution.LocalPlane));
      double util = uls.LoadUtilisation.As(RatioUnit.DecimalFraction);
      DA.SetData(1, util);
      if (util > 1) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Utilisation is above 1!");
      }

      IDeformation ulsDeformationResult = uls.Deformation;
      DA.SetData(2, new Vector3d(
          ulsDeformationResult.X.As(DefaultUnits.StrainUnitResult),
          ulsDeformationResult.YY.As(DefaultUnits.CurvatureUnit),
          ulsDeformationResult.ZZ.As(DefaultUnits.CurvatureUnit)));
      double defUtil = uls.DeformationUtilisation.As(RatioUnit.DecimalFraction);
      DA.SetData(3, defUtil);
      if (defUtil > 1) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Deformation utilisation is above 1!");
      }

      var momentRanges = new List<GH_Interval>();
      foreach (IMomentRange mrng in uls.MomentRanges) {
        var interval = new Interval(
            mrng.Min.As(DefaultUnits.MomentUnit),
            mrng.Max.As(DefaultUnits.MomentUnit));
        momentRanges.Add(new GH_Interval(interval));
      }
      DA.SetDataList(4, momentRanges);

      // calculate neutral line
      Length offset = CalculateOffset(ulsDeformationResult);
      double angleRadians = CalculateAngle(ulsDeformationResult);

      DA.SetData(5, CreateNeutralLine(offset, angleRadians, solution.LocalPlane, solution.ProfileEdge));
      DA.SetData(6, new GH_UnitNumber(offset));
      DA.SetData(7, angleRadians);

      IDeformation failureDeformationResult = failure.Deformation;
      DA.SetData(8, new Vector3d(
          failureDeformationResult.X.As(DefaultUnits.StrainUnitResult),
          failureDeformationResult.YY.As(DefaultUnits.CurvatureUnit),
          failureDeformationResult.ZZ.As(DefaultUnits.CurvatureUnit)));
      Length offsetFailure = CalculateOffset(failureDeformationResult);
      double angleRadiansFailure = CalculateAngle(failureDeformationResult);

      _failureLine = CreateNeutralLine(offsetFailure, angleRadiansFailure, solution.LocalPlane, solution.ProfileEdge);
      DA.SetData(9, _failureLine);
      DA.SetData(10, new GH_UnitNumber(offsetFailure));
      DA.SetData(11, angleRadiansFailure);
    }

    private double CalculateAngle(IDeformation ulsDeformationResult) {
      double kYY = ulsDeformationResult.YY.As(CurvatureUnit.PerMeter);
      double kZZ = ulsDeformationResult.ZZ.As(CurvatureUnit.PerMeter);
      return Math.Atan2(kZZ, kYY);
    }

    private Length CalculateOffset(IDeformation ulsDeformationResult) {
      // neutral line
      double defX = ulsDeformationResult.X.As(StrainUnit.Ratio);
      double kYY = ulsDeformationResult.YY.As(CurvatureUnit.PerMeter);
      double kZZ = ulsDeformationResult.ZZ.As(CurvatureUnit.PerMeter);

      // compute offset
      double offsetSI = -defX / Math.Sqrt(Math.Pow(kYY, 2) + Math.Pow(kZZ, 2));
      if (double.IsNaN(offsetSI)) {
        offsetSI = 0.0;
      }

      // temp length in SI units
      var tempOffset = new Length(offsetSI, DefaultUnits.LengthUnitResult);

      // offset in user selected unit
      return tempOffset;
    }

    private Line CreateNeutralLine(Length offset, double angleRadians, Plane local, Polyline profile) {
      // calculate temp plane for width of neutral line
      Plane tempPlane = local.Clone();
      tempPlane.Rotate(angleRadians, tempPlane.ZAxis);
      // get profile's bounding box in rotate plane
      Curve tempCrv = profile.ToPolylineCurve();
      BoundingBox bbox = tempCrv.GetBoundingBox(tempPlane);

      // calculate width of neutral line to display
      double width = 1.05 * bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(1, 0, 0));

      // get direction as vector
      var direction = new Vector3d(local.XAxis);
      direction.Rotate(angleRadians, local.ZAxis);
      direction.Unitize();

      // starting point for rotated line
      var start = new Point3d(local.Origin);
      start.Transform(Transform.Translation(direction.X * width / 2 * -1, direction.Y * width / 2 * -1, direction.Z * width / 2 * -1));
      var ln = new Line(start, direction, width);

      // offset vector
      var offsVec = new Vector3d(direction);
      offsVec.Rotate(Math.PI / 2, local.ZAxis);
      offsVec.Unitize();
      // move the line
      double off = offset.As(DefaultUnits.LengthUnitResult);
      ln.Transform(Transform.Translation(offsVec.X * off, offsVec.Y * off, offsVec.Z * off));
      return ln;
    }
  }
}
