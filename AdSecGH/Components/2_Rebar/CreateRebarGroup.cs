using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

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
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }

      base.UpdateUI();
    }

    protected override string HtmlHelp_Source() {
      return
        "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Reinforcement.Groups.ITemplateGroup.Face.html";
    }

    // protected override void InitialiseDropdowns() {
    // _spacerDescriptions = new List<string>(new[] {
    //   "Group Type",
    //   "Measure",
    // });
    //
    // _dropDownItems = new List<List<string>>();
    // _selectedItems = new List<string>();
    //
    // _dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
    // _selectedItems.Add(_dropDownItems[0][0]);
    //
    // _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
    // _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));
    //
    // _isInitialised = true;
    // }

    protected override void SolveInternal(IGH_DataAccess da) {
      var groups = new List<AdSecRebarGroupGoo>();

      // cover
      var covers = this.GetCovers(da, Params.Input.Count - 1, _lengthUnit);

      switch (_mode) {
        case FoldMode.Template:
          // check for enough input parameters
          if (Params.Input[0].SourceCount == 0 && Params.Input[1].SourceCount == 0 && Params.Input[2].SourceCount == 0
            && Params.Input[3].SourceCount == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Input parameters {Params.Input[0].NickName}, {Params.Input[1].NickName}, {Params.Input[2].NickName}, and {Params.Input[3].NickName} failed to collect data!");
            return;
          }

          // top
          if (Params.Input[0].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.Top);
            grp.Layers = this.GetILayers(da, 0);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          // left
          if (Params.Input[1].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.LeftSide);
            grp.Layers = this.GetILayers(da, 1);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          // right
          if (Params.Input[2].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.RightSide);
            grp.Layers = this.GetILayers(da, 2);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          // bottom
          if (Params.Input[3].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.Bottom);
            grp.Layers = this.GetILayers(da, 3);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          break;

        case FoldMode.Perimeter:
          // check for enough input parameters
          if (Params.Input[0].SourceCount == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Input parameter {Params.Input[0].NickName} failed to collect data!");
            return;
          }

          // top
          if (Params.Input[0].SourceCount != 0) {
            var grp = IPerimeterGroup.Create();
            grp.Layers = this.GetILayers(da, 0);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          break;

        case FoldMode.Link:
          // check for enough input parameters
          if (Params.Input[0].SourceCount == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Input parameter {Params.Input[0].NickName} failed to collect data!");
            return;
          }

          // top
          if (Params.Input[0].SourceCount != 0) {
            var grp = ILinkGroup.Create(this.GetAdSecRebarBundleGoo(da, 0).Value);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          break;
      }

      for (int i = 0; i < groups.Count; i++) {
        if (covers.Count > i) {
          groups[i].Cover = covers[i];
        } else {
          groups[i].Cover = covers.Last();
        }
      }

      // set output
      da.SetDataList(0, groups);
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
