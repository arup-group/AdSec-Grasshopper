using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateProfileWeb : GH_OasysDropDownComponent {
    private enum FoldMode {
      Constant,
      Tapered
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("0f9a9223-e745-44b9-add2-8b2e5950e86a");
    public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateWeb;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Constant;

    public CreateProfileWeb() : base(
      "Create Web",
      "Web",
      "Create a Web for AdSec Profile",
      CategoryName.Name(),
      SubCategoryName.Cat2()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      // set selected item
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]);
      } else {
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }
      ToggleInput();
    }

    public override void VariableParameterMaintenance() {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      if (_mode == FoldMode.Constant) {
        Params.Input[0].Name = "Thickness [" + unitAbbreviation + "]";
        Params.Input[0].NickName = "t";
        Params.Input[0].Description = "Web thickness";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;
      }
      if (_mode == FoldMode.Tapered) {
        Params.Input[0].Name = "Top Thickness [" + unitAbbreviation + "]";
        Params.Input[0].NickName = "Tt";
        Params.Input[0].Description = "Web thickness at the top";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Bottom Thickness [" + unitAbbreviation + "]";
        Params.Input[1].NickName = "Bt";
        Params.Input[1].Description = "Web thickness at the bottom";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new string[] {
        "Web Type",
        "Measure"
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      _selectedItems.Add(_dropDownItems[0][0]);

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter("Thickness [" + unitAbbreviation + "]", "t", "Web thickness", GH_ParamAccess.item);
      _mode = FoldMode.Constant;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("WebProfile", "Web", "Web Profile for AdSec Profile", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      switch (_mode) {
        case FoldMode.Constant:

          var webConst = new AdSecProfileWebGoo(
            IWebConstant.Create(
              (Length)Input.UnitNumber(this, DA, 0, _lengthUnit)));

          DA.SetData(0, webConst);
          break;

        case FoldMode.Tapered:
          var webTaper = new AdSecProfileWebGoo(
            IWebTapered.Create(
              (Length)Input.UnitNumber(this, DA, 0, _lengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, _lengthUnit)));

          DA.SetData(0, webTaper);
          break;
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      RecordUndoEvent("Changed dropdown");
      switch (_mode) {
        case FoldMode.Constant:
          // remove any additional input parameters
          while (Params.Input.Count > 1) {
            Params.UnregisterInputParameter(Params.Input[1], true);
          }
          break;

        case FoldMode.Tapered:
          // add input parameter
          while (Params.Input.Count != 2) {
            Params.RegisterInputParam(new Param_GenericObject());
          }
          break;
      }
    }
  }
}
