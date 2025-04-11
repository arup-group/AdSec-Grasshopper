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
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Template;

    public override Guid ComponentGuid => new Guid("9876f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarGroup;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]);
        BusinessComponent.SetMode(_mode);
      } else {
        BusinessComponent.LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }

      base.UpdateUI();
    }

    const string modeKey = "Mode";

    public override bool Read(GH_IReader reader) {
      // Save the Units
      var unitString = "";
      if (reader.TryGetString("LengthUnit", ref unitString)) {
        BusinessComponent.LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), unitString);
        _selectedItems[1] = unitString;
      }

      string mode = FoldMode.Template.ToString();
      if (reader.TryGetString(modeKey, ref mode)) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), mode);
        _selectedItems[0] = mode;
      }

      BusinessComponent.SetMode(_mode);

      return base.Read(reader);
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("LengthUnit", BusinessComponent.LengthUnit.ToString());
      writer.SetString(modeKey, _mode.ToString());

      return base.Write(writer);
    }

    protected override string HtmlHelp_Source() {
      return
        "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Reinforcement.Groups.ITemplateGroup.Face.html";
    }

    // protected override void SolveInternal(IGH_DataAccess da) {
    //   // var groups = new List<AdSecRebarGroupGoo>();
    //   //
    //   // // cover
    // var covers = this.GetCovers(da, Params.Input.Count - 1, _lengthUnit);
    //   //
    //   // switch (_mode) {
    //   //   case FoldMode.Template:
    //   //     // check for enough input parameters
    //   //     if (Params.Input[0].SourceCount == 0 && Params.Input[1].SourceCount == 0 && Params.Input[2].SourceCount == 0
    //   //       && Params.Input[3].SourceCount == 0) {
    //   //       AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
    //   //         $"Input parameters {Params.Input[0].NickName}, {Params.Input[1].NickName}, {Params.Input[2].NickName}, and {Params.Input[3].NickName} failed to collect data!");
    //   //       return;
    //   //     }
    //   //
    //   //     // top
    //   //     if (Params.Input[0].SourceCount != 0) {
    //   //       var grp = ITemplateGroup.Create(ITemplateGroup.Face.Top);
    // grp.Layers = this.GetILayers(da, 0);
    //   //       groups.Add(new AdSecRebarGroupGoo(grp));
    //   //     }
    //   //
    //   //     // left
    //   //     if (Params.Input[1].SourceCount != 0) {
    //   //       var grp = ITemplateGroup.Create(ITemplateGroup.Face.LeftSide);
    //   //       grp.Layers = this.GetILayers(da, 1);
    //   //       groups.Add(new AdSecRebarGroupGoo(grp));
    //   //     }
    //   //
    //   //     // right
    //   //     if (Params.Input[2].SourceCount != 0) {
    //   //       var grp = ITemplateGroup.Create(ITemplateGroup.Face.RightSide);
    //   //       grp.Layers = this.GetILayers(da, 2);
    //   //       groups.Add(new AdSecRebarGroupGoo(grp));
    //   //     }
    //   //
    //   //     // bottom
    //   //     if (Params.Input[3].SourceCount != 0) {
    //   //       var grp = ITemplateGroup.Create(ITemplateGroup.Face.Bottom);
    //   //       grp.Layers = this.GetILayers(da, 3);
    //   //       groups.Add(new AdSecRebarGroupGoo(grp));
    //   //     }
    //   //
    //   //     break;
    //   //
    //   //   case FoldMode.Perimeter:
    //   //     // check for enough input parameters
    //   //     if (Params.Input[0].SourceCount == 0) {
    //   //       AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
    //   //         $"Input parameter {Params.Input[0].NickName} failed to collect data!");
    //   //       return;
    //   //     }
    //   //
    //   //     // top
    //   //     if (Params.Input[0].SourceCount != 0) {
    //   //       var grp = IPerimeterGroup.Create();
    //   //       grp.Layers = this.GetILayers(da, 0);
    //   //       groups.Add(new AdSecRebarGroupGoo(grp));
    //   //     }
    //   //
    //   //     break;
    //   //
    //   //   case FoldMode.Link:
    //   //     // check for enough input parameters
    //   //     if (Params.Input[0].SourceCount == 0) {
    //   //       AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
    //   //         $"Input parameter {Params.Input[0].NickName} failed to collect data!");
    //   //       return;
    //   //     }
    //   //
    //   //     // top
    //   //     if (Params.Input[0].SourceCount != 0) {
    //   //       var grp = ILinkGroup.Create(this.GetAdSecRebarBundleGoo(da, 0).Value);
    //   //       groups.Add(new AdSecRebarGroupGoo(grp));
    //   //     }
    //   //
    //   //     break;
    //   // }
    //   //
    //   // for (int i = 0; i < groups.Count; i++) {
    //   //   if (covers.Count > i) {
    //   //     groups[i].Cover = covers[i];
    //   //   } else {
    //   //     groups[i].Cover = covers.Last();
    //   //   }
    //   // }
    //   //
    //   // // set output
    //   // da.SetDataList(0, groups);
    // }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
