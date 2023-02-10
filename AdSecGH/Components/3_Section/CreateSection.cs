using System;
using System.Collections.Generic;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Components
{
    public class CreateSection : GH_OasysComponent
  {
    #region Name and Ribbon Layout
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
      SubCategoryName.Cat4())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Profile", "Pf", "AdSec Profile defining the Section solid boundary", GH_ParamAccess.item);
      pManager.AddGenericParameter("Material", "Mat", "AdSet Material for the section. The DesignCode of this material will be used for analysis", GH_ParamAccess.item);
      pManager.AddGenericParameter("RebarGroup", "RbG", "[Optional] AdSec Reinforcement Groups in the section (applicable for only concrete material).", GH_ParamAccess.list);
      pManager.AddGenericParameter("SubComponent", "Sub", "[Optional] AdSet Subcomponents contained within the section", GH_ParamAccess.list);

      // make all from second input optional
      for (int i = 2; i < pManager.ParamCount; i++)
        pManager[i].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // 0 profile
      AdSecProfileGoo profile = AdSecInput.AdSecProfileGoo(this, DA, 0);

      // 1 material
      AdSecMaterial material = AdSecInput.AdSecMaterial(this, DA, 1);

      // 2 Rebars
      List<AdSecRebarGroup> reinforcements = new List<AdSecRebarGroup>();
      if (Params.Input[2].SourceCount > 0)
        reinforcements = AdSecInput.ReinforcementGroups(this, DA, 2, true);

      // 3 Subcomponents
      Oasys.Collections.IList<ISubComponent> subComponents = Oasys.Collections.IList<ISubComponent>.Create();
      if (Params.Input[3].SourceCount > 0)
        subComponents = AdSecInput.SubComponents(this, DA, 3, true);

      // create section
      AdSecSection section = new AdSecSection(profile.Profile, profile.LocalPlane, material, reinforcements, subComponents);

      DA.SetData(0, new AdSecSectionGoo(section));
    }
  }
}

