using System;
using System.Collections.Generic;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.Business;

using OasysGH;
using OasysGH.Components;

using Attribute = Oasys.Business.Attribute;

namespace Oasys.GH.Helpers {

  public static class Populator {

    public static Dictionary<Type, Func<Attribute, IGH_Param>> ToGhParam
      = new Dictionary<Type, Func<Attribute, IGH_Param>> {
        {
          typeof(DoubleParameter), a => new Param_Number {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
          }
        }, {
          typeof(PointAttribute), a => new Param_GenericObject {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
          }
        },
      };

    public static Dictionary<Type, Func<Attribute, IGH_Goo>> ToGoo = new Dictionary<Type, Func<Attribute, IGH_Goo>> {
      {
        typeof(PointAttribute), a => new GH_ObjectWrapper {
          Value = new AdSecPointGoo((a as PointAttribute)?.Value),
        }
      },
    };

    public static void PopulateInputParams(this IBusinessComponent businessComponent, GH_Component component) {
      RegisterParams(businessComponent.GetAllInputAttributes(), param => component.Params.RegisterInputParam(param));
    }

    private static void RegisterParams(Attribute[] attributesSelector, Action<IGH_Param> action) {
      foreach (var attribute in attributesSelector) {
        var func = ToGhParam[attribute.GetType()];
        var param = func(attribute);
        action(param);
      }
    }

    public static void PopulateOutputParams(this IBusinessComponent businessComponent, GH_Component component) {
      RegisterParams(businessComponent.GetAllOutputAttributes(), param => component.Params.RegisterOutputParam(param));
    }

    public static void SetValues(this IBusinessComponent businessComponent, GH_Component component) { }
  }

  public abstract class BusinessComponentGlue<T> : GH_OasysDropDownComponent where T : IBusinessComponent {
    private readonly T _businessComponent = Activator.CreateInstance<T>();

    public BusinessComponentGlue(string name, string nickname, string description, string category, string subCategory)
      : base(name, nickname, description, category, subCategory) {
    }

    public override Guid ComponentGuid { get; }

    public override OasysPluginInfo PluginInfo { get; }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      _businessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      _businessComponent.PopulateOutputParams(this);
    }

    public override void SetSelected(int i, int j) { }

    protected override void SolveInternal(IGH_DataAccess da) { }

    protected override void InitialiseDropdowns() { }
  }

  public class DummyBusiness : IBusinessComponent {

    public DoubleParameter Alpha { get; set; } = new DoubleParameter {
      Name = "Alpha",
      NickName = "A",
      Description = "Alpha description",
    };
    public DoubleParameter Beta { get; set; } = new DoubleParameter {
      Name = "Beta",
      NickName = "B",
      Description = "Beta description",
    };

    public Attribute[] GetAllInputAttributes() {
      return new[] {
        Alpha,
      };
    }

    public Attribute[] GetAllOutputAttributes() {
      return new[] {
        Beta,
      };
    }

    public void UpdateInputValues(params object[] values) { }

    public void Compute() { Beta.Value = Alpha.Value * 2; }
  }

  public class DummyComponent : BusinessComponentGlue<DummyBusiness> {
    public DummyComponent() : base("name", "nickname", "description", "category", "subcategory") { }
    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B704-91F214C8C87F");
  }
}
