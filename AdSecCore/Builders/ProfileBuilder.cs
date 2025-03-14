using System.Collections.Generic;

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

    public static IIBeamSymmetricalProfile GetIBeam(double width = 10, double depth = 10, double thickness = 1) {
      var thick = Length.FromCentimeters(thickness);
      return IIBeamSymmetricalProfile.Create(Length.FromCentimeters(depth),
        IFlange.Create(Length.FromCentimeters(width), thick), IWebConstant.Create(thick));
    }
  }

  public class PerimeterBuilder : IBuilder<IProfile> {
    private List<IPoint> _points;

    public IProfile Build() {
      var perimeter = IPerimeterProfile.Create();
      var perimeterSolidPolygon = IPolygon.Create();

      foreach (var point in _points) {
        perimeterSolidPolygon.Points.Add(point);
      }

      perimeter.SolidPolygon = perimeterSolidPolygon;

      return perimeter;
    }

    public PerimeterBuilder WithPoint(IPoint point) {
      if (_points == null) {
        _points = new List<IPoint>();
      }

      _points.Add(point);
      return this;
    }
  }
}
