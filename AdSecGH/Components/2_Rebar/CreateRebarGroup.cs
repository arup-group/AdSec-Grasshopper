using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using OasysGH;
using OasysGH.Components;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateReinforcementGroup : GH_OasysDropDownComponent {
    private enum FoldMode {
      Template,
      Perimeter,
      Link
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("9876f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.RebarGroup;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Template;

    public CreateReinforcementGroup() : base(
      "Create Reinforcement Group",
      "Reinforcement Group",
      "Create a Template Reinforcement Group for an AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat3()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]);
        ToggleInput();
      } else {
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }
    }

    public override void VariableParameterMaintenance() {
      if (_mode == FoldMode.Template) {
        Params.Input[0].Name = "Top Rebars";
        Params.Input[0].NickName = "TRs";
        Params.Input[0].Description = "Top Face AdSec Rebars Spaced in a Layer";
        Params.Input[0].Access = GH_ParamAccess.list;
        Params.Input[0].Optional = true;

        Params.Input[1].Name = "Left Side Rebars";
        Params.Input[1].NickName = "LRs";
        Params.Input[1].Description = "Left Side Face AdSec Rebars Spaced in a Layer";
        Params.Input[1].Access = GH_ParamAccess.list;
        Params.Input[1].Optional = true;

        Params.Input[2].Name = "Right Side Rebars";
        Params.Input[2].NickName = "RRs";
        Params.Input[2].Description = "Right Side Face AdSec Rebars Spaced in a Layer";
        Params.Input[2].Access = GH_ParamAccess.list;
        Params.Input[2].Optional = true;

        Params.Input[3].Name = "Bottom Rebars";
        Params.Input[3].NickName = "BRs";
        Params.Input[3].Description = "Bottom Face AdSec Rebars Spaced in a Layer";
        Params.Input[3].Access = GH_ParamAccess.list;
        Params.Input[3].Optional = true;
      }
      if (_mode == FoldMode.Perimeter) {
        Params.Input[0].Name = "Spaced Rebars";
        Params.Input[0].NickName = "RbS";
        Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
        Params.Input[0].Access = GH_ParamAccess.list;
        Params.Input[0].Optional = false;
      }
      if (_mode == FoldMode.Link) {
        Params.Input[0].Name = "Rebar";
        Params.Input[0].NickName = "Rb";
        Params.Input[0].Description = "AdSec Rebar (single or bundle)";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;
      }

      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      Params.Input[Params.Input.Count - 1].Name = "Cover [" + unitAbbreviation + "]";
      Params.Input[Params.Input.Count - 1].NickName = "Cov";
      Params.Input[Params.Input.Count - 1].Description = "AdSec Rebars Spaced in a Layer";
      Params.Input[Params.Input.Count - 1].Access = GH_ParamAccess.list;
      Params.Input[Params.Input.Count - 1].Optional = false;
    }

    protected override string HtmlHelp_Source() {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Reinforcement.Groups.ITemplateGroup.Face.html";
      return help;
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new string[] {
        "Group Type",
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      _selectedItems.Add(_dropDownItems[0][0]);
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter("Top Rebars", "TRs", "Top Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.list);
      pManager.AddGenericParameter("Left Side Rebars", "LRs", "Left Side Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.list);
      pManager.AddGenericParameter("Right Side Rebars", "RRs", "Right Side Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.list);
      pManager.AddGenericParameter("Bottom Rebars", "BRs", "Bottom Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.list);
      pManager.AddGenericParameter("Cover [" + unitAbbreviation + "]", "Cov", "The reinforcement-free zone around the faces of a profile.", GH_ParamAccess.list);
      _mode = FoldMode.Template;
      // make all but last input optional
      for (int i = 0; i < pManager.ParamCount - 1; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Layout", "RbG", "Rebar Groups for AdSec Section", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      var groups = new List<AdSecRebarGroupGoo>();

      // cover
      List<ICover> covers = AdSecInput.Covers(this, DA, Params.Input.Count - 1, _lengthUnit);

      switch (_mode) {
        case FoldMode.Template:
          // check for enough input parameters
          if (Params.Input[0].SourceCount == 0 && Params.Input[1].SourceCount == 0
              && Params.Input[2].SourceCount == 0 && Params.Input[3].SourceCount == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameters " + Params.Input[0].NickName + ", " + Params.Input[1].NickName + ", " + Params.Input[2].NickName + ", and " + Params.Input[3].NickName + " failed to collect data!");
            return;
          }
          // top
          if (Params.Input[0].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.Top);
            grp.Layers = AdSecInput.ILayers(this, DA, 0);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }
          // left
          if (Params.Input[1].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.LeftSide);
            grp.Layers = AdSecInput.ILayers(this, DA, 1);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }
          // right
          if (Params.Input[2].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.RightSide);
            grp.Layers = AdSecInput.ILayers(this, DA, 2);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }
          // bottom
          if (Params.Input[3].SourceCount != 0) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.Bottom);
            grp.Layers = AdSecInput.ILayers(this, DA, 3);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }

          break;

        case FoldMode.Perimeter:
          // check for enough input parameters
          if (Params.Input[0].SourceCount == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[0].NickName + " failed to collect data!");
            return;
          }
          // top
          if (Params.Input[0].SourceCount != 0) {
            var grp = IPerimeterGroup.Create();
            grp.Layers = AdSecInput.ILayers(this, DA, 0);
            groups.Add(new AdSecRebarGroupGoo(grp));
          }
          break;

        case FoldMode.Link:
          // check for enough input parameters
          if (Params.Input[0].SourceCount == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[0].NickName + " failed to collect data!");
            return;
          }
          // top
          if (Params.Input[0].SourceCount != 0) {
            var grp = ILinkGroup.Create(AdSecInput.IBarBundle(this, DA, 0));
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
      DA.SetDataList(0, groups);
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      // remove cover temporarily
      IGH_Param param_Cover = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // remove any additional input parameters
      while (Params.Input.Count > 1) {
        Params.UnregisterInputParameter(Params.Input[1]);
      }

      if (_mode == FoldMode.Template) {
        // register 3 generic
        Params.RegisterInputParam(new Param_GenericObject());
        Params.RegisterInputParam(new Param_GenericObject());
        Params.RegisterInputParam(new Param_GenericObject());
      }
      // add cover back
      Params.RegisterInputParam(param_Cover);
    }
  }
}
