using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.Business;
using Oasys.Business.Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace Oasys.Business {

  public interface IBusinessComponent {
    Attribute[] GetAllInputAttributes();
    Attribute[] GetAllOutputAttributes();
    void UpdateInputValues(params object[] values);
    void Compute();
  }

  public class Attribute {

    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
  }

  public class ParameterAttribute<T> : Attribute {
    public T Value { get; set; }
  }

  public class DoubleParameter : ParameterAttribute<double> { }
  public class PointAttribute : ParameterAttribute<IPoint> { }

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
        RegisterParams(businessComponent.GetAllOutputAttributes(),
          param => component.Params.RegisterOutputParam(param));
      }

      public static void SetValues(this IBusinessComponent businessComponent, GH_Component component) { }
    }
  }

  public class IPointWrapper : IBusinessComponent {

    public DoubleParameter Z { get; set; } = new DoubleParameter {
      Name = "Z",
    };

    public DoubleParameter Y { get; set; } = new DoubleParameter {
      Name = "Y",
    };

    // pManager.AddGenericParameter("Vertex Point", "Vx",
    // "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement", GH_ParamAccess.item);
    public PointAttribute Point { get; set; } = new PointAttribute {
      Name = "Vertex Point",
      NickName = "Vx",
      Description = "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement",
    };

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() {
      Point.Value = IPoint.Create(new Length(Y.Value, LengthUnit.Meter), new Length(Z.Value, LengthUnit.Meter));
    }

    public Attribute[] GetAllInputAttributes() {
      return new[] {
        Y,
        Z,
      };
    }

    public Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Point,
      };
    }

    public void SetInputValue<T>(string name, T value) {
      var attribute = GetInputAttribute<T>(name);
      attribute.Value = value;
    }

    public ParameterAttribute<T> GetInputAttribute<T>(string name) {
      return (ParameterAttribute<T>)GetInputAttribute(name);
    }

    private Attribute GetInputAttribute(string name) {
      return GetAllInputAttributes().FirstOrDefault(a => a.Name == name);
    }

    public Attribute GetOutputAttribute(string name) {
      return GetAllOutputAttributes().FirstOrDefault(a => a.Name == name);
    }

    public ParameterAttribute<T> GetOutputAttribute<T>(string name) {
      return (ParameterAttribute<T>)GetOutputAttribute(name);
    }
  }

  public abstract class BusinessComponentGlue<T> : GH_OasysDropDownComponent where T : IBusinessComponent {
    private T _businessComponent;

    public BusinessComponentGlue(string name, string nickname, string description, string category, string subCategory)
      : base(name, nickname, description, category, subCategory) { }

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

}

namespace AdSecGH.Components {
  public class CreatePoint : GH_OasysDropDownComponent {
    private readonly IPointWrapper _pointWrapper;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;

    public CreatePoint() : base("Create Vertex Point", "Vertex Point",
      "Create a 2D vertex in local yz-plane for AdSec Profile and Reinforcement", CategoryName.Name(),
      SubCategoryName.Cat3()) {
      Hidden = false; // sets the initial state of the component to hidden
      _pointWrapper = new IPointWrapper();
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("1a0cdb3c-d66d-420e-a9d8-35d31587a122");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.VertexPoint;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      Params.Input[0].Name = "Y [" + unitAbbreviation + "]";
      Params.Input[1].Name = "Z [" + unitAbbreviation + "]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter("Y [" + unitAbbreviation + "]", "Y", "The local Y coordinate in yz-plane",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Z [" + unitAbbreviation + "]", "Z", "The local Z coordinate in yz-plane",
        GH_ParamAccess.item);
      // Helper Method to populate pManager with input parameters
      // _pointWrapper.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      _pointWrapper.PopulateOutputParams(this);
      pManager.AddGenericParameter("Vertex Point", "Vx",
        "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement", GH_ParamAccess.item);
      // _pointWrapper.PopulateOutputParams(this);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // get inputs
      var y = (Length)Input.UnitNumber(this, DA, 0, _lengthUnit);
      var z = (Length)Input.UnitNumber(this, DA, 1, _lengthUnit);

      // create IPoint
      var pt = IPoint.Create(y, z);
      // var pointWrapper = new IPointWrapper {
      //   Y = new DoubleParameter {
      //     Value = y.Value,
      //   },
      //   Z = new DoubleParameter {
      //     Value = z.Value,
      //   },
      // };
      // pointWrapper.Compute();
      // var outputs = pointWrapper.GetAllInputAttributes();
      // var pointW = pointWrapper.GetOutputAttribute<IPoint>("Point");
      // DA.SetData(0, new AdSecPointGoo(pointW.Value));

      // Convert to AdSecPointGoo param
      var point = new AdSecPointGoo(pt);

      // set output
      DA.SetData(0, point);
    }

    protected override void UpdateUIFromSelectedItems() {
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
