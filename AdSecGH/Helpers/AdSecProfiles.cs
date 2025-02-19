using System;
using System.Linq;

using Oasys.Profiles;
using Oasys.Taxonomy.Profiles;

using IAngleProfile = Oasys.Profiles.IAngleProfile;
using ICatalogueProfile = Oasys.Profiles.ICatalogueProfile;
using IChannelProfile = Oasys.Profiles.IChannelProfile;
using ICircleHollowProfile = Oasys.Profiles.ICircleHollowProfile;
using ICircleProfile = Oasys.Profiles.ICircleProfile;
using ICruciformSymmetricalProfile = Oasys.Profiles.ICruciformSymmetricalProfile;
using IEllipseHollowProfile = Oasys.Profiles.IEllipseHollowProfile;
using IEllipseProfile = Oasys.Profiles.IEllipseProfile;
using IFlange = Oasys.Profiles.IFlange;
using IGeneralCProfile = Oasys.Profiles.IGeneralCProfile;
using IGeneralZProfile = Oasys.Profiles.IGeneralZProfile;
using IIBeamAsymmetricalProfile = Oasys.Profiles.IIBeamAsymmetricalProfile;
using IIBeamCellularProfile = Oasys.Profiles.IIBeamCellularProfile;
using IIBeamProfile = Oasys.Taxonomy.Profiles.IIBeamProfile;
using IPerimeterProfile = Oasys.Profiles.IPerimeterProfile;
using IProfile = Oasys.Profiles.IProfile;
using IRectangleHollowProfile = Oasys.Profiles.IRectangleHollowProfile;
using IRectangleProfile = Oasys.Profiles.IRectangleProfile;
using IRectoEllipseProfile = Oasys.Profiles.IRectoEllipseProfile;
using ISecantPileProfile = Oasys.Profiles.ISecantPileProfile;
using ISheetPileProfile = Oasys.Profiles.ISheetPileProfile;
using ITrapezoidProfile = Oasys.Profiles.ITrapezoidProfile;
using ITSectionProfile = Oasys.Profiles.ITSectionProfile;
using IWeb = Oasys.Profiles.IWeb;
using IWebConstant = Oasys.Profiles.IWebConstant;
using IWebTapered = Oasys.Profiles.IWebTapered;

namespace AdSecGH.Helpers {
  public static class AdSecProfiles {
    public static IProfile CreateProfile(Oasys.Taxonomy.Profiles.IProfile profile) {
      if (profile == null) {
        return null;
      }

      switch (profile.ProfileType) {
        case ProfileType.Angle: return CreateAngleProfile((Oasys.Taxonomy.Profiles.IAngleProfile)profile);
        case ProfileType.Catalogue: return CreateCatalogueProfile((Oasys.Taxonomy.Profiles.ICatalogueProfile)profile);
        case ProfileType.Channel: return CreateChannelProfile((Oasys.Taxonomy.Profiles.IChannelProfile)profile);
        case ProfileType.Circle: return CreateCircleProfile((Oasys.Taxonomy.Profiles.ICircleProfile)profile);
        case ProfileType.CircleHollow:
          return CreateCircleHollowProfile((Oasys.Taxonomy.Profiles.ICircleHollowProfile)profile);
        case ProfileType.CruciformSymmetrical:
          return CreateCruciformSymmetricalProfile((Oasys.Taxonomy.Profiles.ICruciformSymmetricalProfile)profile);
        case ProfileType.Ellipse: return CreateEllipseProfile((Oasys.Taxonomy.Profiles.IEllipseProfile)profile);
        case ProfileType.EllipseHollow:
          return CreateEllipseHollowProfile((Oasys.Taxonomy.Profiles.IEllipseHollowProfile)profile);
        case ProfileType.GeneralC: return CreateGeneralCProfile((Oasys.Taxonomy.Profiles.IGeneralCProfile)profile);
        case ProfileType.GeneralZ: return CreateGeneralZProfile((Oasys.Taxonomy.Profiles.IGeneralZProfile)profile);
        case ProfileType.IBeam: return CreateIBeamSymmetricalProfile((IBeamProfile)profile);
        case ProfileType.IBeamAssymetrical: return CreateIBeamAsymmetricalProfile((IBeamAsymmetricalProfile)profile);
        case ProfileType.IBeamCellular: return CreateIBeamCellularProfile((IBeamCellularProfile)profile);
        case ProfileType.Perimeter: return CreatePerimeterProfile((Oasys.Taxonomy.Profiles.IPerimeterProfile)profile);
        case ProfileType.Rectangle: return CreateRectangleProfile((Oasys.Taxonomy.Profiles.IRectangleProfile)profile);
        case ProfileType.RectangleHollow:
          return CreateRectangleHollowProfile((Oasys.Taxonomy.Profiles.IRectangleHollowProfile)profile);
        case ProfileType.RectoCircle: return CreateStadiumProfile((IRectoCircleProfile)profile);
        case ProfileType.RectoEllipse:
          return CreateRectoEllipseProfile((Oasys.Taxonomy.Profiles.IRectoEllipseProfile)profile);
        case ProfileType.SecantPile:
          return CreateSecantPileProfile((Oasys.Taxonomy.Profiles.ISecantPileProfile)profile);
        case ProfileType.SheetPile: return CreateSheetPileProfile((Oasys.Taxonomy.Profiles.ISheetPileProfile)profile);
        case ProfileType.Trapezoid: return CreateTrapezoidProfile((Oasys.Taxonomy.Profiles.ITrapezoidProfile)profile);
        case ProfileType.TSection: return CreateTSectionProfile((Oasys.Taxonomy.Profiles.ITSectionProfile)profile);
        case ProfileType.Undefined:
        default:
          throw new NotImplementedException();
      }
    }

    public static IAngleProfile CreateAngleProfile(Oasys.Taxonomy.Profiles.IAngleProfile profile) {
      return IAngleProfile.Create(profile.Depth, CreateFlange(profile.Flange), CreateWeb(profile.Web));
    }

    public static ICatalogueProfile CreateCatalogueProfile(Oasys.Taxonomy.Profiles.ICatalogueProfile profile) {
      return ICatalogueProfile.Create(profile.Description);
    }

    public static IChannelProfile CreateChannelProfile(Oasys.Taxonomy.Profiles.IChannelProfile profile) {
      return IChannelProfile.Create(profile.Depth, CreateFlange(profile.Flanges), CreateWebConstant(profile.Web));
    }

    public static ICircleHollowProfile CreateCircleHollowProfile(Oasys.Taxonomy.Profiles.ICircleHollowProfile profile) {
      return ICircleHollowProfile.Create(profile.Diameter, profile.WallThickness);
    }

    public static ICircleProfile CreateCircleProfile(Oasys.Taxonomy.Profiles.ICircleProfile profile) {
      return ICircleProfile.Create(profile.Diameter);
    }

    public static ICruciformSymmetricalProfile CreateCruciformSymmetricalProfile(
      Oasys.Taxonomy.Profiles.ICruciformSymmetricalProfile profile) {
      return ICruciformSymmetricalProfile.Create(profile.Depth, CreateFlange(profile.Flange),
        CreateWebConstant(profile.Web));
    }

    public static IEllipseHollowProfile CreateEllipseHollowProfile(
      Oasys.Taxonomy.Profiles.IEllipseHollowProfile profile) {
      return IEllipseHollowProfile.Create(profile.Depth, profile.Width, profile.WallThickness);
    }

    public static IEllipseProfile CreateEllipseProfile(Oasys.Taxonomy.Profiles.IEllipseProfile profile) {
      return IEllipseProfile.Create(profile.Depth, profile.Width);
    }

    public static IGeneralCProfile CreateGeneralCProfile(Oasys.Taxonomy.Profiles.IGeneralCProfile profile) {
      return IGeneralCProfile.Create(profile.Depth, profile.FlangeWidth, profile.Lip, profile.Thickness);
    }

    public static IGeneralZProfile CreateGeneralZProfile(Oasys.Taxonomy.Profiles.IGeneralZProfile profile) {
      return IGeneralZProfile.Create(profile.Depth, profile.TopFlangeWidth, profile.BottomFlangeWidth, profile.TopLip,
        profile.BottomLip, profile.Thickness);
    }

    public static IIBeamAsymmetricalProfile CreateIBeamAsymmetricalProfile(
      Oasys.Taxonomy.Profiles.IIBeamAsymmetricalProfile profile) {
      return IIBeamAsymmetricalProfile.Create(profile.Depth, CreateFlange(profile.TopFlange),
        CreateFlange(profile.BottomFlange), CreateWeb(profile.Web));
    }

    public static IIBeamCellularProfile CreateIBeamCellularProfile(
      Oasys.Taxonomy.Profiles.IIBeamCellularProfile profile) {
      return IIBeamCellularProfile.Create(profile.Depth, CreateFlange(profile.Flanges), CreateWebConstant(profile.Web),
        profile.WebOpening);
    }

    public static IIBeamSymmetricalProfile CreateIBeamSymmetricalProfile(IIBeamProfile profile) {
      return IIBeamSymmetricalProfile.Create(profile.Depth, CreateFlange(profile.Flanges),
        CreateWebConstant(profile.Web));
    }

    public static IPerimeterProfile CreatePerimeterProfile(Oasys.Taxonomy.Profiles.IPerimeterProfile profile) {
      var perimeter = IPerimeterProfile.Create();
      perimeter.SolidPolygon = CreatePolygon(profile.Perimeter);

      foreach (var polygon in profile.VoidPolygons) {
        perimeter.VoidPolygons.Add(CreatePolygon(polygon));
      }

      return perimeter;
    }

    public static IRectangleHollowProfile CreateRectangleHollowProfile(
      Oasys.Taxonomy.Profiles.IRectangleHollowProfile profile) {
      return IRectangleHollowProfile.Create(profile.Depth, CreateFlange(profile.Flanges),
        CreateWebConstant(profile.Webs));
    }

    public static IRectangleProfile CreateRectangleProfile(Oasys.Taxonomy.Profiles.IRectangleProfile profile) {
      return IRectangleProfile.Create(profile.Depth, profile.Width);
    }

    public static IStadiumProfile CreateStadiumProfile(IRectoCircleProfile profile) {
      return IStadiumProfile.Create(profile.Depth, profile.Width);
    }

    public static IRectoEllipseProfile CreateRectoEllipseProfile(Oasys.Taxonomy.Profiles.IRectoEllipseProfile profile) {
      return IRectoEllipseProfile.Create(profile.Depth, profile.DepthFlat, profile.Width, profile.WidthFlat);
    }

    public static ISecantPileProfile CreateSecantPileProfile(Oasys.Taxonomy.Profiles.ISecantPileProfile profile) {
      return ISecantPileProfile.Create(profile.Diameter, profile.PileCentres, profile.PileCount, profile.IsWall);
    }

    public static ISheetPileProfile CreateSheetPileProfile(Oasys.Taxonomy.Profiles.ISheetPileProfile profile) {
      return ISheetPileProfile.Create(profile.Depth, profile.Width, profile.TopFlangeWidth, profile.BottomFlangeWidth,
        profile.FlangeThickness, profile.WebThickness);
    }

    public static ITrapezoidProfile CreateTrapezoidProfile(Oasys.Taxonomy.Profiles.ITrapezoidProfile profile) {
      return ITrapezoidProfile.Create(profile.Depth, profile.TopWidth, profile.BottomWidth);
    }

    public static ITSectionProfile CreateTSectionProfile(Oasys.Taxonomy.Profiles.ITSectionProfile profile) {
      return ITSectionProfile.Create(profile.Depth, CreateFlange(profile.Flange), CreateWeb(profile.Web));
    }

    //helpers

    public static IPolygon CreatePolygon(Oasys.Taxonomy.Geometry.IPolygon p) {
      var polygon = IPolygon.Create();
      if (p == null || p.Points == null || !p.Points.Any()) {
        return polygon;
      }

      foreach (var point in p.Points) {
        polygon.Points.Add(IPoint.Create(point.Y, point.Z));
      }

      return polygon;
    }

    public static IFlange CreateFlange(Oasys.Taxonomy.Profiles.IFlange flange) {
      return flange == null ? null : IFlange.Create(flange.Width, flange.Thickness);
    }

    public static IWebConstant CreateWebConstant(Oasys.Taxonomy.Profiles.IWebConstant web) {
      return IWebConstant.Create(web.Thickness);
    }

    public static IWebTapered CreateWebTapered(Oasys.Taxonomy.Profiles.IWebTapered web) {
      return IWebTapered.Create(web.TopThickness, web.BottomThickness);
    }

    public static IWeb CreateWeb(Oasys.Taxonomy.Profiles.IWeb web) {
      IWeb result = null;
      switch (web) {
        case Oasys.Taxonomy.Profiles.IWebConstant webConstant:
          result = CreateWebConstant(webConstant);
          break;
        case Oasys.Taxonomy.Profiles.IWebTapered webTapered:
          result = CreateWebTapered(webTapered);
          break;
      }

      return result;
    }
  }
}
