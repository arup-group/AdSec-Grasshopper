using AdSecCore.Parameters;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace AdSecCoreTests {
  public class BusinessComponentTests {

    private bool spyCalled;

    [Fact]
    public void ShouldNotifyOnValueChanged() {
      var component = new FakeComponent();
      component.Diameter.OnValueChanged += newValue => spyCalled = true;
      component.Diameter.Value = 1;
      Assert.True(spyCalled);
    }

    public class FakeComponent : IBusinessComponent {

      public DoubleParameter Diameter { get; set; } = new();

      public ComponentAttribute Metadata { get; set; }
      public ComponentOrganisation Organisation { get; set; }

      public Attribute[] GetAllInputAttributes() {
        return new Attribute[] {
          Diameter,
        };
      }

      public Attribute[] GetAllOutputAttributes() { throw new NotImplementedException(); }

      public void Compute() { }
    }
  }
}
