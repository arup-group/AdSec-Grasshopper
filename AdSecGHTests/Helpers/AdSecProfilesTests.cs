using System;
using System.Collections.Generic;

using AdSecGH.Helpers;

using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

using ICatalogueProfile = Oasys.Profiles.ICatalogueProfile;
using IChannelProfile = Oasys.Profiles.IChannelProfile;
using ICircleProfile = Oasys.Profiles.ICircleProfile;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecProfilesTests {

    #region init profiles

    private class WebUnknownDummy : IWeb { }

    private class ProfileUnknownDummy : IProfile {
      public ProfileType ProfileType => (ProfileType)(-1);
      public Angle Rotation => new Angle(-1, AngleUnit.Gradian);
    }

    private IAngleProfile angleProfile = null;

    private static AngleProfile SetAngleProfile(IWeb web) {
      var lengthInMeters = new Length(1, LengthUnit.Meter);
      var thicknessInMeters = new Length(0.2, LengthUnit.Meter);
      var depthInInches = new Length(1, LengthUnit.Inch);
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      var flange = new Flange(thicknessInMeters, lengthInMeters);

      return new AngleProfile {
        Depth = depthInInches,
        Rotation = rotationInDegrees,
        Flange = flange,
        Web = web,
      };
    }

    private static CatalogueProfile SetCatalogueProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new CatalogueProfile() {
        Rotation = rotationInDegrees,
        Description = "CAT BSI-IPE IPEAA80",
      };
    }

    private static ChannelProfile SetChannelProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new ChannelProfile() {
        Rotation = rotationInDegrees,
        Web = new WebConstant(new Length(1, LengthUnit.Inch)),
        Flanges = new Flange(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
        Depth = new Length(3, LengthUnit.Inch),
      };
    }

    private static CircleProfile SetCircleProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new CircleProfile() {
        Rotation = rotationInDegrees,
        Diameter = new Length(1, LengthUnit.Inch),
      };
    }

    private static CircleHollowProfile SetCircleHollowProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new CircleHollowProfile() {
        WallThickness = new Length(1, LengthUnit.Inch),
        Rotation = rotationInDegrees,
        Diameter = new Length(1, LengthUnit.Inch),
      };
    }

    private static CruciformSymmetricalProfile SetCruciformSymmetricalProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new CruciformSymmetricalProfile() {
        Rotation = rotationInDegrees,
        Web = new WebConstant(new Length(1, LengthUnit.Inch)),
        Depth = new Length(1, LengthUnit.Inch),
        Flange = new Flange(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
      };
    }

    private static EllipseProfile SetEllipseProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new EllipseProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Width = new Length(2, LengthUnit.Inch),
      };
    }

    private static EllipseHollowProfile SetEllipseHollowProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new EllipseHollowProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Width = new Length(2, LengthUnit.Inch),
        WallThickness = new Length(0.2, LengthUnit.Inch),
      };
    }

    private static GeneralCProfile SetGeneralCProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new GeneralCProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Thickness = new Length(0.2, LengthUnit.Inch),
        FlangeWidth = new Length(1, LengthUnit.Inch),
        Lip = new Length(0.2, LengthUnit.Inch),
      };
    }

    private static GeneralZProfile SetGeneralZProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new GeneralZProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Thickness = new Length(0.2, LengthUnit.Inch),
        BottomFlangeWidth = new Length(1, LengthUnit.Inch),
        BottomLip = new Length(0.2, LengthUnit.Inch),
        TopFlangeWidth = new Length(1, LengthUnit.Inch),
        TopLip = new Length(0.2, LengthUnit.Inch),
      };
    }

    private static IBeamProfile SetBeamProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new IBeamProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Web = new WebConstant(new Length(1, LengthUnit.Inch)),
        Flanges = new Flange(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
      };
    }

    private static IBeamAsymmetricalProfile SetIBeamAssymetricalProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new IBeamAsymmetricalProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Web = new WebConstant(new Length(1, LengthUnit.Inch)),
        BottomFlange = new Flange(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
        TopFlange = new Flange(new Length(2, LengthUnit.Inch), new Length(1, LengthUnit.Inch)),
      };
    }

    private static IBeamCellularProfile SetBeamCellularProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new IBeamCellularProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Web = new WebConstant(new Length(1, LengthUnit.Inch)),
        Flanges = new Flange(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
        Spacing = new Length(0.2, LengthUnit.Inch),
        WebOpening = new Length(0.2, LengthUnit.Inch),
        OpeningType = IBeamOpeningType.Castellated,
      };
    }

    private static PerimeterProfile SetPerimeterProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new PerimeterProfile() {
        Rotation = rotationInDegrees,
        Perimeter = new Polygon(new List<IPoint2d>() {
          new Point2d(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
        }),
        VoidPolygons = new List<IPolygon>() {
          new Polygon(new List<IPoint2d>() {
            new Point2d(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
          }),
        },
      };
    }

    private static RectangleProfile SetRectangleProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new RectangleProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Width = new Length(2, LengthUnit.Inch),
      };
    }

    private static RectangleHollowProfile SetRectangleHollowProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new RectangleHollowProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Flanges = new Flange(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
        Webs = new WebConstant(new Length(1, LengthUnit.Inch)),
      };
    }

    private static RectoCircleProfile SetRectoCircleProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new RectoCircleProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Width = new Length(2, LengthUnit.Inch),
      };
    }

    private static RectoEllipseProfile SetRectoEllipseProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new RectoEllipseProfile() {
        Rotation = rotationInDegrees,
        Depth = new Length(1, LengthUnit.Inch),
        Width = new Length(2, LengthUnit.Inch),
        DepthFlat = new Length(0.2, LengthUnit.Inch),
        WidthFlat = new Length(0.2, LengthUnit.Inch),
      };
    }

    private static SecantPileProfile SetSecantPileProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new SecantPileProfile() {
        Rotation = rotationInDegrees,
        Diameter = new Length(1, LengthUnit.Inch),
        IsWall = true,
        PileCentres = new Length(0.2, LengthUnit.Inch),
        PileCount = 2,
      };
    }

    private static SheetPileProfile SetSheetPileProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new SheetPileProfile() {
        Rotation = rotationInDegrees,
        BottomFlangeWidth = new Length(1, LengthUnit.Inch),
        FlangeThickness = new Length(0.2, LengthUnit.Inch),
        Depth = new Length(1, LengthUnit.Inch),
        TopFlangeWidth = new Length(1, LengthUnit.Inch),
        WebThickness = new Length(0.2, LengthUnit.Inch),
        Width = new Length(2, LengthUnit.Inch),
      };
    }

    private static TrapezoidProfile SetTrapezoidProfile() {
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      return new TrapezoidProfile() {
        Rotation = rotationInDegrees,
        BottomWidth = new Length(1, LengthUnit.Inch),
        Depth = new Length(1, LengthUnit.Inch),
        TopWidth = new Length(2, LengthUnit.Inch),
      };
    }

    private static TSectionProfile SetTSectionProfile() {
      var lengthInMeters = new Length(1, LengthUnit.Meter);
      var thicknessInMeters = new Length(0.2, LengthUnit.Meter);
      var depthInInches = new Length(1, LengthUnit.Inch);
      var rotationInDegrees = new Angle(0, AngleUnit.Degree);

      var flange = new Flange(thicknessInMeters, lengthInMeters);

      return new TSectionProfile {
        Depth = depthInInches,
        Rotation = rotationInDegrees,
        Flange = flange,
        Web = new WebConstant(new Length(1, LengthUnit.Inch)),
      };
    }

    #endregion

    public AdSecProfilesTests() {
      angleProfile = SetAngleProfile(new WebConstant(new Length(1, LengthUnit.Inch)));
    }

    [Fact]
    public void CreateWeb_WithUnknownIWeb_ReturnsNull() {
      var unknownWeb = new WebUnknownDummy();
      var result = AdSecProfiles.CreateWeb(unknownWeb);

      Assert.Null(result);
    }

    [Fact]
    public void CreateWeb_WithNullIWeb_ReturnsNull() {
      var result = AdSecProfiles.CreateWeb(null);

      Assert.Null(result);
    }

    [Fact]
    public void CreateWeb_WithIWebConstant_ReturnsIWebConstant() {
      var webConstant = (WebConstant)angleProfile.Web;

      var result = AdSecProfiles.CreateWeb(webConstant);

      Assert.Equal(webConstant.Thickness.As(LengthUnit.Meter), result.TopThickness.Value);
    }

    [Fact]
    public void CreateWeb_WithIWebTapered_ReturnsIWebTapered() {
      var webTapered = new WebTapered(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch));
      var result = AdSecProfiles.CreateWeb(webTapered);

      Assert.Equal(webTapered.TopThickness.As(LengthUnit.Meter), result.TopThickness.Value);
      Assert.Equal(webTapered.BottomThickness.As(LengthUnit.Meter), result.BottomThickness.Value);
    }

    [Fact]
    public void CreateFlange_WithNullIWeb_ReturnsNull() {
      var result = AdSecProfiles.CreateFlange(null);

      Assert.Null(result);
    }

    [Fact]
    public void CreateFlange_ReturnsValidFlange() {
      var result = AdSecProfiles.CreateFlange(angleProfile.Flange);

      Assert.Equal(angleProfile.Flange.Thickness.As(LengthUnit.Meter), result.Thickness.Value);
      Assert.Equal(angleProfile.Flange.Width.As(LengthUnit.Meter), result.Width.Value);
    }

    [Fact]
    public void CreatePolygon_WithNullPolygon_ReturnsNull() {
      var result = AdSecProfiles.CreatePolygon(null);

      Assert.Empty(result.Points);
    }

    [Fact]
    public void CreatePolygon_WithNullPolygonPoints_ReturnsNull() {
      var result = AdSecProfiles.CreatePolygon(new Polygon() {
        Points = null,
      });

      Assert.Empty(result.Points);
    }

    [Fact]
    public void CreatePolygon_WithEmptyPolygonPoints_ReturnsNull() {
      var result = AdSecProfiles.CreatePolygon(new Polygon());

      Assert.Empty(result.Points);
    }

    [Fact]
    public void CreatePolygon_WithPolygonPoints_ReturnsValidPolygon() {
      var result = AdSecProfiles.CreatePolygon(new Polygon(new List<IPoint2d>() {
        new Point2d(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Inch)),
      }));

      Assert.Single(result.Points);
      Assert.Equal(0.0254, result.Points[0].Y.As(LengthUnit.Meter));
      Assert.Equal(0.0508, result.Points[0].Z.As(LengthUnit.Meter));
    }

    [Fact]
    public void CreateProfile_WhenNullProfile_ReturnsNull() {
      var result = AdSecProfiles.CreateProfile(null);
      Assert.Null(result);
    }

    [Fact]
    public void CreateProfile_WhenUnknownProfileType_ThrowsError() {
      Assert.Throws<NotImplementedException>(() => AdSecProfiles.CreateProfile(new ProfileUnknownDummy()));
    }

    [Fact]
    public void CreateProfile_WhenAngleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(angleProfile);

      Assert.NotNull(result);
      Assert.Equal(2.4, result.SurfaceAreaPerUnitLength().Value);
      Assert.Equal($"{nameof(Oasys.Profiles.IAngleProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCatalogueProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetCatalogueProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICatalogueProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenChannelProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetChannelProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IChannelProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCircleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetCircleProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICircleProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCircleHollowProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetCircleHollowProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICircleHollowProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCruciformSymmetricalProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetCruciformSymmetricalProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICruciformSymmetricalProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenEllipseProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetEllipseProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IEllipseProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenEllipseHollowProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetEllipseHollowProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IEllipseHollowProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenGeneralCProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetGeneralCProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IGeneralCProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenGeneralZProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetGeneralZProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IGeneralZProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenBeamProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetBeamProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamSymmetricalProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenIBeamAssymetricalProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetIBeamAssymetricalProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamAsymmetricalProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenIBeamCellularProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetBeamCellularProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamCellularProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenPerimeterProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetPerimeterProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IPerimeterProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectangleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetRectangleProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IRectangleProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectangleHollowProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetRectangleHollowProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IRectangleHollowProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectoCircleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetRectoCircleProfile());

      Assert.NotNull(result);
      Assert.Equal("IStadiumProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectoEllipseProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetRectoEllipseProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IRectoEllipseProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenSecantPileProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetSecantPileProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ISecantPileProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenSheetPileProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetSheetPileProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ISheetPileProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenTrapezoidProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetTrapezoidProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ITrapezoidProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenTSectionProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SetTSectionProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ITSectionProfile)}_Implementation", result.GetType().Name);
    }

  }
}
