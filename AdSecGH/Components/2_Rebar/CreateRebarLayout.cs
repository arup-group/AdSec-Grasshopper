using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.Reinforcement.Groups;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateReinforcementLayout : GH_OasysDropDownComponent {
    private enum FoldMode {
      Line,
      SingleBars,
      Circle,
      Arc,
    }

    private const string PositiveAngleIsConsideredAntiClockwise = "Positive angle is considered anti-clockwise.";

    public override Guid ComponentGuid => new Guid("1250f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarLayout;
    private AngleUnit _angleUnit = AngleUnit.Radian;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Line;

    public CreateReinforcementLayout() : base("Create Reinforcement Layout", "Reinforcement Layout",
      "Create a Reinforcement Layout for an AdSec Section", CategoryName.Name(), SubCategoryName.Cat3()) {
      Hidden = false;
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]);

        switch (_mode) {
          case FoldMode.Line:
          case FoldMode.SingleBars:
            while (_dropDownItems.Count > 1) {
              _dropDownItems.RemoveAt(1);
            }

            _spacerDescriptions[1] = "Measure";
            break;

          case FoldMode.Arc:
          case FoldMode.Circle:
            if (_dropDownItems.Count < 2) {
              _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
            }

            if (_dropDownItems.Count < 3) {
              _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle));
            }

            _spacerDescriptions[1] = "Length measure";
            break;
        }

        ToggleInput();
      } else {
        switch (i) {
          case 1:
            _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
            break;

          case 2:
            _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[i]);
            break;
        }
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string angleUnitAbbreviation = Angle.GetAbbreviation(_angleUnit);
      string lengthUnitAbbreviation = Length.GetAbbreviation(_lengthUnit);

      switch (_mode) {
        case FoldMode.Line:
          UpdateSpacedRebarInput();
          Params.Input[1].UpdateItemInput("Position 1", "Vx1", "First bar position");
          Params.Input[2].UpdateItemInput("Position 2", "Vx2", "Last bar position");
          break;
        case FoldMode.SingleBars:
          Params.Input[0].UpdateItemInput("Rebar", "Rb", "AdSec Rebar (single or bundle)");
          Params.Input[1].UpdateListInput("Position(s)", "Vxs", "List of bar positions");
          break;
        case FoldMode.Circle:
          UpdateSpacedRebarInput();
          UpdateCenterInput();
          UpdateRadiusInput(lengthUnitAbbreviation);
          UpdateStartAngleInput(angleUnitAbbreviation);
          break;
        case FoldMode.Arc:
          UpdateSpacedRebarInput();
          UpdateCenterInput();
          UpdateRadiusInput(lengthUnitAbbreviation);
          UpdateStartAngleInput(angleUnitAbbreviation);
          Params.Input[4].UpdateItemInput($"SweepAngle [{angleUnitAbbreviation}]", "e°",
            $"The angle (in {angleUnitAbbreviation}) sweeped by the arc from its start angle. {PositiveAngleIsConsideredAntiClockwise} Default is π/2",
            true);
          break;
      }
    }

    private void UpdateSpacedRebarInput(int index = 0) {
      const string spacedRebarsName = "Spaced Rebars";
      const string spacedRebarsNick = "RbS";
      const string spacedRebarsDesc = "AdSec Rebars Spaced in a Layer";
      Params.Input[index].UpdateItemInput(spacedRebarsName, spacedRebarsNick, spacedRebarsDesc);
    }

    private void UpdateCenterInput(int index = 1) {
      const string centreDesc = "Vertex Point representing the centre of the circle";
      Params.Input[index].UpdateItemInput("Centre", "CVx", centreDesc, true);
    }

    private void UpdateRadiusInput(string lengthUnitAbbreviation, int index = 2) {
      const string radiusDesc = "Distance representing the radius of the circle";
      Params.Input[index].UpdateItemInput($"Radius [{lengthUnitAbbreviation}]", "r", radiusDesc);
    }

    private void UpdateStartAngleInput(string angleUnitAbbreviation, int index = 3) {
      Params.Input[index].UpdateItemInput($"StartAngle [{angleUnitAbbreviation}]", "s°",
        $"[Optional] The starting angle (in {angleUnitAbbreviation}) of the circle. {PositiveAngleIsConsideredAntiClockwise} Default is 0",
        true);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Layout Type",
        "Measure",
        "Angular measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      _selectedItems.Add(_dropDownItems[0][0]);

      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));
      _selectedItems.Add(Angle.GetAbbreviation(_angleUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Spaced Rebars", "RbS", "AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
      pManager.AddGenericParameter("Position 1", "Vx1", "First bar position", GH_ParamAccess.item);
      pManager.AddGenericParameter("Position 2", "Vx2", "Last bar position", GH_ParamAccess.item);
      _mode = FoldMode.Line;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Layout", "RbG", "Rebar Group for AdSec Section", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess da) {
      AdSecRebarGroupGoo group = null;
      var rebarLayerValue = this.GetAdSecRebarLayerGoo(da, 0).Value;
      switch (_mode) {
        case FoldMode.Line:
          // create line group
          group = new AdSecRebarGroupGoo(ILineGroup.Create(this.GetAdSecPointGoo(da, 1).AdSecPoint,
            this.GetAdSecPointGoo(da, 2).AdSecPoint, rebarLayerValue));
          break;
        case FoldMode.Circle:
          // create circle rebar group
          group = new AdSecRebarGroupGoo(ICircleGroup.Create(this.GetAdSecPointGoo(da, 1, true).AdSecPoint,
            (Length)Input.UnitNumber(this, da, 2, _lengthUnit), (Angle)Input.UnitNumber(this, da, 3, _angleUnit, true),
            rebarLayerValue));
          break;
        case FoldMode.Arc:
          // create arc rebar grouup
          group = new AdSecRebarGroupGoo(IArcGroup.Create(this.GetAdSecPointGoo(da, 1, true).AdSecPoint,
            (Length)Input.UnitNumber(this, da, 2, _lengthUnit), (Angle)Input.UnitNumber(this, da, 3, _angleUnit),
            (Angle)Input.UnitNumber(this, da, 4, _angleUnit), rebarLayerValue));
          break;
        case FoldMode.SingleBars:
          // create single rebar group
          var bars = ISingleBars.Create(this.GetAdSecRebarBundleGoo(da, 0).Value);
          bars.Positions = this.GetIPoints(da, 1);
          group = new AdSecRebarGroupGoo(bars);

          break;
      }

      // set output
      da.SetData(0, group);
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[2]);
      ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      // remove any additional input parameters
      while (Params.Input.Count > 1) {
        Params.UnregisterInputParameter(Params.Input[1], true);
      }

      switch (_mode) {
        case FoldMode.Line:
          Params.RegisterInputParam(new Param_GenericObject());
          Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.SingleBars:
          while (Params.Input.Count > 1) {
            Params.UnregisterInputParameter(Params.Input[1], true);
          }

          Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.Circle:
          Params.RegisterInputParam(new Param_GenericObject());
          Params.RegisterInputParam(new Param_GenericObject());
          Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.Arc:
          Params.RegisterInputParam(new Param_GenericObject());
          Params.RegisterInputParam(new Param_GenericObject());
          Params.RegisterInputParam(new Param_GenericObject());
          Params.RegisterInputParam(new Param_GenericObject());
          break;
      }
    }
  }
}
