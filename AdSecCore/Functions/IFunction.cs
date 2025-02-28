using System;
namespace AdSecCore.Functions {

  public interface IFunction {

    FuncAttribute Metadata { get; set; }
    Organisation Organisation { get; set; }
    Attribute[] GetAllInputAttributes();

    Attribute[] GetAllOutputAttributes();

    void Compute();
  }

  public class FuncAttribute {
    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
  }

  public class Organisation {
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public bool Hidden { get; set; }
  }

  public class Attribute {

    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
    public bool Optional { get; set; } = true;

    public void Update(ref Attribute update) {
      update.Name = Name;
      update.NickName = NickName;
      update.Description = Description;
      update.Optional = Optional;

      if (this is IAccessible accessible && update is IAccessible AdSecSection) {
        AdSecSection.Access = accessible.Access;
      }
    }
  }

  public enum Access {
    Item,
    List,
  }

  public interface IDefault {
    void SetDefault();
  }

  public interface IAccessible {
    Access Access { get; set; }
  }

  public class ParameterAttribute<T> : Attribute, IDefault, IAccessible {
    private T _value;
    public T Value {
      get => _value;
      set {
        _value = value;
        OnValueChanged?.Invoke(value);
      }
    }
    public T Default { get; set; }
    public virtual Access Access { get; set; } = Access.Item;

    public void SetDefault() {
      Value = Default;
    }

    public event Action<T> OnValueChanged;
  }

  public class BaseArrayParameter<T> : ParameterAttribute<T[]> {
    public override Access Access { get; set; } = Access.List;
  }

}
