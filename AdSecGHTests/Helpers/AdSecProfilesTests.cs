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
    private class WebUnknownDummy : IWeb { }

    private class ProfileUnknownDummy : IProfile {
      public ProfileType ProfileType => (ProfileType)(-1);
      public Angle Rotation => new Angle(-1, AngleUnit.Gradian);
    }

    private IAngleProfile angleProfile = null;

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

    [Fact]
    public void CreateProfile_WhenAngleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(angleProfile);

      Assert.NotNull(result);
      Assert.Equal($"{nameof(Oasys.Profiles.IAngleProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCatalogueProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetACatalogueProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICatalogueProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenChannelProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAChannelProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IChannelProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCircleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetACircleProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICircleProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCircleHollowProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetACircleHollowProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICircleHollowProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenCruciformSymmetricalProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetACruciformSymmetricalProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ICruciformSymmetricalProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenEllipseProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAnEllipseProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IEllipseProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenEllipseHollowProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAnEllipseHollowProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IEllipseHollowProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenGeneralCProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAGeneralCProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IGeneralCProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenGeneralZProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAGeneralZProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IGeneralZProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenBeamProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetABeamProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamSymmetricalProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenIBeamAssymetricalProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAnIBeamAssymetricalProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamAsymmetricalProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenIBeamCellularProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetABeamCellularProfile());

      Assert.NotNull(result);
      Assert.Equal("IIBeamCellularProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenPerimeterProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetAPerimeterProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IPerimeterProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectangleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetARectangleProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IRectangleProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectangleHollowProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetARectangleHollowProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IRectangleHollowProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectoCircleProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetARectoCircleProfile());

      Assert.NotNull(result);
      Assert.Equal("IStadiumProfile_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenRectoEllipseProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetARectoEllipseProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(IRectoEllipseProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenSecantPileProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetASecantPileProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ISecantPileProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenSheetPileProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetASheetPileProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ISheetPileProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenTrapezoidProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetATrapezoidProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ITrapezoidProfile)}_Implementation", result.GetType().Name);
    }

    [Fact]
    public void CreateProfile_WhenTSectionProfileType_ReturnValidProfile() {
      var result = AdSecProfiles.CreateProfile(SampleProfiles.GetATSectionProfile());

      Assert.NotNull(result);
      Assert.Equal($"{nameof(ITSectionProfile)}_Implementation", result.GetType().Name);
    }

  }
}
