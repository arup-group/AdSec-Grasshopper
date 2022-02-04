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
    public class Analyse : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("4621cc01-0b76-4f58-b24e-81e32ae24f92");
        public Analyse()
          : base("Analyse Section", "Analyse", "Analyse an AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat6())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Solution;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Section", "Sec", "AdSec Section to analyse", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Res", "AdSec Results for a Section. Results object allows to calculate strength (ULS) and serviceability (SLS) results.", GH_ParamAccess.item);
            pManager.AddGenericParameter("FailureSurface", "Fail", "Mesh representing the strength failure surface.", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get section input
            AdSecSection section = GetInput.AdSecSection(this, DA, 0);

            // create new adsec instance
            IAdSec adSec = IAdSec.Create(section.DesignCode);

            // analyse
            ISolution solution = adSec.Analyse(section.Section);

            // display warnings
            Oasys.Collections.IList<IWarning> warnings = solution.Warnings;
            foreach (IWarning warn in warnings)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warn.Description);

            // set outputs
            DA.SetData(0, new AdSecSolutionGoo(solution, section));
            DA.SetData(1, new AdSecFailureSurfaceGoo(solution.Strength.GetFailureSurface(), section.LocalPlane));
        }
    }
}
