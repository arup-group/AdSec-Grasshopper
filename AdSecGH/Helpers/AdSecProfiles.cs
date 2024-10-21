using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

namespace AdSecGH.Helpers {
  public static class AdSecProfiles {
    public static Oasys.Profiles.IProfile CreateProfile(IProfile profile) {
      switch (profile.ProfileType) {
        case ProfileType.Angle:
          return CreateAngleProfile((IAngleProfile)profile);
        case ProfileType.Catalogue:
          return CreateCatalogueProfile((ICatalogueProfile)profile);
        case ProfileType.Channel:
          return CreateChannelProfile((IChannelProfile)profile);
        case ProfileType.Circle:
          return CreateCircleProfile((ICircleProfile)profile);
        case ProfileType.CircleHollow:
          return CreateCircleHollowProfile((ICircleHollowProfile)profile);
        case ProfileType.CruciformSymmetrical:
          return CreateCruciformSymmetricalProfile((ICruciformSymmetricalProfile)profile);
        case ProfileType.Ellipse:
          return CreateEllipseProfile((IEllipseProfile)profile);
        case ProfileType.EllipseHollow:
          return CreateEllipseHollowProfile((IEllipseHollowProfile)profile);
        case ProfileType.GeneralC:
          return CreateGeneralCProfile((IGeneralCProfile)profile);
        case ProfileType.GeneralZ:
          return CreateGeneralZProfile((IGeneralZProfile)profile);
        case ProfileType.IBeam:
          return CreateIBeamSymmetricalProfile((IBeamProfile)profile);
        case ProfileType.IBeamAssymetrical:
          return CreateIBeamAsymmetricalProfile((IBeamAsymmetricalProfile)profile);
        case ProfileType.IBeamCellular:
          return CreateIBeamCellularProfile((IBeamCellularProfile) profile);
        case ProfileType.Perimeter:
          return CreatePerimeterProfile((IPerimeterProfile)profile);
        case ProfileType.Rectangle:
          return CreateRectangleProfile((IRectangleProfile)profile);
        case ProfileType.RectangleHollow:
          return CreateRectangleHollowProfile((IRectangleHollowProfile)profile);
        case ProfileType.RectoCircle:
          return CreateStadiumProfile((IRectoCircleProfile)profile);
        case ProfileType.RectoEllipse:
          return CreateRectoEllipseProfile((IRectoEllipseProfile)profile);
        case ProfileType.SecantPile:
          return CreateSecantPileProfile((ISecantPileProfile)profile);
        case ProfileType.SheetPile:
          return CreateSheetPileProfile((ISheetPileProfile)profile);
        case ProfileType.Trapezoid:
          return CreateTrapezoidProfile((ITrapezoidProfile)profile);
        case ProfileType.TSection:
          return CreateTSectionProfile((ITSectionProfile)profile);
        case ProfileType.Undefined:
        default:
          throw new System.NotImplementedException();
      }
    }

    public static Oasys.Profiles.IAngleProfile CreateAngleProfile(IAngleProfile profile) {
      return Oasys.Profiles.IAngleProfile.Create(profile.Depth, CreateFlange(profile.Flange), CreateWeb(profile.Web));
    }

    public static Oasys.Profiles.ICatalogueProfile CreateCatalogueProfile(ICatalogueProfile profile) {
      return Oasys.Profiles.ICatalogueProfile.Create(profile.Description);
    }

    public static Oasys.Profiles.IChannelProfile CreateChannelProfile(IChannelProfile profile) {
      return Oasys.Profiles.IChannelProfile.Create(profile.Depth, CreateFlange(profile.Flanges), CreateWebConstant(profile.Web));
    }

    public static Oasys.Profiles.ICircleHollowProfile CreateCircleHollowProfile(ICircleHollowProfile profile) {
      return Oasys.Profiles.ICircleHollowProfile.Create(profile.Diameter, profile.WallThickness);
    }

    public static Oasys.Profiles.ICircleProfile CreateCircleProfile(ICircleProfile profile) {
      return Oasys.Profiles.ICircleProfile.Create(profile.Diameter);
    }

    public static Oasys.Profiles.ICruciformSymmetricalProfile CreateCruciformSymmetricalProfile(ICruciformSymmetricalProfile profile) {
      return Oasys.Profiles.ICruciformSymmetricalProfile.Create(profile.Depth, CreateFlange(profile.Flange), CreateWebConstant(profile.Web));
    }

    public static Oasys.Profiles.IEllipseHollowProfile CreateEllipseHollowProfile(IEllipseHollowProfile profile) {
      return Oasys.Profiles.IEllipseHollowProfile.Create(profile.Depth, profile.Width, profile.WallThickness);
    }

    public static Oasys.Profiles.IEllipseProfile CreateEllipseProfile(IEllipseProfile profile) {
      return Oasys.Profiles.IEllipseProfile.Create(profile.Depth, profile.Width);
    }

    public static Oasys.Profiles.IGeneralCProfile CreateGeneralCProfile(IGeneralCProfile profile) {
      return Oasys.Profiles.IGeneralCProfile.Create(profile.Depth, profile.FlangeWidth, profile.Lip, profile.Thickness);
    }

    public static Oasys.Profiles.IGeneralZProfile CreateGeneralZProfile(IGeneralZProfile profile) {
      return Oasys.Profiles.IGeneralZProfile.Create(profile.Depth, profile.TopFlangeWidth, profile.BottomFlangeWidth, profile.TopLip, profile.BottomLip, profile.Thickness);
    }

    public static Oasys.Profiles.IIBeamAsymmetricalProfile CreateIBeamAsymmetricalProfile(IIBeamAsymmetricalProfile profile) {
      return Oasys.Profiles.IIBeamAsymmetricalProfile.Create(profile.Depth, CreateFlange(profile.TopFlange), CreateFlange(profile.BottomFlange), CreateWeb(profile.Web));
    }

    public static Oasys.Profiles.IIBeamCellularProfile CreateIBeamCellularProfile(IIBeamCellularProfile profile) {
      return Oasys.Profiles.IIBeamCellularProfile.Create(profile.Depth, CreateFlange(profile.Flanges), CreateWebConstant(profile.Web), profile.WebOpening);
    }

    public static Oasys.Profiles.IIBeamSymmetricalProfile CreateIBeamSymmetricalProfile(IIBeamProfile profile) {
      return Oasys.Profiles.IIBeamSymmetricalProfile.Create(profile.Depth, CreateFlange(profile.Flanges), CreateWebConstant(profile.Web));
    }

    public static Oasys.Profiles.IPerimeterProfile CreatePerimeterProfile(IPerimeterProfile profile) {
      var perimeter = Oasys.Profiles.IPerimeterProfile.Create();
      perimeter.SolidPolygon = CreatePolygon(profile.Perimeter);

      foreach (IPolygon polygon in profile.VoidPolygons) {
        perimeter.VoidPolygons.Add(CreatePolygon(polygon));
      }
      return perimeter;
    }

    private static Oasys.Profiles.IPolygon CreatePolygon(IPolygon p) {
      var polygon = Oasys.Profiles.IPolygon.Create();
      foreach (IPoint2d point in p.Points) {
        polygon.Points.Add(Oasys.Profiles.IPoint.Create(point.Y, point.Z));
      }
      return polygon;
    }

    public static Oasys.Profiles.IRectangleHollowProfile CreateRectangleHollowProfile(IRectangleHollowProfile profile) {
      return Oasys.Profiles.IRectangleHollowProfile.Create(profile.Depth, CreateFlange(profile.Flanges), CreateWebConstant(profile.Webs));
    }

    public static Oasys.Profiles.IRectangleProfile CreateRectangleProfile(IRectangleProfile profile) {
      return Oasys.Profiles.IRectangleProfile.Create(profile.Depth, profile.Width);
    }

    public static Oasys.Profiles.IFlange CreateFlange(IFlange flange) {
      return Oasys.Profiles.IFlange.Create(flange.Width, flange.Thickness);
    }

    public static Oasys.Profiles.IStadiumProfile CreateStadiumProfile(IRectoCircleProfile profile) {
      return Oasys.Profiles.IStadiumProfile.Create(profile.Depth, profile.Width);
    }

    public static Oasys.Profiles.IRectoEllipseProfile CreateRectoEllipseProfile(IRectoEllipseProfile profile) {
      return Oasys.Profiles.IRectoEllipseProfile.Create(profile.Depth, profile.DepthFlat, profile.Width, profile.WidthFlat);
    }

    public static Oasys.Profiles.ISecantPileProfile CreateSecantPileProfile(ISecantPileProfile profile) {
      return Oasys.Profiles.ISecantPileProfile.Create(profile.Diameter, profile.PileCentres, profile.PileCount, profile.IsWall);
    }

    public static Oasys.Profiles.ISheetPileProfile CreateSheetPileProfile(ISheetPileProfile profile) {
      return Oasys.Profiles.ISheetPileProfile.Create(profile.Depth, profile.Width, profile.TopFlangeWidth, profile.BottomFlangeWidth, profile.FlangeThickness, profile.WebThickness);
    }

    public static Oasys.Profiles.ITrapezoidProfile CreateTrapezoidProfile(ITrapezoidProfile profile) {
      return Oasys.Profiles.ITrapezoidProfile.Create(profile.Depth, profile.TopWidth, profile.BottomWidth);
    }

    public static Oasys.Profiles.ITSectionProfile CreateTSectionProfile(ITSectionProfile profile) {
      return Oasys.Profiles.ITSectionProfile.Create(profile.Depth, CreateFlange(profile.Flange), CreateWeb(profile.Web));
    }

    public static Oasys.Profiles.IWebConstant CreateWebConstant(IWebConstant web) {
      return Oasys.Profiles.IWebConstant.Create(web.Thickness);
    }

    public static Oasys.Profiles.IWebTapered CreateWebTapered(IWebTapered web) {
      return Oasys.Profiles.IWebTapered.Create(web.TopThickness, web.BottomThickness);
    }

    public static Oasys.Profiles.IWeb CreateWeb(IWeb web) {
      if (web is IWebConstant webConstant) {
        return CreateWebConstant(webConstant);
      }
      else if (web is IWebTapered webTapered) {
        return CreateWebTapered(webTapered);
      }
      else { return null; }

    }
  }
}
