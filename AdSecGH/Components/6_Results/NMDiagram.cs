using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace AdSecGH.Components {
  public class NMDiagram : GH_OasysDropDownComponent {
    private enum FoldMode {
      NM,
      MM
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("21cd9e4c-6c85-4077-b575-1e04127f2998");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.N_M;
    private AngleUnit _angleUnit = AngleUnit.Radian;
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private FoldMode _mode = FoldMode.NM;

    public NMDiagram() : base(
      "N-M Diagram",
      "N-M",
      "Calculates a force-moment (N-M) or moment-moment (M-M) interaction curve.",
      CategoryName.Name(),
      SubCategoryName.Cat7()) {
      this.Hidden = false; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      this._selectedItems[i] = this._dropDownItems[i][j];

      if (i == 0) {
        switch (this._selectedItems[0]) {
          case ("N-M"):
            this._mode = FoldMode.NM;
            this._dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle);
            this._selectedItems[1] = _angleUnit.ToString();
            break;

          case ("M-M"):
            this._mode = FoldMode.MM;
            this._dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
            this._selectedItems[1] = _forceUnit.ToString();
            break;
        }
      }
      else {
        switch (this._selectedItems[0]) {
          case ("N-M"):
            _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), this._selectedItems[i]);
            break;

          case ("M-M"):
            _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), this._selectedItems[i]);
            break;
        }
      }
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      switch (this._selectedItems[0]) {
        case ("N-M"):
          string angleUnitAbbreviation = Angle.GetAbbreviation(this._angleUnit);
          Params.Input[1].Name = "Moment Angle [" + angleUnitAbbreviation + "]";
          Params.Input[1].NickName = "A";
          Params.Input[1].Description = "[Default 0] The moment angle, which must be in the range -180 degrees to +180 degrees. Angle of zero equals Nx-Myy diagram.";
          Params.Output[0].Name = "N-M Curve";
          Params.Output[0].NickName = "NM";
          Params.Output[0].Description = "AdSec Force-Moment (N-M) interaction diagram";
          break;

        case ("M-M"):
          string forceUnitAbbreviation = Force.GetAbbreviation(this._forceUnit);
          Params.Input[1].Name = "Axial Force [" + forceUnitAbbreviation + "]";
          Params.Input[1].NickName = "F";
          Params.Input[1].Description = "[Default 0] The axial force to calculate the moment-moment diagram for.";
          Params.Output[0].Name = "M-M Curve";
          Params.Output[0].NickName = "MM";
          Params.Output[0].Description = "AdSec Moment-Moment (M-M) interaction diagram";
          break;
      }
    }

    protected override void InitialiseDropdowns() {
      this._spacerDescriptions = new List<string>(new string[] {
        "Interaction",
        "Measure"
      });

      this._dropDownItems = new List<List<string>>();
      this._selectedItems = new List<string>();

      // type
      this._dropDownItems.Add(new List<string>() { "N-M", "M-M" });
      this._selectedItems.Add(this._dropDownItems[0][0]);

      // force
      this._dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle));
      this._selectedItems.Add(Angle.GetAbbreviation(this._angleUnit));

      this._isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string angleUnitAbbreviation = Angle.GetAbbreviation(this._angleUnit);
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to calculate interaction diagram from", GH_ParamAccess.item);
      pManager.AddGenericParameter("Moment Angle [" + angleUnitAbbreviation + "]", "A", "[Default 0] The moment angle, which must be in the range -180 degrees to +180 degrees. Angle of zero equals Nx-Myy diagram.", GH_ParamAccess.item);
      pManager[1].Optional = true;
      // create default rectangle as 1/2 meter square
      Length sz = Length.FromMeters(0.5);
      Rectangle3d rect = new Rectangle3d(Plane.WorldXY, sz.As(DefaultUnits.LengthUnitGeometry), sz.As(DefaultUnits.LengthUnitGeometry));
      pManager.AddRectangleParameter("Plot", "R", "Rectangle for plot boundary", GH_ParamAccess.item, rect);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("N-M Curve", "NM", "AdSec Force-Moment (N-M) interaction diagram", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get solution input
      AdSecSolutionGoo solution = AdSecInput.Solution(this, DA, 0);

      // Get boundary input
      Rectangle3d rect = new Rectangle3d();
      if (!DA.GetData(2, ref rect)) {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[2].NickName + " to Rectangle");
        return;
      }

      if (this._mode == FoldMode.NM) {
        // get angle input
        Angle angle = (Angle)Input.UnitNumber(this, DA, 1, _angleUnit, true);

        // get loadcurve
        Oasys.Collections.IList<Oasys.AdSec.Mesh.ILoadCurve> loadCurve = solution.Value.Strength.GetForceMomentInteractionCurve(angle);

        // create output
        DA.SetData(0, new AdSecNMMCurveGoo(loadCurve[0], angle, rect));
      }
      else {
        // get force input
        Force force = (Force)Input.UnitNumber(this, DA, 1, _forceUnit, true);

        // get loadcurve
        Oasys.Collections.IList<Oasys.AdSec.Mesh.ILoadCurve> loadCurve = solution.Value.Strength.GetMomentMomentInteractionCurve(force);

        // check if curve is valid
        if (loadCurve.Count == 0) {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input axial force is outside the capacity range of the section");
          return;
        }

        // create output
        DA.SetData(0, new AdSecNMMCurveGoo(loadCurve[0], rect));
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      switch (this._selectedItems[0]) {
        case ("N-M"):
          this._mode = FoldMode.NM;
          this._angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), this._selectedItems[1]);
          break;

        case ("M-M"):
          this._mode = FoldMode.MM;
          this._forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), this._selectedItems[1]);
          break;
      }
      base.UpdateUIFromSelectedItems();
    }
  }
}
