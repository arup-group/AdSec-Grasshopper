using System;
using System.Diagnostics;

using AdSecGHCore.Constants;

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
    public override void Compute() { throw new System.NotImplementedException(); }

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
