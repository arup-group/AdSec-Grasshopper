using System;
using System.Drawing;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecCore.Functions {
  public class ConcreteStressStrainGh : ConcreteStressStrainFunction {
    public ConcreteStressStrainGh() {
      var vertex = Vertex as Attribute;
      VertexInput.Update(ref vertex);
      Vertex.OnValueChanged += goo => {
        if (goo?.Value != null) {
          VertexInput.Value = goo.AdSecPoint;
        }
      };
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
