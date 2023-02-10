using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
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

namespace AdSecGH.Components
{
    public class CreateReinforcementLayout : GH_OasysDropDownComponent
  {
    private enum FoldMode
    {
      Line,
      SingleBars,
      Circle,
      Arc,
    }

    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("1250f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.RebarLayout;
    private AngleUnit _angleUnit = AngleUnit.Radian;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Line;

    public CreateReinforcementLayout() : base(
      "Create Reinforcement Layout",
      "Reinforcement Layout",
      "Create a Reinforcement Layout for an AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat3())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Spaced Rebars", "RbS", "AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
      pManager.AddGenericParameter("Position 1", "Vx1", "First bar position", GH_ParamAccess.item);
      pManager.AddGenericParameter("Position 2", "Vx2", "Last bar position", GH_ParamAccess.item);
      _mode = FoldMode.Line;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Layout", "RbG", "Rebar Group for AdSec Section", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      AdSecRebarGroupGoo group = null;

      switch (this._mode)
      {
        case FoldMode.Line:
          // create line group
          group = new AdSecRebarGroupGoo(
            ILineGroup.Create(
              AdSecInput.IPoint(this, DA, 1),
              AdSecInput.IPoint(this, DA, 2),
              AdSecInput.ILayer(this, DA, 0)));
          break;

        case FoldMode.Circle:
          // create circle rebar group
          group = new AdSecRebarGroupGoo(
            ICircleGroup.Create(
              AdSecInput.IPoint(this, DA, 1, true),
              (Length)Input.UnitNumber(this, DA, 2, this._lengthUnit),
              (Angle)Input.UnitNumber(this, DA, 3, this._angleUnit, true),
              AdSecInput.ILayer(this, DA, 0)));
          break;

        case FoldMode.Arc:
          // create arc rebar grouup
          group = new AdSecRebarGroupGoo(
            IArcGroup.Create(
              AdSecInput.IPoint(this, DA, 1, true),
              (Length)Input.UnitNumber(this, DA, 2, this._lengthUnit),
              (Angle)Input.UnitNumber(this, DA, 3, this._angleUnit),
              (Angle)Input.UnitNumber(this, DA, 4, this._angleUnit),
              AdSecInput.ILayer(this, DA, 0)));
          break;

        case FoldMode.SingleBars:
          // create single rebar group
          ISingleBars bars = ISingleBars.Create(
            AdSecInput.IBarBundle(this, DA, 0));

          bars.Positions = AdSecInput.IPoints(this, DA, 1);

          group = new AdSecRebarGroupGoo(bars);
          break;
      }

      // set output
      DA.SetData(0, group);
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      List<string> spacerDescriptions = new List<string>(new string[] {
        "Layout Type",
        "Measure",
        "Angular measure"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      this.DropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      this.SelectedItems.Add(this.DropDownItems[0][0]);

      this.SelectedItems.Add(this._lengthUnit.ToString());
      this.SelectedItems.Add(this._angleUnit.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // set selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];
      if (i == 0)
      {
        this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this.SelectedItems[i]);

        switch (this._mode)
        {
          case FoldMode.Line:
          case FoldMode.SingleBars:
            while (this.DropDownItems.Count > 1)
              this.DropDownItems.RemoveAt(1);
            this.SpacerDescriptions[1] = "Measure";
            break;
          case FoldMode.Arc:
          case FoldMode.Circle:
            if (this.DropDownItems.Count < 2)
              this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
            if (this.DropDownItems.Count < 3)
              this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle));
            this.SpacerDescriptions[1] = "Length measure";
            break;
        }

        this.ToggleInput();
      }
      else
      {
        switch (i)
        {
          case 1:
            this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[i]);
            break;

          case 2:
            this._angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), this.SelectedItems[i]);
            break;
        }
      }
    }

    public override void UpdateUIFromSelectedItems()
    {
      this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this.SelectedItems[0]);
      this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[1]);
      this._angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), this.SelectedItems[2]);
      this.CreateAttributes();
      this.ToggleInput();
    }
    #endregion

    private void ToggleInput()
    {
      switch (this._mode)
      {
        case FoldMode.Line:
          // remove any additional input parameters
          while (this.Params.Input.Count > 1)
            this.Params.UnregisterInputParameter(this.Params.Input[1], true);
          // register 2 generic
          this.Params.RegisterInputParam(new Param_GenericObject());
          this.Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.SingleBars:
          // remove any additional input parameters
          while (this.Params.Input.Count > 1)
            this.Params.UnregisterInputParameter(this.Params.Input[1], true);
          // register 1 generic
          this.Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.Circle:
          // remove any additional input parameters
          while (this.Params.Input.Count > 1)
            this.Params.UnregisterInputParameter(this.Params.Input[1], true);
          // register 3 generic
          this.Params.RegisterInputParam(new Param_GenericObject());
          this.Params.RegisterInputParam(new Param_GenericObject());
          this.Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.Arc:
          // remove any additional input parameters
          while (this.Params.Input.Count > 1)
            this.Params.UnregisterInputParameter(this.Params.Input[1], true);
          // register 4 generic
          this.Params.RegisterInputParam(new Param_GenericObject());
          this.Params.RegisterInputParam(new Param_GenericObject());
          this.Params.RegisterInputParam(new Param_GenericObject());
          this.Params.RegisterInputParam(new Param_GenericObject());
          break;
      }

      this.UpdateUI();
    }

    public override void VariableParameterMaintenance()
    {
      string angleUnitAbbreviation = Angle.GetAbbreviation(this._angleUnit);
      string lengthUnitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      if (this._mode == FoldMode.Line)
      {
        this.Params.Input[0].Name = "Spaced Rebars";
        this.Params.Input[0].NickName = "RbS";
        this.Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Position 1";
        this.Params.Input[1].NickName = "Vx1";
        this.Params.Input[1].Description = "First bar position";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;

        this.Params.Input[2].Name = "Position 2";
        this.Params.Input[2].NickName = "Vx2";
        this.Params.Input[2].Description = "Last bar position";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;

      }
      if (this._mode == FoldMode.SingleBars)
      {
        this.Params.Input[0].Name = "Rebar";
        this.Params.Input[0].NickName = "Rb";
        this.Params.Input[0].Description = "AdSec Rebar (single or bundle)";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Position(s)";
        this.Params.Input[1].NickName = "Vxs";
        this.Params.Input[1].Description = "List of bar positions";
        this.Params.Input[1].Access = GH_ParamAccess.list;
        this.Params.Input[1].Optional = false;
      }
      if (this._mode == FoldMode.Circle)
      {
        this.Params.Input[0].Name = "Spaced Rebars";
        this.Params.Input[0].NickName = "RbS";
        this.Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Centre";
        this.Params.Input[1].NickName = "CVx";
        this.Params.Input[1].Description = "Vertex Point representing the centre of the circle";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = true;

        this.Params.Input[2].Name = "Radius [" + lengthUnitAbbreviation + "]";
        this.Params.Input[2].NickName = "r";
        this.Params.Input[2].Description = "Distance representing the radius of the circle";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;

        this.Params.Input[3].Name = "StartAngle [" + angleUnitAbbreviation + "]";
        this.Params.Input[3].NickName = "s°";
        this.Params.Input[3].Description = "[Optional] The starting angle (in " + angleUnitAbbreviation + ") of the circle. Positive angle is considered anti-clockwise. Default is 0";
        this.Params.Input[3].Access = GH_ParamAccess.item;
        this.Params.Input[3].Optional = true;
      }
      if (this._mode == FoldMode.Arc)
      {
        this.Params.Input[0].Name = "Spaced Rebars";
        this.Params.Input[0].NickName = "RbS";
        this.Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Centre";
        this.Params.Input[1].NickName = "CVx";
        this.Params.Input[1].Description = "Vertex Point representing the centre of the circle";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = true;

        this.Params.Input[2].Name = "Radius [" + lengthUnitAbbreviation + "]";
        this.Params.Input[2].NickName = "r";
        this.Params.Input[2].Description = "Distance representing the radius of the circle";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;

        this.Params.Input[3].Name = "StartAngle [" + angleUnitAbbreviation + "]";
        this.Params.Input[3].NickName = "s°";
        this.Params.Input[3].Description = "[Optional] The starting angle (in " + angleUnitAbbreviation + ")) of the circle. Positive angle is considered anti-clockwise. Default is 0";
        this.Params.Input[3].Access = GH_ParamAccess.item;
        this.Params.Input[3].Optional = true;

        this.Params.Input[4].Name = "SweepAngle [" + angleUnitAbbreviation + "]";
        this.Params.Input[4].NickName = "e°";
        this.Params.Input[4].Description = "The angle (in " + angleUnitAbbreviation + ") sweeped by the arc from its start angle. Positive angle is considered anti-clockwise. Default is π/2";
        this.Params.Input[4].Access = GH_ParamAccess.item;
        this.Params.Input[4].Optional = true;
      }
    }
  }
}
