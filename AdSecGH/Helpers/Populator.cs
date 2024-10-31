using System;
using System.Collections.Generic;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.Business;

using OasysGH;
using OasysGH.Components;

using Attribute = Oasys.Business.Attribute;

namespace Oasys.GH.Helpers {

  public static class Populator {

    private static readonly Dictionary<Type, Func<Attribute, IGH_Param>> ToGhParam
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

    private static readonly Dictionary<Type, Func<Attribute, IGH_Goo>> ToGoo
      = new Dictionary<Type, Func<Attribute, IGH_Goo>> {
        {
          typeof(PointAttribute), a => new GH_ObjectWrapper {
            Value = new AdSecPointGoo((a as PointAttribute)?.Value),
          }
        }, {
          typeof(DoubleParameter), a => new GH_Number((a as DoubleParameter).Value)
        },
      };

    public static void SetDefaultValues(this IBusinessComponent businessComponent) {
      foreach (var attribute in businessComponent.GetAllInputAttributes()) {
        if (attribute is IDefault @default) {
          @default.SetDefault();
        }
      }
    }

    public static void SetDefaultValues(this IBusinessComponent businessComponent, GH_Component component) {
      businessComponent.SetDefaultValues();
      foreach (var attribute in businessComponent.GetAllInputAttributes()) {
        int index = component.Params.IndexOfInputParam(attribute.Name);
        var param = component.Params.Input[index];
        var goo = ToGoo[attribute.GetType()](attribute);
        param.AddVolatileData(new GH_Path(0), 0, goo);
      }
    }

    public static void SetOutputValues(
      this IBusinessComponent businessComponent, GH_Component component, IGH_DataAccess dataAccess) {
      foreach (var attribute in businessComponent.GetAllOutputAttributes()) {
        int index = component.Params.IndexOfOutputParam(attribute.Name);
        var goo = ToGoo[attribute.GetType()](attribute);
        dataAccess.SetData(index, goo);
      }
    }

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
  }

  public abstract class BusinessOasysDropdownGlue<T> : GH_OasysDropDownComponent, IDefaultValues
    where T : IBusinessComponent {
    private readonly T _businessComponent = Activator.CreateInstance<T>();

    public BusinessOasysDropdownGlue(
      string name, string nickname, string description, string category, string subCategory) : base(name, nickname,
      description, category, subCategory) { }

    public override Guid ComponentGuid { get; }

    public override OasysPluginInfo PluginInfo { get; }

    public void SetDefaultValues() { _businessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      _businessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      _businessComponent.PopulateOutputParams(this);
    }

    public override void SetSelected(int i, int j) { }

    protected override void SolveInternal(IGH_DataAccess da) {
      _businessComponent.Compute();
      _businessComponent.SetOutputValues(this, da);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>();
      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();
      _isInitialised = true;
    }
  }

  public abstract class BusinessOasysGlue<T> : GH_OasysComponent, IDefaultValues where T : IBusinessComponent {

    private readonly T _businessComponent = Activator.CreateInstance<T>();

    public BusinessOasysGlue(string name, string nickname, string description, string category, string subCategory) :
      base(name, nickname, description, category, subCategory) { }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;
    public void SetDefaultValues() { _businessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      _businessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      _businessComponent.PopulateOutputParams(this);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      _businessComponent.Compute();
      _businessComponent.SetOutputValues(this, DA);
    }
  }

  public interface IDefaultValues {
    void SetDefaultValues();
  }

  public class DummyBusiness : IBusinessComponent {

    public DoubleParameter Alpha { get; set; } = new DoubleParameter {
      Name = "Alpha",
      NickName = "A",
      Description = "Alpha description",
      Default = 1,
    };
    public DoubleParameter Beta { get; set; } = new DoubleParameter {
      Name = "Beta",
      NickName = "B",
      Description = "Beta description",
      Default = 2,
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

    public void Compute() { Beta.Value = (Alpha.Value * 2) + 10; }

    public void GetDefaultValues() { throw new NotImplementedException(); }
  }

  public static class Test {

    public const string thisIsForTestingPurposesOnly = "this is for testing purposes only";
  }

  public class DummyOasysComponent : BusinessOasysGlue<DummyBusiness> {

    public DummyOasysComponent() : base("Oasys Component Glue", "OCG", Test.thisIsForTestingPurposesOnly, "Oasys",
      "dummy") { }

    public override GH_Exposure Exposure { get; } = GH_Exposure.hidden;

    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B734-91F214C8B87F");

    protected override void SolveInstance(IGH_DataAccess DA) { }
  }

  public class DummyOasysDropdown : BusinessOasysDropdownGlue<DummyBusiness> {
    public DummyOasysDropdown() : base("Business Dropdown Glue", "BDG", "description", "Oasys", "Dummy") { }
    public override GH_Exposure Exposure { get; } = GH_Exposure.hidden;
    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B704-91F214C8C871");
  }
}
