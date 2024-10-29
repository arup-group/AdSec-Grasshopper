using System;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGHTests.Helpers {
  // [Collection("GrasshopperFixture collection")]
  // public class DataConvertorTest {
  //   private readonly DummyComponent _component;
  //
  //   public DataConvertorTest() {
  //     _component = new DummyComponent();
  //   }
  //
  //   [Fact]
  //   public void ShouldHaveTheSameNumberOfInputs() {
  //     Assert.Single(_component.Params.Input);
  //   }
  //
  //   [Fact]
  //   public void ShouldPassTheName() {
  //     Assert.Equal("Alpha", _component.Params.Input[0].Name);
  //   }
  //
  //   [Fact]
  //   public void ShouldPassTheNickname() {
  //     Assert.Equal("A", _component.Params.Input[0].NickName);
  //   }
  // }

  public class DummyBusiness : IBusinessComponent {

    public DoubleParameter Alpha { get; set; } = new DoubleParameter {
      Name = "Alpha",
      NickName = "A",
      Description = "Alpha description",
    };
    public DoubleParameter Beta { get; set; }

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

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() { Beta.Value = Alpha.Value * 2; }
  }

  public class DummyComponent : BusinessComponentGlue<DummyBusiness> {
    public DummyComponent() : base("name", "nickname", "description", "category", "subcategory") { }
  }
}
