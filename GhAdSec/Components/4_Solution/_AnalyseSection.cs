//using Grasshopper.Kernel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using GrasshopperAsyncComponent;
//using Oasys.AdSec;
//using GhAdSec.Parameters;
//using Oasys.AdSec.DesignCode;
//using Oasys.Profiles;
//using Oasys.AdSec.Materials;

//namespace GhAdSec.Components
//{
//    public class AnalyseSection : GH_AsyncComponent
//    {
//        #region Name and Ribbon Layout
//        // This region handles how the component in displayed on the ribbon
//        // including name, exposure level and icon
//        public override Guid ComponentGuid => new Guid("2f76e252-c57c-4582-8bdc-719d352dac59");
//        public AnalyseSection()
//          : base("Section", "Section", "Create an AdSec Section",
//                Ribbon.CategoryName.Name(),
//                Ribbon.SubCategoryName.Cat4())
//        {
//            BaseWorker = new SolutionWorker();
//            TaskCreationOptions = System.Threading.Tasks.TaskCreationOptions.AttachedToParent;
//            this.Hidden = false; } // sets the initial state of the component to hidden

//        public override GH_Exposure Exposure => GH_Exposure.primary;

//        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.Analyse;
//        #endregion


//        #region Custom UI
//        //This region overrides the typical component layout
//        #endregion

//        #region Input and output

//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddGenericParameter("Section", "Sec", "AdSec Section to analyse", GH_ParamAccess.item);
//        }

//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddGenericParameter("Solution", "Sol", "AdSec Solution for a Section. A Solution allows to calculate strength and serviceability results.", GH_ParamAccess.item);
//        }
//        #endregion

//        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
//        {
//            base.AppendAdditionalMenuItems(menu);
//            Menu_AppendItem(menu, "Cancel", (s, e) =>
//            {
//                RequestCancellation();
//            });
//        }
//    }

//    public class SolutionWorker : WorkerInstance
//    {
//        public SolutionWorker() : base(null) { }
//        public override WorkerInstance Duplicate() => new SolutionWorker();
//        ISolution Solution { get; set; }
//        AdSecSection Section { get; set; }
//        //IDesignCode DesignCode { get; set; }
//        //IAdSec AdSec { get; set; }
//        public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
//        {

//            // get section input
//            Section = GetInput.Section(this.Parent, DA, 0);

//        }

//        public override void DoWork(Action<string, double> ReportProgress, Action Done)
//        {
//            //for (int i = 0; i < 1; i++)
//            //{

//            //}

//            //AdSecSection clone = Section.Duplicate();

//            //// create new adsec instance
//            IAdSec adSec = IAdSec.Create(Section.DesignCode);

//            IProfile profile = IPerimeterProfile.Create(Section.Section.Profile);
//            IMaterial mat = Section.Section.Material;
//            ISection newSection = ISection.Create(profile, mat);
            
//            //// analyse
//            Solution = adSec.Analyse(newSection);

//            Done();
//        }

//        public override void SetData(IGH_DataAccess DA)
//        {
//            // 👉 Checking for cancellation!
//            if (CancellationToken.IsCancellationRequested) { return; }

//            DA.SetData(0, new AdSecSolutionGoo(Solution));
//        }
//    }

//}
