using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Oasys.AdSec.Reinforcement;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdSecGH.Components {
  public class CreateRebar : GH_OasysDropDownComponent {
    private enum FoldMode {
      Single,
      Bundle
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("024d241a-b6cc-4134-9f5c-ac9a6dcb2c4b");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Rebar;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Single;

    public CreateRebar() : base(
      "Create Rebar",
      "Rebar",
      "Create Rebar (single or bundle) for an AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat3()) {
      this.Hidden = false; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      this._selectedItems[i] = this._dropDownItems[i][j];
      if (i == 0) {
        this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this._selectedItems[i]);
        this.ToggleInput();
      }
      else {
        this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this._selectedItems[i]);
      }
    }

    public override void VariableParameterMaintenance() {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      this.Params.Input[1].Name = "Diameter [" + unitAbbreviation + "]";
      if (this._mode == FoldMode.Bundle) {
        this.Params.Input[2].Name = "Count";
        this.Params.Input[2].NickName = "N";
        this.Params.Input[2].Description = "Count per bundle (1, 2, 3 or 4)";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;
      }
    }

    protected override void InitialiseDropdowns() {
      this._spacerDescriptions = new List<string>(new string[] {
        "Rebar Type",
        "Measure"
      });

      this._dropDownItems = new List<List<string>>();
      this._selectedItems = new List<string>();

      this._dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      this._selectedItems.Add(this._dropDownItems[0][0]);

      this._dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      this._selectedItems.Add(Length.GetAbbreviation(this._lengthUnit));

      this._isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      pManager.AddGenericParameter("Material", "Mat", "AdSec Reinforcement Material", GH_ParamAccess.item);
      pManager.AddGenericParameter("Diameter [" + unitAbbreviation + "]", "Ø", "Bar Diameter", GH_ParamAccess.item);
      this._mode = FoldMode.Single;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Rebar", "Rb", "Rebar (single or bundle) for AdSec Reinforcement", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // 0 material input
      AdSecMaterial material = AdSecInput.AdSecMaterial(this, DA, 0);

      switch (this._mode) {
        case FoldMode.Single:
          AdSecRebarBundleGoo rebar = new AdSecRebarBundleGoo(
            IBarBundle.Create(
              (Oasys.AdSec.Materials.IReinforcement)material.Material,
              (Length)Input.UnitNumber(this, DA, 1, this._lengthUnit)));
          DA.SetData(0, rebar);
          break;

        case FoldMode.Bundle:
          int count = 1;
          DA.GetData(2, ref count);

          AdSecRebarBundleGoo bundle = new AdSecRebarBundleGoo(
            IBarBundle.Create(
              (Oasys.AdSec.Materials.IReinforcement)material.Material,
              (Length)Input.UnitNumber(this, DA, 1, this._lengthUnit),
              count));

          DA.SetData(0, bundle);
          break;
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this._selectedItems[0]);
      this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this._selectedItems[1]);
      this.ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      this.RecordUndoEvent("Changed dropdown");
      switch (this._mode) {
        case FoldMode.Single:
          // remove any additional input parameters
          while (this.Params.Input.Count > 2)
            this.Params.UnregisterInputParameter(this.Params.Input[2], true);
          break;

        case FoldMode.Bundle:
          // add input parameter
          while (this.Params.Input.Count != 3)
            this.Params.RegisterInputParam(new Param_Integer());
          break;
      }
    }
  }
}
