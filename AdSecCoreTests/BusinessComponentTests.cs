﻿using AdSecCore.Functions;

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

      public DoubleParameter Diameter { get; set; } = new();

      public FuncAttribute Metadata { get; set; } = new();
      public Organisation Organisation { get; set; } = new();

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
