using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH;

using OasysUnits;
using OasysUnits.Units;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class CreateSectionGh : CreateSectionFunction {
    public CreateSectionGh() {
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);
      AdSecSection.OnValueChanged += goo => {
        if (goo.Value != null) {
          Section.Value = new SectionDesign() {
            Section = goo.Value.Section,
            DesignCode = goo.Value.DesignCode
          };
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { AdSecSection };
    }
  }

  public class CreateSection : ComponentAdapter<CreateSectionGh> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("af6a8179-5e5f-498c-a83c-e98b90d4464c");

    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateSection;

    protected override void SolveInstance(IGH_DataAccess DA) {
      // 0 profile
      var profile = this.GetAdSecProfileGoo(DA, 0);

      // 1 material
      var material = this.GetAdSecMaterial(DA, 1);

      // 2 Rebars
      var reinforcements = new List<AdSecRebarGroup>();
      if (Params.Input[2].SourceCount > 0) {
        reinforcements = this.GetReinforcementGroups(DA, 2, true);
      }

      // 3 Subcomponents
      var subComponents = Oasys.Collections.IList<ISubComponent>.Create();
      if (Params.Input[3].SourceCount > 0) {
        subComponents = this.GetSubComponents(DA, 3, true);
      }

      // create section
      var section = new AdSecSection(profile.Profile, profile.LocalPlane, material, reinforcements, subComponents);

      if (section.Section.Profile is IPerimeterProfile) {
        section = ClonePerimeter(material, section, profile, reinforcements, subComponents);
      }

      DA.SetData(0, new AdSecSectionGoo(section));
    }

    private static AdSecSection ClonePerimeter(
      AdSecMaterial material, AdSecSection section, AdSecProfileGoo profile, List<AdSecRebarGroup> reinforcements,
      Oasys.Collections.IList<ISubComponent> subComponents) {
      var adSec = IAdSec.Create(material.DesignCode.DesignCode);
      var flattened = adSec.Flatten(section.Section);

      string description = profile.Profile.Description();
      string[] coordinates1 = description.Remove(0, 11).Split(new[] { ") L(", }, StringSplitOptions.None);
      double maxY1 = double.MinValue;
      double maxZ1 = double.MinValue;
      foreach (string c in coordinates1) {
        string[] value = c.Split('|');
        double y1 = double.Parse(value[0]);
        double z1 = double.Parse(value[1].Remove(value[1].Length - 2));

        if (y1 > maxY1) {
          maxY1 = y1;
        }

        if (z1 > maxZ1) {
          maxZ1 = z1;
        }
      }

      string[] coordinates2 = flattened.Profile.Description().Remove(0, 11).Split(new[] {
        ") L(",
      }, StringSplitOptions.None);
      double maxY2 = double.MinValue;
      double maxZ2 = double.MinValue;
      foreach (string c in coordinates2) {
        string[] value = c.Split('|');
        double y2 = double.Parse(value[0]);
        double z2 = double.Parse(value[1].Remove(value[1].Length - 2));

        if (y2 > maxY2) {
          maxY2 = y2;
        }

        if (z2 > maxZ2) {
          maxZ2 = z2;
        }
      }

      double deltaY = maxY2 - maxY1;
      double deltaZ = maxZ2 - maxZ1;

      var updatedReinforcement = new List<AdSecRebarGroup>();
      foreach (var group in reinforcements) {
        var duplicate = new AdSecRebarGroup();
        if (group.Cover != null) {
          duplicate.Cover = ICover.Create(group.Cover.UniformCover);
        }

        updatedReinforcement.Add(duplicate);

        switch (group.Group) {
          case ISingleBars bars:
            var bundle = IBarBundle.Create(bars.BarBundle.Material, bars.BarBundle.Diameter,
              bars.BarBundle.CountPerBundle);
            var singleBars = ISingleBars.Create(bundle);

            foreach (var point in bars.Positions) {
              var p = IPoint.Create(new Length(point.Y.As(LengthUnit.Meter) - deltaY, LengthUnit.Meter),
                new Length(point.Z.As(LengthUnit.Meter) - deltaZ, LengthUnit.Meter));
              singleBars.Positions.Add(p);
            }

            duplicate.Group = singleBars;
            break;

          default:
            duplicate.Group = group.Group;
            break;
        }
      }

      section = new AdSecSection(profile.Profile, profile.LocalPlane, material, updatedReinforcement, subComponents);
      return section;
    }
  }
}
