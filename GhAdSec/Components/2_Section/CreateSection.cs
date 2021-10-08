using System;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.Units;
using UnitsNet;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using AdSecGH.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;

namespace AdSecGH.Components
{
    public class CreateSection : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("af6a8179-5e5f-498c-a83c-e98b90d4464c");
        public CreateSection()
          : base("Section", "Section", "Create an AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.Section;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Profile", "Pf", "AdSec Profile defining the Section solid boundary", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Mat", "AdSet Material for the section. The DesignCode of this material will be used for analysis", GH_ParamAccess.item);
            pManager.AddGenericParameter("RebarGroup", "RbG", "[Optional] AdSec Reinforcement Groups in the section (applicable for only concrete material).", GH_ParamAccess.list);
            pManager.AddGenericParameter("SubComponent", "Sub", "[Optional] AdSet Subcomponents contained within the section", GH_ParamAccess.list);
            
            // make all from second input optional
            for (int i = 2; i < pManager.ParamCount; i++)
                pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Section", "Sec", "AdSet Section", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 0 profile
            AdSecProfileGoo profile = GetInput.AdSecProfileGoo(this, DA, 0);

            // 1 material
            AdSecMaterial material = GetInput.AdSecMaterial(this, DA, 1);

            // 2 Rebars
            List<AdSecRebarGroupGoo> reinforcements = new List<AdSecRebarGroupGoo>();
            if (Params.Input[2].SourceCount > 0)
            {
                reinforcements = GetInput.ReinforcementGroups(this, DA, 2, true);
            }

            // 3 Subcomponents
            Oasys.Collections.IList<Oasys.AdSec.ISubComponent> subComponents = Oasys.Collections.IList<Oasys.AdSec.ISubComponent>.Create();
            if (Params.Input[3].SourceCount > 0)
            {
                subComponents = GetInput.SubComponents(this, DA, 3, true);
            }

            // create section
            AdSecSection section = new AdSecSection(profile.Profile, profile.LocalPlane, material, reinforcements, subComponents);

            DA.SetData(0, new AdSecSectionGoo(section));
        }
    }
}
