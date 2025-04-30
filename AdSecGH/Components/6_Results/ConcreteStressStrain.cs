using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

using Attribute = AdSecCore.Functions.Attribute;
namespace AdSecCore.Functions {
  public class ConcreteStressStrainGh : ConcreteStressStrainFunction {
    public ConcreteStressStrainGh() {
      var vertex = Vertex as Attribute;
      VertexInput.Update(ref vertex);
      Vertex.OnValueChanged += goo => { VertexInput.Value = goo.AdSecPoint; };
    }

    public AdSecPointParameter Vertex { get; set; } = new AdSecPointParameter();

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
       SolutionInput,
       LoadInput,
       Vertex,
      };
    }
  }



  public class ConcreteStressStrain : ComponentAdapter<ConcreteStressStrainGh> {

    protected override void BeforeSolveInstance() {
      this.UpdateDefaultUnits();
      this.RefreshParameter();
    }

    public ConcreteStressStrain() {
      Hidden = true;
      Category = CategoryName.Name();
      SubCategory = SubCategoryName.Cat7();
    }

    public override Guid ComponentGuid => new Guid("542fc96d-d90a-4301-855f-d14507cc9753");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainConcrete;
  }
}
