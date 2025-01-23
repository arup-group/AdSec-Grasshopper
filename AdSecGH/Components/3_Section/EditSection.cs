using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Components {
  public class EditSection : GH_OasysComponent {

    public EditSection() : base("EditSection", "EditSect", "Edit an AdSec Section", CategoryName.Name(),
      SubCategoryName.Cat4()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("9b0acde5-f57f-4a39-a9c3-cdc935037490");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditSection;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to edit or get information from",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Profile", "Pf", "[Optional] Edit the Profile defining the Section solid boundary",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Material", "Mat", "[Optional] Edit the Material for the section",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("DesignCode", "Code", "[Optional] Edit the Section DesignCode", GH_ParamAccess.item);
      pManager.AddGenericParameter("RebarGroup", "RbG",
        "[Optional] Edit the Reinforcement Groups in the section (applicable for only concrete material).",
        GH_ParamAccess.list);
      pManager.AddGenericParameter("SubComponent", "Sub",
        "[Optional] Edit the Subcomponents contained within the section", GH_ParamAccess.list);

      // make all from second input optional
      for (int i = 1; i < pManager.ParamCount; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "Edited AdSec Section", GH_ParamAccess.item);
      pManager.AddGenericParameter("Profile", "Pf", "Profile defining the Section solid boundary", GH_ParamAccess.item);
      pManager.AddGenericParameter("Material", "Mat", "Material for the section", GH_ParamAccess.item);
      pManager.AddGenericParameter("DesignCode", "Code", "Section DesignCode", GH_ParamAccess.item);
      pManager.AddGenericParameter("RebarGroup", "RbG",
        "Reinforcement Groups in the section (applicable for only concrete material).", GH_ParamAccess.list);
      pManager.AddGenericParameter("SubComponent", "Sub", "Subcomponents contained within the section",
        GH_ParamAccess.list);
      pManager.AddGenericParameter("SectionCurves", "CAD",
        "All curves used for displaying the section - useful for making CAD drawings", GH_ParamAccess.list);
      pManager.HideParameter(7);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // 0 section
      var in_section = AdSecInput.AdSecSection(this, DA, 0);
      if (in_section == null) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          "Input parameter " + Params.Input[0].NickName + " failed to collect data!");
        return;
      }

      // 1 profile
      AdSecProfileGoo profile = null;
      if (Params.Input[1].SourceCount > 0) {
        profile = this.GetAdSecProfileGoo(DA, 1, true);
      } else {
        profile = new AdSecProfileGoo(in_section.Section.Profile, in_section.LocalPlane);
      }

      DA.SetData(1, profile);

      // 2 material
      var material = new AdSecMaterial();
      if (Params.Input[2].SourceCount > 0) {
        material = this.GetAdSecMaterial(DA, 2, true);
      } else {
        material = new AdSecMaterial(in_section.Section.Material, in_section._materialName);
      }
      // wait for potential update to designcode to set material output

      // 3 DesignCode
      if (Params.Input[3].SourceCount > 0) {
        material.DesignCode = this.GetAdSecDesignCode(DA, 3);
      } else {
        material.DesignCode = new AdSecDesignCode(in_section.DesignCode, in_section._codeName);
      }

      DA.SetData(3, new AdSecDesignCodeGoo(material.DesignCode));

      // after potentially changing the design code we can also set the material output now:
      DA.SetData(2, new AdSecMaterialGoo(material));

      // 4 Rebars
      var reinforcements = new List<AdSecRebarGroup>();
      if (Params.Input[4].SourceCount > 0) {
        reinforcements = AdSecInput.ReinforcementGroups(this, DA, 4, true);
      } else {
        foreach (var rebarGrp in in_section.Section.ReinforcementGroups) {
          var rebar = new AdSecRebarGroup(rebarGrp) {
            Cover = in_section.Section.Cover,
          };
          reinforcements.Add(rebar);
        }
      }

      var out_rebars = new List<AdSecRebarGroupGoo>();
      foreach (var rebar in reinforcements) {
        out_rebars.Add(new AdSecRebarGroupGoo(rebar));
      }

      DA.SetDataList(4, out_rebars);

      // 5 Subcomponents
      var subComponents = Oasys.Collections.IList<ISubComponent>.Create();
      if (Params.Input[5].SourceCount > 0) {
        subComponents = AdSecInput.SubComponents(this, DA, 5, true);
      } else {
        subComponents = in_section.Section.SubComponents;
      }

      var out_subComponents = new List<AdSecSubComponentGoo>();
      foreach (var sub in subComponents) {
        var subGoo = new AdSecSubComponentGoo(sub, in_section.LocalPlane, in_section.DesignCode, in_section._codeName,
          in_section._materialName);
        out_subComponents.Add(subGoo);
      }

      DA.SetDataList(5, out_subComponents);

      // create new section
      var out_section = new AdSecSection(profile.Profile, profile.LocalPlane, material, reinforcements, subComponents);

      DA.SetData(0, new AdSecSectionGoo(out_section));

      // ### output section geometry ###
      // collect all curves in this list
      var curves = new List<GH_Curve>();

      GH_Curve ghProfileEdge = null;
      if (GH_Convert.ToGHCurve(out_section.m_profileEdge, GH_Conversion.Both, ref ghProfileEdge)) {
        curves.Add(ghProfileEdge);
      }

      if (out_section.m_profileVoidEdges != null && out_section.m_profileVoidEdges.Count > 0) {
        foreach (var voidEdge in out_section.m_profileVoidEdges) {
          GH_Curve ghVoidEdge = null;
          if (GH_Convert.ToGHCurve(voidEdge, GH_Conversion.Both, ref ghVoidEdge)) {
            curves.Add(ghVoidEdge);
          }
        }
      }

      if (out_section.m_rebarEdges != null && out_section.m_rebarEdges.Count > 0) {
        foreach (var rebar in out_section.m_rebarEdges) {
          GH_Curve ghRebar = null;
          if (GH_Convert.ToGHCurve(rebar, GH_Conversion.Both, ref ghRebar)) {
            curves.Add(ghRebar);
          }
        }
      }

      if (out_section.m_linkEdges != null && out_section.m_linkEdges.Count > 0) {
        foreach (var link in out_section.m_linkEdges) {
          GH_Curve ghLink = null;
          if (GH_Convert.ToGHCurve(link, GH_Conversion.Both, ref ghLink)) {
            curves.Add(ghLink);
          }
        }
      }

      if (out_section.m_subEdges != null && out_section.m_subEdges.Count > 0) {
        foreach (var subEdge in out_section.m_subEdges) {
          GH_Curve ghSubEdge = null;
          if (GH_Convert.ToGHCurve(subEdge, GH_Conversion.Both, ref ghSubEdge)) {
            curves.Add(ghSubEdge);
          }
        }
      }

      if (out_section.m_subVoidEdges != null && out_section.m_subVoidEdges.Count > 0) {
        foreach (var subVoidEdges in out_section.m_subVoidEdges) {
          foreach (var subVoidEdge in subVoidEdges) {
            GH_Curve ghSubEdge = null;
            if (GH_Convert.ToGHCurve(subVoidEdge, GH_Conversion.Both, ref ghSubEdge)) {
              curves.Add(ghSubEdge);
            }
          }
        }
      }

      DA.SetDataList(6, curves);
    }
  }
}
