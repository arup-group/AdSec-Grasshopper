using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using Oasys.Units;
using OasysGH.Components;
using Rhino.Geometry;
using UnitsNet;
using OasysGH;

namespace AdSecGH.Components
{
  public class ResultsULS : GH_OasysComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("1337cc01-0b76-4f58-b24e-81e32ae24f92");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.ULS;

    public ResultsULS()
      : base("Strength Result", "ULS", "Performs strength checks (ULS), for a given Load or Deformation.",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat7())
    { this.Hidden = false; } // sets the initial state of the component to hidden
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    #endregion

    #region Input and output

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform strenght check on.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(Units.StrainUnit);
      IQuantity curvature = new Curvature(0, Units.CurvatureUnit);
      string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
      IQuantity moment = new Moment(0, Units.MomentUnit);
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

      pManager.AddLineParameter("Neutral Axis", "NAx", "Line of Neutral Axis", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // get solution input
      AdSecSolutionGoo solution = GetInput.Solution(this, DA, 0);

      IStrengthResult uls = null;

      // get load - can be either load or deformation
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(1, ref gh_typ))
      {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecLoadGoo)
        {
          AdSecLoadGoo load = (AdSecLoadGoo)gh_typ.Value;
          uls = solution.Value.Strength.Check(load.Value);
        }
        else if (gh_typ.Value is AdSecDeformationGoo)
        {
          AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typ.Value;
          uls = solution.Value.Strength.Check(def.Value);
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

      DA.SetData(0, new AdSecLoadGoo(uls.Load, solution.LocalPlane));
      double util = uls.LoadUtilisation.As(RatioUnit.DecimalFraction);
      DA.SetData(1, util);
      if (util > 1)
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Utilisation is above 1!");

      IDeformation ulsDeformationResult = uls.Deformation;
      DA.SetData(2, new Vector3d(
          ulsDeformationResult.X.As(Units.StrainUnit),
          ulsDeformationResult.YY.As(Units.CurvatureUnit),
          ulsDeformationResult.ZZ.As(Units.CurvatureUnit)));
      double defUtil = uls.DeformationUtilisation.As(RatioUnit.DecimalFraction);
      DA.SetData(3, defUtil);
      if (defUtil > 1)
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Deformation utilisation is above 1!");

      List<GH_Interval> momentRanges = new List<GH_Interval>();
      foreach (IMomentRange mrng in uls.MomentRanges)
      {
        Interval interval = new Interval(
            mrng.Min.As(Units.MomentUnit),
            mrng.Max.As(Units.MomentUnit));
        momentRanges.Add(new GH_Interval(interval));
      }
      DA.SetDataList(4, momentRanges);

      DA.SetData(5, CreateNeutralLine(ulsDeformationResult, solution.LocalPlane, solution.ProfileEdge));
    }

    private Line CreateNeutralLine(IDeformation ulsDeformationResult, Plane local, Polyline profile)
    {
      // neutral line
      double defX = ulsDeformationResult.X.As(StrainUnit.Ratio);
      double kYY = ulsDeformationResult.YY.As(CurvatureUnit.PerMeter);
      double kZZ = ulsDeformationResult.ZZ.As(CurvatureUnit.PerMeter);

      // compute offset
      double offsetSI = -defX / Math.Sqrt(Math.Pow(kYY, 2) + Math.Pow(kZZ, 2));
      if (double.IsNaN(offsetSI))
        offsetSI = 0.0;

      // temp length in SI units
      Length tempOffset = new Length(offsetSI, LengthUnit.Meter);

      // offset in user selected unit
      Length offset = new Length(tempOffset.As(Units.LengthUnit), Units.LengthUnit);

      // compute angle
      double angleRadians = Math.Atan2(kZZ, kYY);
      // temp angle in radians
      Angle tempAngle = new Angle(angleRadians, AngleUnit.Radian);

      // calculate temp plane for width of neutral line
      Plane tempPlane = local.Clone();
      tempPlane.Rotate(angleRadians, tempPlane.ZAxis);
      // get profile's bounding box in rotate plane
      Curve tempCrv = profile.ToPolylineCurve();
      BoundingBox bbox = tempCrv.GetBoundingBox(tempPlane);

      // calculate width of neutral line to display
      double width = 1.05 * bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(1, 0, 0));

      // get direction as vector
      Vector3d direction = new Vector3d(local.XAxis);
      direction.Rotate(angleRadians, local.ZAxis);
      direction.Unitize();

      // starting point for rotated line
      Point3d start = new Point3d(local.Origin);
      start.Transform(Transform.Translation(direction.X * width / 2 * -1, direction.Y * width / 2 * -1, direction.Z * width / 2 * -1));
      Line ln = new Line(start, direction, width);

      // offset vector
      Vector3d offsVec = new Vector3d(direction);
      offsVec.Rotate(Math.PI / 2, local.ZAxis);
      offsVec.Unitize();
      // move the line
      double off = offset.As(Units.LengthUnit);
      ln.Transform(Transform.Translation(offsVec.X * off, offsVec.Y * off, offsVec.Z * off));
      return ln;
    }
  }
}
