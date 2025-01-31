using System.Collections.Generic;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_IPointsTests {
    private Oasys.Collections.IList<IPoint> _points;
    private List<int> invalidIds;
    private int pointsConverted = 0;
    private readonly IPoint _sample = IPoint.Create(new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));

    public AdSecInputTests_IPointsTests() {
      _points = Oasys.Collections.IList<IPoint>.Create();
      invalidIds = new List<int>();
      pointsConverted = 0;
    }

    [Fact]
    public void TryCastToIPointsReturnsFalseWhenInvalidIdsListIsNullInitialised() {
      bool castSuccessful = AdSecInput.TryCastToIPoints(null, _points, null, ref pointsConverted);

      Assert.False(castSuccessful);
      Assert.Empty(_points);
      Assert.Empty(invalidIds);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsFalseWhenNull() {
      bool castSuccessful = AdSecInput.TryCastToIPoints(null, _points, invalidIds, ref pointsConverted);

      Assert.False(castSuccessful);
      Assert.Empty(_points);
      Assert.Empty(invalidIds);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsFalseWhenEmptySections() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToIPoints(objectWrappers, _points, invalidIds, ref pointsConverted);

      Assert.False(castSuccessful);
      Assert.Empty(_points);
      Assert.Empty(invalidIds);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsFalseWhenValueIsNull() {
      IPoint point = null;
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(point),
      };
      bool castSuccessful = AdSecInput.TryCastToIPoints(objectWrappers, _points, invalidIds, ref pointsConverted);

      Assert.False(castSuccessful);
      Assert.Empty(_points);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsEmptyWhenNullItem() {
      var objectWrapper = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToIPoints(objectWrappers, _points, invalidIds, ref pointsConverted);

      Assert.False(castSuccessful);
      Assert.Empty(_points);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsFalseWhenSecondItemIncorrect() {
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(_sample),
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToIPoints(objectWrappers, _points, invalidIds, ref pointsConverted);

      Assert.False(castSuccessful);
      Assert.Single(_points);
      Assert.Single(invalidIds);
      Assert.Equal(1, invalidIds[0]);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsCorrectDataFromIPoint() {
      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(_sample),
      };
      bool castSuccessful = AdSecInput.TryCastToIPoints(objwrap, _points, invalidIds, ref pointsConverted);

      Assert.True(castSuccessful);
      Assert.NotNull(_points);
      Assert.Empty(invalidIds);
      Assert.Equal(0, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsCorrectDataFromPoint3ds() {
      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(new Point3d(1, 1, 1)),
        new GH_ObjectWrapper(new Point3d(2, 2, 2)),
      };
      bool castSuccessful = AdSecInput.TryCastToIPoints(objwrap, _points, invalidIds, ref pointsConverted);

      Assert.True(castSuccessful);
      Assert.NotNull(_points);
      Assert.Empty(invalidIds);
      Assert.Equal(2, pointsConverted);
    }

    [Fact]
    public void TryCastToIPointsReturnsCorrectDataFromCurve() {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objectWrapper = new GH_ObjectWrapper(tuple.Item1);
      var objwrap = new List<GH_ObjectWrapper>() {
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToIPoints(objwrap, _points, invalidIds, ref pointsConverted);

      Assert.True(castSuccessful);
      Assert.NotNull(_points);
      Assert.Empty(invalidIds);
      Assert.Equal(0, pointsConverted);
    }

    #region helpers

    [Fact]
    public void TryCastToIPointReturnsFalseWhenCantConvert() {
      IPoint point = null;
      bool castSuccessful = AdSecInput.TryCastToIPoint(new GH_ObjectWrapper(null), ref point);

      Assert.False(castSuccessful);
      Assert.Null(point);
    }

    [Fact]
    public void TryCastToIPointReturnsIPointWhenInputIsIPoint() {
      IPoint point = null;
      bool castSuccessful = AdSecInput.TryCastToIPoint(new GH_ObjectWrapper(_sample), ref point);

      Assert.True(castSuccessful);
      Assert.Equal(_sample, point);
    }

    [Fact]
    public void TryCastToIPointReturnsIPointWhenInputIsAdSecPointGoo() {
      var input = new AdSecPointGoo(_sample);

      IPoint point = null;
      bool castSuccessful = AdSecInput.TryCastToIPoint(new GH_ObjectWrapper(input), ref point);

      Assert.True(castSuccessful);
      Assert.Equal(input.AdSecPoint, point);
    }

    [Fact]
    public void TryCastToCurveReturnsFalseWhenCantConvert() {
      Curve curve = null;
      bool castSuccessful = AdSecInput.TryCastToCurve(new GH_ObjectWrapper(null), ref curve);

      Assert.False(castSuccessful);
      Assert.Null(curve);
    }

    [Fact]
    public void TryCastToCurveReturnsTrueWhenDataIsCorrect() {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objectWrapper = new GH_ObjectWrapper(tuple.Item1);
      Curve curve = null;
      bool castSuccessful = AdSecInput.TryCastToCurve(objectWrapper, ref curve);

      Assert.True(castSuccessful);
      Assert.NotNull(curve);
    }

    [Fact]
    public void TryCastToPoint3dReturnsFalseWhenCantConvert() {
      var point3d = Point3d.Unset;
      bool castSuccessful = AdSecInput.TryCastToPoint3d(new GH_ObjectWrapper(null), ref point3d);

      Assert.False(castSuccessful);
      Assert.False(point3d.IsValid);
    }

    [Fact]
    public void TryCastToPoint3dReturnsTrueWhenDataIsCorrect() {
      var point3d = Point3d.Unset;
      bool castSuccessful = AdSecInput.TryCastToPoint3d(new GH_ObjectWrapper(new Point3d()), ref point3d);

      Assert.True(castSuccessful);
      Assert.True(point3d.IsValid);
    }

    [Fact]
    public void ProcessTemporaryPointsReturnsEmptyPointsForNullLists() {
      IList<IPoint> iPoints = null;
      List<Point3d> temporaryPoints = null;

      AdSecInput.ProcessTemporaryPoints(ref iPoints, ref temporaryPoints);

      Assert.Empty(iPoints);
      Assert.Empty(temporaryPoints);
    }

    [Fact]
    public void ProcessTemporaryPointsReturnsEmptyPoints() {
      IList<IPoint> iPoints = new List<IPoint>();
      var temporaryPoints = new List<Point3d>();

      AdSecInput.ProcessTemporaryPoints(ref iPoints, ref temporaryPoints);

      Assert.Empty(iPoints);
      Assert.Empty(temporaryPoints);
    }

    [Fact]
    public void ProcessTemporaryPointsReturnsPointForSingleTemporaryPoint() {
      IList<IPoint> iPoints = new List<IPoint>();
      var temporaryPoints = new List<Point3d>() {
        new Point3d(),
      };

      AdSecInput.ProcessTemporaryPoints(ref iPoints, ref temporaryPoints);

      Assert.Single(iPoints);
      Assert.Single(temporaryPoints);
      Assert.Equal(0, iPoints[0].Y.Value);
      Assert.Equal(0, iPoints[0].Z.Value);
    }

    [Fact]
    public void ProcessTemporaryPointsReturnsPoints() {
      IList<IPoint> iPoints = new List<IPoint>();
      var temporaryPoints = new List<Point3d>() {
        new Point3d(new Vector3d(1, 1, 1)),
        new Point3d(new Vector3d(2, 2, 2)),
      };

      AdSecInput.ProcessTemporaryPoints(ref iPoints, ref temporaryPoints);

      Assert.Equal(2, iPoints.Count);
      Assert.Equal(2, temporaryPoints.Count);
      Assert.Equal(0.001, iPoints[0].Y.Value);
      Assert.Equal(0.001, iPoints[0].Z.Value);
    }

    #endregion

  }
}
