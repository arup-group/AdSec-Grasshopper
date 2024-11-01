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
    public T Value { get; set; }
    public T Default { get; set; }
    public virtual Access Access { get; set; } = Access.Item;

    public void SetDefault() {
      Value = Default;
    }
  }

  public class BaseArrayParameter<T> : ParameterAttribute<T[]> {
    public override Access Access { get; set; } = Access.List;
  }

}
