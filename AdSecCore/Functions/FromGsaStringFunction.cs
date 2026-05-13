using AdSecCore.Constants;
using AdSecCore.Parameters;

using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class FromGsaStringFunction : Function {

    public StringParameter Input { get; set; } = new StringParameter {
      Name = "GSA Material",
      NickName = "GSA",
      Description = "GSA Material string to convert to an AdSec catalogue material",
    };

    public MaterialParameter Material { get; set; }
      = Default.Material(description: "AdSec catalogue concrete material derived from the GSA material string");

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Material from GSA",
      NickName = "MatGSA",
      Description = "Create an AdSec catalogue concrete material from a GSA material string",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat1(),
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Input };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { Material };
    }

    public override void Compute() {
      if (!GsaMaterialParser.TryParseConcreteMaterial(Input.Value, out var materialDesign, out string warning)) {
        ErrorMessages.Add(
          $"Could not parse GSA material string: '{Input.Value}'.{System.Environment.NewLine}"
          + "Expected format: 'GSA Material (<code> Concrete <grade>)'. "
          + "Only Concrete materials with a supported design code are accepted.");
        return;
      }

      if (warning != null) {
        WarningMessages.Add(warning);
      }

      Material.Value = materialDesign;
    }
  }
}
