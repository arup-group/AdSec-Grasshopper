using System;

using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Helpers {
  public static class PlaneHelper {
    public static bool IsNotParallelToWorldXYZ(Plane plane) {
      return plane.IsValid && !plane.Equals(Plane.WorldXY) && !plane.Equals(Plane.WorldYZ)
        && !plane.Equals(Plane.WorldZX);
    }
  }

  public static class AxisHelper {
    public static (Line Xaxis, Line Yaxis, Line Zaxis) GetLocalAxisLines(IProfile profile, Plane plane) {
      var area = profile.Area();
      double pythagoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));

      var length = new Length(pythagoras * 0.15, LengthUnit.Meter);
      var Xaxis = new Line(plane.Origin, plane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
      var Yaxis = new Line(plane.Origin, plane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
      var Zaxis = new Line(plane.Origin, plane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));

      return (Xaxis, Yaxis, Zaxis);
    }
  }
}
