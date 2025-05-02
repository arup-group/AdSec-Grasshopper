using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AdSecGH.Parameters;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public enum FoldMode {
    Template,
    Perimeter,
    Link,
  }

  public interface IVariableInput {
    event Action OnVariableInputChanged;
  }

  public interface IDropdownOptions {
    IOptions[] Options { get; set; }
  }

  public interface IOptions {
    string Description { get; set; }
    string[] GetOptions();
  }

  public class EnumOptions : IOptions {
    public string Description { get; set; }
    public Type EnumType { get; set; }

    public string[] GetOptions() {
      return Enum.GetNames(EnumType);
    }
  }

  public class UnitOptions : IOptions {
    public string Description { get; set; } = "Measure";
    public string[] GetOptions() { return Array.Empty<string>(); }
    public Type UnitType { get; set; }
  }

  public class RebarGroupFunction : Function, IVariableInput, IDropdownOptions {

    public event Action OnVariableInputChanged;

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Reinforcement Group",
      NickName = "Reinforcement Group",
      Description = "Create a Template Reinforcement Group for an AdSec Section"
    };

    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3()
    };

    public RebarGroupParameter Layout { get; set; } = new RebarGroupParameter() {
      Name = "Layout",
      NickName = "RbG",
      Description = "Rebar Groups for AdSec Section",
    };

    public RebarLayerParameter TopRebars { get; set; } = new RebarLayerParameter() {
      Name = "Top Rebars",
      NickName = "TRs",
      Description = "Top Face AdSec Rebars Spaced in a Layer",
      Optional = true,
    };

    public RebarLayerParameter LeftRebars { get; set; } = new RebarLayerParameter() {
      Name = "Left Side Rebars",
      NickName = "LRs",
      Description = "Left Side Face AdSec Rebars Spaced in a Layer",
      Optional = true,
    };

    public RebarLayerParameter RightRebars { get; set; } = new RebarLayerParameter() {
      Name = "Right Side Rebars",
      NickName = "RRs",
      Description = "Right Side Face AdSec Rebars Spaced in a Layer",
      Optional = true,
    };

    public RebarLayerParameter BottomRebars { get; set; } = new RebarLayerParameter() {
      Name = "Bottom Rebars",
      NickName = "BRs",
      Description = "Bottom Face AdSec Rebars Spaced in a Layer",
      Optional = true,
    };

    public RebarLayerParameter SpacedRebars { get; set; } = new RebarLayerParameter() {
      Name = "Spaced Rebars",
      NickName = "RbS",
      Description = "AdSec Rebars Spaced in a Layer",
      Optional = false,
    };

    public RebarBundleParameter Rebar { get; set; } = new RebarBundleParameter() {
      Name = "Rebar",
      NickName = "Rb",
      Description = "AdSec Rebar (single or bundle)",
      Optional = false,
    };
    public DoubleArrayParameter Cover { get; set; } = new DoubleArrayParameter() {
      Name = "Cover",
      NickName = "Cov",
      Description = "The reinforcement-free zone around the faces of a profile.",
      Optional = false,
    };

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Layout
      };
    }

    public override Attribute[] GetAllInputAttributes() {
      Attribute[] allInputAttributes = { };
      switch (Mode) {
        case FoldMode.Template:
          allInputAttributes = new Attribute[] {
            TopRebars,
            LeftRebars,
            RightRebars,
            BottomRebars,
            Cover
          };
          break;
        case FoldMode.Perimeter:
          allInputAttributes = new Attribute[] {
            SpacedRebars,
            Cover
          };
          break;
        case FoldMode.Link:
          allInputAttributes = new Attribute[] {
            Rebar,
            Cover
          };
          break;
      }

      return allInputAttributes;
    }

    public override void Compute() {
      var groups = new List<AdSecRebarGroup>();

      switch (Mode) {
        case FoldMode.Template:
          CreateTemplate(groups);
          break;
        case FoldMode.Link:
          CreateLink(groups);
          break;
        case FoldMode.Perimeter:
          CreatePerimeter(groups);
          break;
      }

      var cover = Cover.Value;
      if (cover == null) {
        WarningMessages.Add($"Input parameter {Cover.NickName} failed to collect data!");
        return;
      }

      Layout.Value = groups.ToArray();
      double coverSize = 0;
      for (int i = 0; i < Layout.Value.Length; i++) {
        if (cover.Length > i) {
          coverSize = cover[i];
        }

        var rebarGroup = Layout.Value[i];
        rebarGroup.Cover = ICover.Create(Length.From(coverSize, LengthUnitGeometry));
      }
    }

    private void CreatePerimeter(List<AdSecRebarGroup> groups) {
      var perimeterGroup = IPerimeterGroup.Create();
      foreach (var layer in SpacedRebars.Value) {
        perimeterGroup.Layers.Add(layer);
      }

      groups.Add(new AdSecRebarGroup(perimeterGroup));
    }

    private void CreateLink(List<AdSecRebarGroup> groups) {
      var linkGroup = ILinkGroup.Create(Rebar.Value);
      groups.Add(new AdSecRebarGroup(linkGroup));
    }

    private void CreateTemplate(List<AdSecRebarGroup> groups) {
      // top
      if (CreateRebarGroup(TopRebars.Value, out AdSecRebarGroup topGroup, ITemplateGroup.Face.Top)) {
        groups.Add(topGroup);
      }

      // left
      if (CreateRebarGroup(LeftRebars.Value, out AdSecRebarGroup leftGroup, ITemplateGroup.Face.LeftSide)) {
        groups.Add(leftGroup);
      }

      // right
      if (CreateRebarGroup(RightRebars.Value, out AdSecRebarGroup rightGroup, ITemplateGroup.Face.RightSide)) {
        groups.Add(rightGroup);
      }

      // bottom
      if (CreateRebarGroup(BottomRebars.Value, out AdSecRebarGroup bottomGroup, ITemplateGroup.Face.Bottom)) {
        groups.Add(bottomGroup);
      }

      if (groups.Count == 0) {
        WarningMessages.Add(
          $"Input parameters {TopRebars.NickName}, {LeftRebars.NickName}, {RightRebars.NickName}, and {BottomRebars.NickName} failed to collect data!");
      }
    }

    private static bool CreateRebarGroup(ILayer[] rebar, out AdSecRebarGroup group, IFace face) {
      if (rebar != null) {
        var grp = ITemplateGroup.Create(face);

        foreach (var layer in rebar) {
          grp.Layers.Add(layer);
        }

        group = new AdSecRebarGroup(grp);
        return true;
      }

      group = null;
      return false;
    }

    public void SetMode(FoldMode template) {
      if (Mode == template) {
        return;
      }

      Mode = template;
      OnVariableInputChanged?.Invoke();
    }

    public FoldMode Mode { get; set; } = FoldMode.Template;
    public IOptions[] Options { get; set; } = {
      new EnumOptions() {
        EnumType = typeof(FoldMode),
        Description = "Group Type",
      },
      new UnitOptions() {
        Description = "Measure",
        UnitType = typeof(LengthUnit),
      }
    };
  }
}
