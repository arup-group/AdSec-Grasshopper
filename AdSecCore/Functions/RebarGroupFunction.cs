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
    bool IsComputed { get; set; }
  }

  public class EnumOptions : IOptions {

    public string Description { get; set; }
    public Type EnumType { get; set; }

    public string[] GetOptions() {
      return Enum.GetNames(EnumType);
    }

    public bool IsComputed { get; set; } = true;
  }

  public class UnitOptions : IOptions {
    public string Description { get; set; } = "Measure";
    public string[] GetOptions() { return Array.Empty<string>(); }

    public Type UnitType { get; set; }

    public bool IsComputed { get; set; } = false;

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

      // cover

      switch (Mode) {
        case FoldMode.Template:
          var topRebarsValue = TopRebars.Value;

          // top
          if (topRebarsValue != null) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.Top);

            foreach (var layer in topRebarsValue) {
              grp.Layers.Add(layer);
            }

            groups.Add(new AdSecRebarGroup(grp));
          }

          // left
          if (LeftRebars.Value != null) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.LeftSide);

            foreach (var layer in LeftRebars.Value) {
              grp.Layers.Add(layer);
            }

            groups.Add(new AdSecRebarGroup(grp));
          }

          // right
          if (RightRebars.Value != null) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.RightSide);

            foreach (var layer in RightRebars.Value) {
              grp.Layers.Add(layer);
            }

            groups.Add(new AdSecRebarGroup(grp));
          }

          // bottom
          if (BottomRebars.Value != null) {
            var grp = ITemplateGroup.Create(ITemplateGroup.Face.Bottom);

            foreach (var layer in BottomRebars.Value) {
              grp.Layers.Add(layer);
            }

            groups.Add(new AdSecRebarGroup(grp));
          }

          if (groups.Count == 0) {
            WarningMessages.Add(
              $"Input parameters {TopRebars.NickName}, {LeftRebars.NickName}, {RightRebars.NickName}, and {BottomRebars.NickName} failed to collect data!");
          }

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
        rebarGroup.Cover = ICover.Create(Length.From(coverSize, LengthUnit));
      }

      //   case FoldMode.Link:
      //     // check for enough input parameters
      //     if (Params.Input[0].SourceCount == 0) {
      //       AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
      //         $"Input parameter {Params.Input[0].NickName} failed to collect data!");
      //       return;
      //     }
      //
      //     // top
      //     if (Params.Input[0].SourceCount != 0) {
      //       var grp = ILinkGroup.Create(this.GetAdSecRebarBundleGoo(da, 0).Value);
      //       groups.Add(new AdSecRebarGroupGoo(grp));
      //     }
      //
      //     break;
      // }
      //
      // for (int i = 0; i < groups.Count; i++) {
      //   if (covers.Count > i) {
      //     groups[i].Cover = covers[i];
      //   } else {
      //     groups[i].Cover = covers.Last();
      //   }
      // }
      //
      // // set output
      // da.SetDataList(0, groups);
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
