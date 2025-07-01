using System;
using System.Collections.Generic;
using System.Drawing;

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
    }

    public AdSecPointParameter AdSecPoint { get; set; } = new AdSecPointParameter();

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecPoint,
      };
    }
  }

  public class CreatePoint : DropdownAdapter<CreatePointGh> {

    private LengthUnit _lengthUnitGeometry = DefaultUnits.LengthUnitGeometry;
    public CreatePoint() { Hidden = false; }
    public override Guid ComponentGuid => new Guid("1a0cdb3c-d66d-420e-a9d8-35d31587a122");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    protected override Bitmap Icon => Resources.VertexPoint;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      _lengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      UpdateLocalUnitsAndRefreshParams();
      base.UpdateUI();
    }


    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(BusinessComponent.LengthUnitGeometry));

      _isInitialised = true;
    }

    protected override void UpdateUIFromSelectedItems() {
      _lengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }

    protected override void BeforeSolveInstance() {
      UpdateLocalUnitsAndRefreshParams();
    }

    private void UpdateLocalUnitsAndRefreshParams() {
      UpdateUnits();
      //update local unit if any
      BusinessComponent.LengthUnitGeometry = _lengthUnitGeometry;
      RefreshParameter(this);
    }
  }
}
