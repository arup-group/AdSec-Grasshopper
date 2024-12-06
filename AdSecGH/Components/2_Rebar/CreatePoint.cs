using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {

  public class CreatePointGh : PointRebarFunction { }

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

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter("Y [" + unitAbbreviation + "]", "Y", "The local Y coordinate in yz-plane",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Z [" + unitAbbreviation + "]", "Z", "The local Z coordinate in yz-plane",
        GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Vertex Point", "Vx",
        "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // get inputs
      var y = (Length)Input.UnitNumber(this, DA, 0, _lengthUnit);
      var z = (Length)Input.UnitNumber(this, DA, 1, _lengthUnit);

      // create IPoint
      var pt = IPoint.Create(y, z);

      // Convert to AdSecPointGoo param
      var point = new AdSecPointGoo(pt);

      // set output
      DA.SetData(0, point);
    }

    protected override void UpdateUIFromSelectedItems() {
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
