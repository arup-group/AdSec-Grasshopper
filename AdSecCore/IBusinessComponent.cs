using System;
using System.Linq;

using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace Oasys.Business {

  public interface IBusinessComponent {

    ComponentAttribute Metadata { get; set; }
    ComponentOrganisation Organisation { get; set; }
    Attribute[] GetAllInputAttributes();
    Attribute[] GetAllOutputAttributes();
    void UpdateInputValues(params object[] values);
    void Compute();
  }

  public class ComponentAttribute {
    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
  }

  public class ComponentOrganisation {
    public string Category { get; set; }
    public string SubCategory { get; set; }
  }

  public class Attribute {

    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
  }

  public interface IDefault {
    void SetDefault();
  }

  public class ParameterAttribute<T> : Attribute, IDefault {
    public T Value { get; set; }
    public T Default { get; set; }

    public void SetDefault() {
      Value = Default;
    }
  }

  public class DoubleParameter : ParameterAttribute<double> { }
  public class PointAttribute : ParameterAttribute<IPoint> { }

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

    public ComponentAttribute Metadata { get; set; }
    public ComponentOrganisation Organisation { get; set; }

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

    public void GetDefaultValues() { throw new NotImplementedException(); }

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

}
