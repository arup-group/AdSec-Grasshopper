using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

using Rhino.Runtime;

namespace AdSecGH.Components {
  public class CreateReinforcementGroup : DropdownAdapter<RebarGroupFunction> {

    public override Guid ComponentGuid => new Guid("9876f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarGroup;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        BusinessComponent.SetMode((FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]));
      } else {
        BusinessComponent.LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }

      base.UpdateUI();
    }

    const string modeKey = "Mode";

    public override bool Read(GH_IReader reader) {
      var unitString = string.Empty;
      if (reader.TryGetString("LengthUnit", ref unitString)) {
        BusinessComponent.LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), unitString);
        _selectedItems[1] = unitString;
      }

      string mode = FoldMode.Template.ToString();
      if (reader.TryGetString(modeKey, ref mode)) {
        BusinessComponent.SetMode((FoldMode)Enum.Parse(typeof(FoldMode), mode));
        _selectedItems[0] = mode;
      }

      return base.Read(reader);
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("LengthUnit", BusinessComponent.LengthUnit.ToString());
      writer.SetString(modeKey, BusinessComponent.Mode.ToString());

      return base.Write(writer);
    }

    protected override string HtmlHelp_Source() {
      return
        "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Reinforcement.Groups.ITemplateGroup.Face.html";
    }

    protected override void UpdateUIFromSelectedItems() {
      BusinessComponent.SetMode((FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]));
      BusinessComponent.LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
