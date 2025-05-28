using System;
using System.Drawing;
using System.Linq;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
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
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);
      AdSecSection.OnValueChanged += goo => {
        if (goo?.Value != null) {
          Section.Value = new SectionDesign() {
            Section = goo.Value.Section,
            DesignCode = new DesignCode() {
              IDesignCode = goo.Value.DesignCode,
              DesignCodeName = goo.Value._codeName,
            },
            LocalPlane = goo.Value.LocalPlane.ToOasys()
          };
        }
      };

      var adSecPoint = AdSecPoint as Attribute;
      Position.Update(ref adSecPoint);
      Position.OnValueChanged += goo => {
        AdSecPoint.Value = goo.Select(x => new AdSecPointGoo(x, Section.Value.LocalPlane)).ToArray();
      };

      AdSecPoint.Name = Position.NameWithUnits(DefaultUnits.LengthUnitGeometry);
      Diameter.Name = Diameter.NameWithUnits(DefaultUnits.LengthUnitGeometry);
    }

    public AdSecPointArrayParameter AdSecPoint { get; set; } = new AdSecPointArrayParameter();
    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

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
