using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.Profiles;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateProfileFlange : GH_OasysDropDownComponent {
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;

    public CreateProfileFlange() : base("Create Flange", "Flange", "Create a Flange for AdSec Profile",
      CategoryName.Name(), SubCategoryName.Cat2()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("c182921f-0ace-49ca-8fb7-5722dbf2ba30");
    public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateFlange;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      Params.Input[0].Name = $"Width [{unitAbbreviation}]";
      Params.Input[1].Name = $"Thickness [{unitAbbreviation}]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // length
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      IQuantity quantity = new Length(0, _lengthUnit);

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter($"Width [{unitAbbreviation}]", "B", "Flange width", GH_ParamAccess.item);
      pManager.AddGenericParameter($"Thickness [{unitAbbreviation}]", "t", "Flange thickness",
        GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("FlangeProfile", "Fla", "Flange Profile for AdSec Profile", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      var flange = new AdSecProfileFlangeGoo(IFlange.Create((Length)Input.UnitNumber(this, DA, 0, _lengthUnit),
        (Length)Input.UnitNumber(this, DA, 1, _lengthUnit)));

      DA.SetData(0, flange);
    }

    protected override void UpdateUIFromSelectedItems() {
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
