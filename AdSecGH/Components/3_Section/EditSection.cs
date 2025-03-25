using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysGH;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class EditSectionGh : EditSectionFunction {

    public EditSectionGh() {
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

      var adSecSectionOut = AdSecSectionOut as Attribute;
      SectionOut.Update(ref adSecSectionOut);
      SectionOut.OnValueChanged += design => {
        if (design != null) {
          AdSecSectionOut.Value = new AdSecSectionGoo(new AdSecSection(design));
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();
    public AdSecSectionParameter AdSecSectionOut { get; set; } = new AdSecSectionParameter();

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection, Profile, Material, DesignCode, RebarGroup, SubComponent,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecSectionOut, ProfileOut, MaterialOut, DesignCodeOut, RebarGroupOut, SubComponentOut, Geometry,
      };
    }
  }

  public class EditSection : ComponentAdapter<EditSectionGh> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("9b0acde5-f57f-4a39-a9c3-cdc935037490");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditSection;

    protected override void SolveInstance(IGH_DataAccess DA) {
      base.SolveInstance(DA);
      // var sectionDesign = BusinessComponent.AdSecSection.Value;
      // var in_section = sectionDesign.Value;
      // var profile = new AdSecProfileGoo(BusinessComponent.Profile.Value
      // ?? ProfileDesign.From(BusinessComponent.Section.Value));

      // DA.SetData(1, profile);

      // 2 material
      // var material = new AdSecMaterial();
      // if (Params.Input[2].SourceCount > 0) {
      // material = this.GetAdSecMaterial(DA, 2, true);
      // } else {
      // material = new AdSecMaterial(in_section.Section.Material, in_section._materialName);
      // }
      // wait for potential update to designcode to set material output

      // 3 DesignCode
      // if (Params.Input[3].SourceCount > 0) {
      //   material.DesignCode = this.GetAdSecDesignCode(DA, 3);
      // } else {
      //   material.DesignCode = new AdSecDesignCode(in_section.DesignCode, in_section._codeName);
      // }
      //
      // DA.SetData(3, new AdSecDesignCodeGoo(material.DesignCode));

      // after potentially changing the design code we can also set the material output now:
      // DA.SetData(2, new AdSecMaterialGoo(new MaterialDesign {
      //   Material = material.Material,
      //   DesignCode = material.DesignCode.DesignCode,
      // }));

      // 4 Rebars
      // var reinforcements = new List<AdSecRebarGroup>();
      // if (Params.Input[4].SourceCount > 0) {
      //   reinforcements = this.GetReinforcementGroups(DA, 4, true);
      // } else {
      //   foreach (var rebarGrp in in_section.Section.ReinforcementGroups) {
      //     var rebar = new AdSecRebarGroup(rebarGrp) {
      //       Cover = in_section.Section.Cover,
      //     };
      //     reinforcements.Add(rebar);
      //   }
      // }

      // var out_rebars = new List<AdSecRebarGroupGoo>();
      // foreach (var rebar in reinforcements) {
      //   out_rebars.Add(new AdSecRebarGroupGoo(rebar));
      // }

      // DA.SetDataList(4, out_rebars);

      // 5 Subcomponents
      // Oasys.Collections.IList<ISubComponent> subComponents;
      // if (Params.Input[5].SourceCount > 0) {
      //   subComponents = this.GetSubComponents(DA, 5, true);
      // } else {
      //   subComponents = in_section.Section.SubComponents;
      // }

      // var out_subComponents = new List<AdSecSubComponentGoo>();
      // foreach (var sub in subComponents) {
      //   var subGoo = new AdSecSubComponentGoo(sub, in_section.LocalPlane, in_section.DesignCode, in_section._codeName,
      //     in_section._materialName);
      //   out_subComponents.Add(subGoo);
      // }
      //
      // DA.SetDataList(5, out_subComponents);

      // create new section
      // var out_section = new AdSecSection(profile.Profile, profile.LocalPlane, material, reinforcements, subComponents);

      var adSecSectionGoo = BusinessComponent.AdSecSectionOut.Value;
      adSecSectionGoo.UpdateGeometryRepresentation();
      // DA.SetData(0, adSecSectionGoo);

      // ### output section geometry ###
      // collect all curves in this list
      var curves = adSecSectionGoo._drawInstructions.Select(x => {
        GH_Curve curve = null;
        GH_Convert.ToGHCurve(x.Geometry, GH_Conversion.Both, ref curve);
        return curve;
      }).ToList();
      DA.SetDataList(6, curves);
    }
  }
}
