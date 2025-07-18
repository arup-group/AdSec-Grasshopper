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

    public class FakeFunc : Function {

      public DoubleParameter Diameter { get; set; } = new DoubleParameter();

      public override FuncAttribute Metadata { get; set; } = new FuncAttribute();
      public override Organisation Organisation { get; set; } = new Organisation();

      public override Attribute[] GetAllInputAttributes() {
        return new Attribute[] {
          Diameter,
        };
      }

      public override Attribute[] GetAllOutputAttributes() { throw new NotImplementedException(); }

      public override void Compute() { }
    }
  }
}
