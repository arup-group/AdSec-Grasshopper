using System;
using Grasshopper.Kernel;
using Oasys.AdSec;

namespace GhAdSec.Components
{
    public class AdSecVersion : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("1b1758e2-3818-42e3-8f79-4b98a7187ca3");
        public AdSecVersion()
          : base("AdSec Plugin Version", "Version", "Get the version of this plugin.",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat0())
        { this.Hidden = true; } // sets the initial state of the component to hidden

        //public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.GsaVersion;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("AdSec Plugin Version", "Plugin", "Plugin version", GH_ParamAccess.item);
            //pManager.AddTextParameter("Location", "File", "Plugin File Location", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_AssemblyInfo adsecplugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));

            DA.SetData(0, adsecplugin.Version);
            DA.SetData(1, IVersion.Api());
        }
    }
}
