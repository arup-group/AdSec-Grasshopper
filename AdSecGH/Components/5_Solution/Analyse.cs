using System;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.AdSec;

using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Components {
  public class Analyse : GH_OasysComponent {

    public Analyse() : base("Analyse Section", "Analyse", "Analyse an AdSec Section", CategoryName.Name(),
      SubCategoryName.Cat6()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("4621cc01-0b76-4f58-b24e-81e32ae24f92");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Solution;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to analyse", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Results", "Res",
        "AdSec Results for a Section. Results object allows to calculate strength (ULS) and serviceability (SLS) results.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("FailureSurface", "Fail", "Mesh representing the strength failure surface.",
        GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get section input
      var section = this.GetAdSecSection(DA, 0);

      // create new adsec instance
      var adSec = IAdSec.Create(section.DesignCode);

      // analyse
      var solution = adSec.Analyse(section.Section);

      // display warnings
      var warnings = solution.Warnings;
      foreach (var warn in warnings) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warn.Description);
      }

      // set outputs
      DA.SetData(0, new AdSecSolutionGoo(solution, section));
      DA.SetData(1, new AdSecFailureSurfaceGoo(solution.Strength.GetFailureSurface(), section.LocalPlane));
    }
  }
}
