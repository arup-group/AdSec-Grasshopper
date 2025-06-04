using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateRebarSpacing : DropdownAdapter<CreateRebarSpacingFunction> {
    // private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private CreateRebarSpacingFunction.FoldMode _mode = CreateRebarSpacingFunction.FoldMode.Distance;

    public override Guid ComponentGuid => new Guid("846d546a-4284-4d69-906b-0e6985d7ddd3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarSpacing;

    public override void SetSelected(int i, int j) {
      string selectedItem = _dropDownItems[i][j];
      _selectedItems[i] = selectedItem;
      if (i == 0) {
        _mode = (CreateRebarSpacingFunction.FoldMode)Enum.Parse(typeof(CreateRebarSpacingFunction.FoldMode),
          _selectedItems[i]);
        BusinessComponent.SetMode(_mode);
        _selectedItems[i] = selectedItem;
      } else {
        UpdateUnits();
      }

      base.UpdateUI();
    }

    public override bool Read(GH_IReader reader) {
      var modeString = string.Empty;
      if (reader.TryGetString("Mode", ref modeString)) {
        BusinessComponent.SetMode(
          (CreateRebarSpacingFunction.FoldMode)Enum.Parse(typeof(CreateRebarSpacingFunction.FoldMode), modeString));
        _selectedItems[0] = modeString;
      }

      var unitString = string.Empty;
      if (reader.TryGetString("LengthUnit", ref unitString)) {
        BusinessComponent.LengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), unitString);

        if (_dropDownItems.Count > 1) {
          _selectedItems[1] = unitString;
        }
      }

      return base.Read(reader);
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("Mode", BusinessComponent.GetMode().ToString());
      writer.SetString("LengthUnit", BusinessComponent.LengthUnitGeometry.ToString());
      return base.Write(writer);
    }

    protected override void BeforeSolveInstance() { UpdateUnits(); }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      if (_dropDownItems.Count > 1) {
        BusinessComponent.LocalLengthUnitGeometry
          = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
        BusinessComponent.UpdateUnits();
      }

      RefreshParameter();
    }
  }
}
