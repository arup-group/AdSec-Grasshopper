using System;
using System.Collections.Generic;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateSection : GH_OasysComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("af6a8179-5e5f-498c-a83c-e98b90d4464c");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateSection;

    public CreateSection() : base(
      "Create Section",
      "Section",
      "Create an AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat4()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Profile", "Pf", "AdSec Profile defining the Section solid boundary", GH_ParamAccess.item);
      pManager.AddGenericParameter("Material", "Mat", "AdSet Material for the section. The DesignCode of this material will be used for analysis", GH_ParamAccess.item);
      pManager.AddGenericParameter("RebarGroup", "RbG", "[Optional] AdSec Reinforcement Groups in the section (applicable for only concrete material).", GH_ParamAccess.list);
      pManager.AddGenericParameter("SubComponent", "Sub", "[Optional] AdSet Subcomponents contained within the section", GH_ParamAccess.list);

      // make all from second input optional
      for (int i = 2; i < pManager.ParamCount; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // 0 profile
      AdSecProfileGoo profile = AdSecInput.AdSecProfileGoo(this, DA, 0);

      // 1 material
      AdSecMaterial material = AdSecInput.AdSecMaterial(this, DA, 1);

      // 2 Rebars
      var reinforcements = new List<AdSecRebarGroup>();
      if (Params.Input[2].SourceCount > 0) {
        reinforcements = AdSecInput.ReinforcementGroups(this, DA, 2, true);
      }

      // 3 Subcomponents
      var subComponents = Oasys.Collections.IList<ISubComponent>.Create();
      if (Params.Input[3].SourceCount > 0) {
        subComponents = AdSecInput.SubComponents(this, DA, 3, true);
      }

      // create section
      var section = new AdSecSection(profile.Profile, profile.LocalPlane, material, reinforcements, subComponents);

      if (section.Section.Profile is IPerimeterProfile) {
        var adSec = IAdSec.Create(material.DesignCode.DesignCode);
        ISection flattened = adSec.Flatten(section.Section);

        string foo1 = profile.Profile.Description();
        string foo2 = flattened.Profile.Description();

        string[] coordinates1 = profile.Profile.Description().Remove(0, 11).Split(new[] { ") L(" }, StringSplitOptions.None);
        double maxY1 = double.MinValue;
        double maxZ1 = double.MinValue;
        foreach(string c in coordinates1) {
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

        string[] coordinates2 = flattened.Profile.Description().Remove(0, 11).Split(new[] { ") L(" }, StringSplitOptions.None);
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
        foreach (AdSecRebarGroup group in reinforcements) {
          var duplicate = new AdSecRebarGroup();
          if(group.Cover != null) {
            duplicate.Cover = ICover.Create(group.Cover.UniformCover);
          }

          updatedReinforcement.Add(duplicate);

          switch (group.Group) {
            case ISingleBars bars:
              var bundle = IBarBundle.Create(bars.BarBundle.Material, bars.BarBundle.Diameter, bars.BarBundle.CountPerBundle);
              var singleBars = ISingleBars.Create(bundle);

              foreach (IPoint point in bars.Positions) {
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
      }

      DA.SetData(0, new AdSecSectionGoo(section));
    }
  }
}
