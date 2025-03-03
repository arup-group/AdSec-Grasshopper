using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Components {
  public class NMDiagram : GH_OasysDropDownComponent {
    private AngleUnit _angleUnit = AngleUnit.Radian;
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private FoldMode _mode = FoldMode.NM;

    public NMDiagram() : base("N-M Diagram", "N-M",
      "Calculates a force-moment (N-M) or moment-moment (M-M) interaction curve.", CategoryName.Name(),
      SubCategoryName.Cat7()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("21cd9e4c-6c85-4077-b575-1e04127f2998");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.N_M;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      if (i == 0) {
        switch (_selectedItems[0]) {
          case "N-M":
            _mode = FoldMode.NM;
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle);
            _selectedItems[1] = _angleUnit.ToString();
            break;

          case "M-M":
            _mode = FoldMode.MM;
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
            _selectedItems[1] = _forceUnit.ToString();
            break;
        }
      } else {
        switch (_selectedItems[0]) {
          case "N-M":
            _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[i]);
            break;

          case "M-M":
            _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), _selectedItems[i]);
            break;
        }
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      switch (_selectedItems[0]) {
        case "N-M":
          string angleUnitAbbreviation = Angle.GetAbbreviation(_angleUnit);
          Params.Input[1].Name = $"Moment Angle [{angleUnitAbbreviation}]";
          Params.Input[1].NickName = "A";
          Params.Input[1].Description
            = "[Default 0] The moment angle, which must be in the range -180 degrees to +180 degrees. Angle of zero equals Nx-Myy diagram.";
          Params.Output[0].Name = "N-M Curve";
          Params.Output[0].NickName = "NM";
          Params.Output[0].Description = "AdSec Force-Moment (N-M) interaction diagram";
          break;

        case "M-M":
          string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
          Params.Input[1].Name = $"Axial Force [{forceUnitAbbreviation}]";
          Params.Input[1].NickName = "F";
          Params.Input[1].Description = "[Default 0] The axial force to calculate the moment-moment diagram for.";
          Params.Output[0].Name = "M-M Curve";
          Params.Output[0].NickName = "MM";
          Params.Output[0].Description = "AdSec Moment-Moment (M-M) interaction diagram";
          break;
      }
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Interaction",
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // type
      _dropDownItems.Add(new List<string> {
        "N-M",
        "M-M",
      });
      _selectedItems.Add(_dropDownItems[0][0]);

      // force
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle));
      _selectedItems.Add(Angle.GetAbbreviation(_angleUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string angleUnitAbbreviation = Angle.GetAbbreviation(_angleUnit);
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to calculate interaction diagram from",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"Moment Angle [{angleUnitAbbreviation}]", "A",
        "[Default 0] The moment angle, which must be in the range -180 degrees to +180 degrees. Angle of zero equals Nx-Myy diagram.",
        GH_ParamAccess.item);
      pManager[1].Optional = true;
      // create default rectangle as 1/2 meter square
      var length = Length.FromMeters(0.5);
      var rect = new Rectangle3d(Plane.WorldXY, length.As(DefaultUnits.LengthUnitGeometry),
        length.As(DefaultUnits.LengthUnitGeometry));
      pManager.AddRectangleParameter("Plot", "R", "Rectangle for plot boundary", GH_ParamAccess.item, rect);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("N-M Curve", "NM", "AdSec Force-Moment (N-M) interaction diagram",
        GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // get solution input
      var solution = this.GetSolutionGoo(DA, 0);

      // Get boundary input
      var rect = new Rectangle3d();
      if (!DA.GetData(2, ref rect)) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unable to convert {Params.Input[2].NickName} to Rectangle");
        return;
      }

      if (_mode == FoldMode.NM) {
        // get angle input
        var angle = (Angle)Input.UnitNumber(this, DA, 1, _angleUnit, true);

        // get loadcurve
        var loadCurve = solution.Strength.GetForceMomentInteractionCurve(angle);

        // create output
        DA.SetData(0, new AdSecInteractionDiagramGoo(loadCurve[0], angle, rect));
      } else {
        // get force input
        var force = (Force)Input.UnitNumber(this, DA, 1, _forceUnit, true);

        // get loadcurve
        var loadCurve = solution.Strength.GetMomentMomentInteractionCurve(force);

        // check if curve is valid
        if (loadCurve.Count == 0) {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
            "The input axial force is outside the capacity range of the section");
          return;
        }

        // create output
        DA.SetData(0, new AdSecInteractionDiagramGoo(loadCurve[0], Angle.FromRadians(0), rect, AdSecInteractionDiagramGoo.InteractionCurveType.MM));
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      switch (_selectedItems[0]) {
        case "N-M":
          _mode = FoldMode.NM;
          _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[1]);
          break;

        case "M-M":
          _mode = FoldMode.MM;
          _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), _selectedItems[1]);
          break;
      }

      base.UpdateUIFromSelectedItems();
    }

    private enum FoldMode {
      NM,
      MM,
    }
  }
}
