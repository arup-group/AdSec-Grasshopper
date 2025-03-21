using AdSecCore.Functions;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecCoreTests {
  public class BusinessComponentTests {

    private bool spyCalled;

    [Fact]
    public void ShouldNotifyOnValueChanged() {
      var component = new FakeFunc();
      component.Diameter.OnValueChanged += newValue => spyCalled = true;
      component.Diameter.Value = 1;
      Assert.True(spyCalled);
    }

    public class FakeFunc : IFunction {

      public DoubleParameter Diameter { get; set; } = new DoubleParameter();

      public FuncAttribute Metadata { get; set; } = new FuncAttribute();
      public Organisation Organisation { get; set; } = new Organisation();

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
