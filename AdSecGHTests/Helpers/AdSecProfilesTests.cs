using System;
using System.Collections.Generic;

using AdSecGH.Helpers;

using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecProfilesTests {
    private class WebUnknownDummy : IWeb { }

    private class ProfileUnknownDummy : IProfile {
      public ProfileType ProfileType => (ProfileType)(-1);
      public Angle Rotation => new Angle(-1, AngleUnit.Gradian);
    }

    private static IAngleProfile angleProfile = null;

    public AdSecProfilesTests() {
      angleProfile = SampleProfiles.GetAnAngleProfile(new WebConstant(new Length(1, LengthUnit.Inch)));
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

    private IProfile[] profiles = {
      SampleProfiles.GetAnAngleProfile(new WebConstant(new Length(1, LengthUnit.Inch))),
      SampleProfiles.GetACatalogueProfile(),
      SampleProfiles.GetAChannelProfile(),
      SampleProfiles.GetACircleProfile(),
      SampleProfiles.GetACircleHollowProfile(),
      SampleProfiles.GetACruciformSymmetricalProfile(),
      SampleProfiles.GetAnEllipseProfile(),
      SampleProfiles.GetAnEllipseHollowProfile(),
      SampleProfiles.GetAGeneralCProfile(),
      SampleProfiles.GetAGeneralZProfile(),
      SampleProfiles.GetAnIBeamAssymetricalProfile(),
      SampleProfiles.GetAnIBeamCellularProfile(),
      SampleProfiles.GetAPerimeterProfile(),
      SampleProfiles.GetARectangleProfile(),
      SampleProfiles.GetARectangleHollowProfile(),
      SampleProfiles.GetARectoEllipseProfile(),
      SampleProfiles.GetASecantPileProfile(),
      SampleProfiles.GetASheetPileProfile(),
      SampleProfiles.GetATrapezoidProfile(),
      SampleProfiles.GetATSectionProfile(),
    };

    [Fact]
    public void CheckProfiles_NotNull() {
      foreach (var profile in profiles) {
        var result = AdSecProfiles.CreateProfile(profile);
        Assert.NotNull(result);
      }
    }

    [Fact]
    public void CheckProfiles_HaveTheRightType() {
      foreach (var profile in profiles) {
        var result = AdSecProfiles.CreateProfile(profile);
        Assert.Equal($"I{profile.GetType().Name}_Implementation", result.GetType().Name);
      }
    }

    [Fact]
    public void CreateProfile_WhenBeamProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetABeamProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamSymmetricalProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectoCircleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetARectoCircleProfile());

      Assert.NotNull(result);
      Assert.Equal("IStadiumProfile_Implementation", result.GetType().Name);
    }

  }
}
