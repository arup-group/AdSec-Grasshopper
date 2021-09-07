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
using GhAdSec.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GhAdSec.Components
{
    public class CheckULS : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("1337cc01-0b76-4f58-b24e-81e32ae24f92");
        public CheckULS()
          : base("Strength Result", "Strength", "Performs strength analysis (ULS), for a given Load or Deformation. " + System.Environment.NewLine +
                "Can also generate the strength failure surface and the force-moment and moment-moment interaction curves",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat5())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.Analyse;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Solution", "Sol", "AdSec Solution to perform strenght check on", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("FailureSurface", "Fail", "The strength failure surface.", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get section input
            AdSecSection section = GetInput.Section(this, DA, 0);

            // create new adsec instance
            IAdSec adSec = IAdSec.Create(section.DesignCode);

            // analyse
            ISolution solution = adSec.Analyse(section.Section);

            // set output
            DA.SetData(0, new AdSecSolutionGoo(solution));
        }
    }
}
