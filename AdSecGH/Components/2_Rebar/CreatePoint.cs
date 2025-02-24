using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class CreatePointGh : PointRebarFunction {

    public CreatePointGh() {
      var adSecPointAttribute = AdSecPoint as Attribute;
      Point.Update(ref adSecPointAttribute);
      Point.OnValueChanged += goo => {
        AdSecPoint.Value = new AdSecPointGoo(Point.Value);
      };

      Y.Name = Y.NameWithUnits(DefaultUnits.LengthUnitGeometry);
      Z.Name = Z.NameWithUnits(DefaultUnits.LengthUnitGeometry);
    }

    public AdSecPointParameter AdSecPoint { get; set; } = new AdSecPointParameter();

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecPoint,
      };
    }
  }

  public class CreatePoint : DropdownAdapter<CreatePointGh> {
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;

    public CreatePoint() { Hidden = false; }

    public override Guid ComponentGuid => new Guid("1a0cdb3c-d66d-420e-a9d8-35d31587a122");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    protected override Bitmap Icon => Resources.VertexPoint;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      Params.Input[0].Name = "Y [" + unitAbbreviation + "]";
      Params.Input[1].Name = "Z [" + unitAbbreviation + "]";
      BusinessComponent.Y.Name = Params.Input[0].Name;
      BusinessComponent.Z.Name = Params.Input[1].Name;
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      _isInitialised = true;
    }

    protected override void UpdateUIFromSelectedItems() {
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
