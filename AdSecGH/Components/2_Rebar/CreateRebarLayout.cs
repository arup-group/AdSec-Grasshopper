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
    private AngleUnit _angleUnit = AngleUnit.Radian;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Line;

    public CreateReinforcementLayout() : base("Create Reinforcement Layout", "Reinforcement Layout",
      "Create a Reinforcement Layout for an AdSec Section", CategoryName.Name(), SubCategoryName.Cat3()) {
      Hidden = false;
    }

    public override Guid ComponentGuid => new Guid("1250f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarLayout;

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
      if (_mode == FoldMode.Line) {
        Params.Input[0].Name = "Spaced Rebars";
        Params.Input[0].NickName = "RbS";
        Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Position 1";
        Params.Input[1].NickName = "Vx1";
        Params.Input[1].Description = "First bar position";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = "Position 2";
        Params.Input[2].NickName = "Vx2";
        Params.Input[2].Description = "Last bar position";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      }

      if (_mode == FoldMode.SingleBars) {
        Params.Input[0].Name = "Rebar";
        Params.Input[0].NickName = "Rb";
        Params.Input[0].Description = "AdSec Rebar (single or bundle)";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Position(s)";
        Params.Input[1].NickName = "Vxs";
        Params.Input[1].Description = "List of bar positions";
        Params.Input[1].Access = GH_ParamAccess.list;
        Params.Input[1].Optional = false;
      }

      if (_mode == FoldMode.Circle) {
        Params.Input[0].Name = "Spaced Rebars";
        Params.Input[0].NickName = "RbS";
        Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Centre";
        Params.Input[1].NickName = "CVx";
        Params.Input[1].Description = "Vertex Point representing the centre of the circle";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = true;

        Params.Input[2].Name = $"Radius [{lengthUnitAbbreviation}]";
        Params.Input[2].NickName = "r";
        Params.Input[2].Description = "Distance representing the radius of the circle";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;

        Params.Input[3].Name = $"StartAngle [{angleUnitAbbreviation}]";
        Params.Input[3].NickName = "s°";
        Params.Input[3].Description = $"[Optional] The starting angle (in {angleUnitAbbreviation}) of the circle. Positive angle is considered anti-clockwise. Default is 0";
        Params.Input[3].Access = GH_ParamAccess.item;
        Params.Input[3].Optional = true;
      }

      if (_mode == FoldMode.Arc) {
        Params.Input[0].Name = "Spaced Rebars";
        Params.Input[0].NickName = "RbS";
        Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Centre";
        Params.Input[1].NickName = "CVx";
        Params.Input[1].Description = "Vertex Point representing the centre of the circle";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = true;

        Params.Input[2].Name = $"Radius [{lengthUnitAbbreviation}]";
        Params.Input[2].NickName = "r";
        Params.Input[2].Description = "Distance representing the radius of the circle";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;

        Params.Input[3].Name = $"StartAngle [{angleUnitAbbreviation}]";
        Params.Input[3].NickName = "s°";
        Params.Input[3].Description = $"[Optional] The starting angle (in {angleUnitAbbreviation})) of the circle. Positive angle is considered anti-clockwise. Default is 0";
        Params.Input[3].Access = GH_ParamAccess.item;
        Params.Input[3].Optional = true;

        Params.Input[4].Name = $"SweepAngle [{angleUnitAbbreviation}]";
        Params.Input[4].NickName = "e°";
        Params.Input[4].Description = $"The angle (in {angleUnitAbbreviation}) sweeped by the arc from its start angle. Positive angle is considered anti-clockwise. Default is π/2";
        Params.Input[4].Access = GH_ParamAccess.item;
        Params.Input[4].Optional = true;
      }
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
      var codeName = rebarLayerValue.CodeDescription;
      var layer = rebarLayerValue.Layer;
      switch (_mode) {
        case FoldMode.Line:
          // create line group
          group = new AdSecRebarGroupGoo(ILineGroup.Create(this.GetAdSecPointGoo(da, 1).AdSecPoint,
            this.GetAdSecPointGoo(da, 2).AdSecPoint, layer), codeName);
          break;
        case FoldMode.Circle:
          // create circle rebar group
          group = new AdSecRebarGroupGoo(ICircleGroup.Create(this.GetAdSecPointGoo(da, 1, true).AdSecPoint,
            (Length)Input.UnitNumber(this, da, 2, _lengthUnit), (Angle)Input.UnitNumber(this, da, 3, _angleUnit, true),
            layer), codeName);
          break;
        case FoldMode.Arc:
          // create arc rebar grouup
          group = new AdSecRebarGroupGoo(IArcGroup.Create(this.GetAdSecPointGoo(da, 1, true).AdSecPoint,
            (Length)Input.UnitNumber(this, da, 2, _lengthUnit), (Angle)Input.UnitNumber(this, da, 3, _angleUnit),
            (Angle)Input.UnitNumber(this, da, 4, _angleUnit), layer), codeName);
          break;
        case FoldMode.SingleBars:
          // create single rebar group
          var barBundle = this.GetAdSecRebarBundleGoo(da, 0).Value;
          var bars = ISingleBars.Create(barBundle.Bundle);
          bars.Positions = this.GetIPoints(da, 1);
          group = new AdSecRebarGroupGoo(bars, barBundle.CodeDescription);
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

    private enum FoldMode {
      Line,
      SingleBars,
      Circle,
      Arc,
    }
  }
}
