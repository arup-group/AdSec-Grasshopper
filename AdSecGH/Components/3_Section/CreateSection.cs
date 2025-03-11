using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class CreateSectionGh : CreateSectionFunction {
    public CreateSectionGh() {
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);
      AdSecSection.OnValueChanged += goo => {
        if (goo.Value != null) {
          Section.Value = new SectionDesign {
            Section = goo.Value.Section,
            DesignCode = goo.Value.DesignCode,
          };
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { AdSecSection, };
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

      var profileProfile = profile.Profile;
      var materialMaterial = material.Material;
      var designCodeDesignCode = material.DesignCode.DesignCode;

      if (profile.Profile is IPerimeterProfile) {
        reinforcements = SectionBuilder.CalibrateReinforcementGroupsForIPerimeterProfile(reinforcements,
          designCodeDesignCode, profileProfile, materialMaterial);
      }

      var section = new AdSecSection(profileProfile, profile.LocalPlane, material, reinforcements, subComponents);

      DA.SetData(0, new AdSecSectionGoo(section));
    }
  }
}
