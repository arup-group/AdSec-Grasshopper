using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Oasys.AdSec.Reinforcement.Layers;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
    public class CreateRebarSpacing : GH_OasysDropDownComponent
  {
    private enum FoldMode
    {
      Distance,
      Count
    }

    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("846d546a-4284-4d69-906b-0e6985d7ddd3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.RebarSpacing;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Distance;

    public CreateRebarSpacing() : base(
      "Create Rebar Spacing",
      "Spacing",
      "Create Rebar spacing (by Count or Pitch) for an AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat3())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output

    #endregion
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter("Rebar", "Rb", "AdSec Rebar (single or bundle)", GH_ParamAccess.item);
      pManager.AddGenericParameter("Spacing [" + unitAbbreviation + "]", "S", "Number of bars is calculated based on the available length and the given bar pitch. The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm.", GH_ParamAccess.item);
      this._mode = FoldMode.Distance;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Spaced Rebars", "RbS", "Rebars Spaced in a Layer for AdSec Reinforcement", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // 0 rebar input
      AdSecRebarBundleGoo rebar = AdSecInput.AdSecRebarBundleGoo(this, DA, 0);

      switch (this._mode)
      {
        case FoldMode.Distance:
          AdSecRebarLayerGoo bundleD = new AdSecRebarLayerGoo(
            ILayerByBarPitch.Create(
              rebar.Value,
              (Length)Input.UnitNumber(this, DA, 1, _lengthUnit)));
          DA.SetData(0, bundleD);
          break;

        case FoldMode.Count:
          int count = 1;
          DA.GetData(1, ref count);

          AdSecRebarLayerGoo bundleC = new AdSecRebarLayerGoo(
            ILayerByBarCount.Create(
              count,
              rebar.Value));
          DA.SetData(0, bundleC);
          break;
      }
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Spacing method",
        "Measure"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      this.DropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      this.SelectedItems.Add(this.DropDownItems[0][0]);

      // length
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      this.SelectedItems.Add(Length.GetAbbreviation(this._lengthUnit));

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      this.SelectedItems[i] = this.DropDownItems[i][j];
      if (i == 0)
      {
        this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this.SelectedItems[i]);
        if (this._mode == FoldMode.Count)
        {
          // remove the second dropdown (length)
          while (this.DropDownItems.Count > 1)
            this.DropDownItems.RemoveAt(this.DropDownItems.Count - 1);
          while (this.SelectedItems.Count > 1)
            this.SelectedItems.RemoveAt(this.SelectedItems.Count - 1);
        }
        else
        {
          // add second dropdown (length)
          if (this.DropDownItems.Count != 2)
          {
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
            this.SelectedItems.Add(this._lengthUnit.ToString());
          }
        }
        this.ToggleInput();
      }
      else
      {
        this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[i]);
      }
    }

    public override void UpdateUIFromSelectedItems()
    {
      this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this.SelectedItems[0]);
      if (this._mode == FoldMode.Distance)
        this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[1]);
      this.ToggleInput();
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    #region menu override
    private void ToggleInput()
    {
      switch (this._mode)
      {
        case FoldMode.Distance:
          // remove any additional input parameters
          while (this.Params.Input.Count > 1)
            this.Params.UnregisterInputParameter(this.Params.Input[1], true);

          this.Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.Count:
          // add input parameter
          while (this.Params.Input.Count > 1)
            this.Params.UnregisterInputParameter(this.Params.Input[1], true);

          this.Params.RegisterInputParam(new Param_Integer());
          break;
      }
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      if (this._mode == FoldMode.Distance)
      {
        string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
        this.Params.Input[1].Name = "Spacing [" + unitAbbreviation + "]";
        this.Params.Input[1].NickName = "S";
        this.Params.Input[1].Description = "Number of bars is calculated based on the available length and the given bar pitch. The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm.";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;
      }
      if (this._mode == FoldMode.Count)
      {
        this.Params.Input[1].Name = "Count";
        this.Params.Input[1].NickName = "N";
        this.Params.Input[1].Description = "The number of bundles or single bars. The bundles or single bars are spaced out evenly over the available space.";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;
      }
    }
  }
}
