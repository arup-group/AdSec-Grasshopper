using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {
  public class ProfileBuilder : IBuilder<IProfile> {
    private double depth;
    private double width;

    public IProfile Build() {
      return IRectangleProfile.Create(Length.FromCentimeters(depth), Length.FromCentimeters(width));
    }

    public ProfileBuilder WidthDepth(double depth) {
      this.depth = depth;
      return this;
    }

    public ProfileBuilder WithWidth(double width) {
      this.width = width;
      return this;
    }
  }
}
