using System;
using System.Drawing;
using System.Linq;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class FlattenRebarGh : FlattenRebarFunction {

    public FlattenRebarGh() {
      AdSecSection.OnValueChanged += goo => {
        Section.Value = goo.Value?.Section;
      };
      Position.OnValueChanged += goo => {
        AdSecPoint.Value = goo.Select(x => new AdSecPointGoo(x)).ToArray();
      };
      var adSecSection = AdSecSection as Attribute;
      FromAttribute(ref adSecSection, Section);
      var adSecPoint = AdSecPoint as Attribute;
      FromAttribute(ref adSecPoint, Position);
      AdSecPoint.Name = Position.NameWithUnits(DefaultUnits.LengthUnitGeometry);
      Diameter.Name = Diameter.NameWithUnits(DefaultUnits.LengthUnitGeometry);
    }

    public AdSecPointArrayParameter AdSecPoint { get; set; } = new AdSecPointArrayParameter();
    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

    private static void FromAttribute(ref Attribute update, Attribute from) {
      update.Name = from.Name;
      update.NickName = from.NickName;
      update.Description = from.Description;

      if (from is IAccessible accessible && update is IAccessible AdSecSection) {
        AdSecSection.Access = accessible.Access;
      }
    }

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecPoint,
        Diameter,
        BundleCount,
        PreLoad,
        Material,
      };
    }
  }

  public class FlattenRebar : ComponentAdapter<FlattenRebarGh> {

    public FlattenRebar() {
      Hidden = true;
    }

    public override Guid ComponentGuid => new Guid("879ecac6-464d-4286-9113-c15fcf03e4e6");

    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.FlattenRebar;
  }
}
