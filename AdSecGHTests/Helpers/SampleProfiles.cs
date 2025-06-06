using System.Collections.Generic;

using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGHTests.Helpers {
  public static class SampleProfiles {
    public static Length GetThickness() {
      return new Length(0.2, DefaultUnits.LengthUnitGeometry);
    }

    public static Flange GetFlange() {
      return new Flange(GetThickness(), LengthOne());
    }

    public static Length LengthOne() {
      return new Length(1, DefaultUnits.LengthUnitGeometry);
    }

    public static WebConstant GetWebConstant() {
      return new WebConstant(GetThickness());
    }

    public static AngleProfile GetAnAngleProfile(IWeb web) {
      return new AngleProfile {
        Depth = LengthOne(),
        Rotation = Angle.Zero,
        Flange = GetFlange(),
        Web = web,
      };
    }

    public static CatalogueProfile GetACatalogueProfile() {
      return new CatalogueProfile() {
        Rotation = Angle.Zero,
        Description = "CAT BSI-IPE IPEAA80",
      };
    }

    public static ChannelProfile GetAChannelProfile() {
      return new ChannelProfile() {
        Rotation = Angle.Zero,
        Web = GetWebConstant(),
        Flanges = GetFlange(),
        Depth = LengthOne(),
      };
    }

    public static CircleProfile GetACircleProfile() {
      return new CircleProfile() {
        Rotation = Angle.Zero,
        Diameter = LengthOne(),
      };
    }

    public static CircleHollowProfile GetACircleHollowProfile() {
      return new CircleHollowProfile() {
        WallThickness = GetThickness(),
        Rotation = Angle.Zero,
        Diameter = LengthOne(),
      };
    }

    public static CruciformSymmetricalProfile GetACruciformSymmetricalProfile() {
      return new CruciformSymmetricalProfile() {
        Rotation = Angle.Zero,
        Web = GetWebConstant(),
        Depth = LengthOne(),
        Flange = GetFlange(),
      };
    }

    public static EllipseProfile GetAnEllipseProfile() {
      return new EllipseProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Width = LengthOne(),
      };
    }

    public static EllipseHollowProfile GetAnEllipseHollowProfile() {
      return new EllipseHollowProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Width = LengthOne(),
        WallThickness = GetThickness(),
      };
    }

    public static GeneralCProfile GetAGeneralCProfile() {
      return new GeneralCProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Thickness = GetThickness(),
        FlangeWidth = LengthOne(),
        Lip = LengthOne(),
      };
    }

    public static GeneralZProfile GetAGeneralZProfile() {
      return new GeneralZProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Thickness = GetThickness(),
        BottomFlangeWidth = LengthOne(),
        BottomLip = GetThickness(),
        TopFlangeWidth = LengthOne(),
        TopLip = GetThickness(),
      };
    }

    public static IBeamProfile GetABeamProfile() {
      return new IBeamProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Web = GetWebConstant(),
        Flanges = GetFlange(),
      };
    }

    public static IBeamAsymmetricalProfile GetAnIBeamAssymetricalProfile() {
      return new IBeamAsymmetricalProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Web = GetWebConstant(),
        BottomFlange = GetFlange(),
        TopFlange = GetFlange(),
      };
    }

    public static IBeamCellularProfile GetAnIBeamCellularProfile() {
      return new IBeamCellularProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Web = GetWebConstant(),
        Flanges = GetFlange(),
        Spacing = GetThickness(),
        WebOpening = GetThickness(),
        OpeningType = IBeamOpeningType.Castellated,
      };
    }

    public static PerimeterProfile GetAPerimeterProfile() {
      return new PerimeterProfile() {
        Rotation = Angle.Zero,
        Perimeter = new Polygon(new List<IPoint2d>() {
          new Point2d(LengthOne(), LengthOne()),
        }),
        VoidPolygons = new List<IPolygon>() {
          new Polygon(new List<IPoint2d>() {
            new Point2d(LengthOne(), LengthOne()),
          }),
        },
      };
    }

    public static RectangleProfile GetARectangleProfile() {
      return new RectangleProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Width = LengthOne(),
      };
    }

    public static RectangleHollowProfile GetARectangleHollowProfile() {
      return new RectangleHollowProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Flanges = GetFlange(),
        Webs = GetWebConstant(),
      };
    }

    public static RectoCircleProfile GetARectoCircleProfile() {
      return new RectoCircleProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Width = LengthOne(),
      };
    }

    public static RectoEllipseProfile GetARectoEllipseProfile() {
      return new RectoEllipseProfile() {
        Rotation = Angle.Zero,
        Depth = LengthOne(),
        Width = LengthOne(),
        DepthFlat = GetThickness(),
        WidthFlat = GetThickness(),
      };
    }

    public static SecantPileProfile GetASecantPileProfile() {
      return new SecantPileProfile() {
        Rotation = Angle.Zero,
        Diameter = LengthOne(),
        IsWall = true,
        PileCentres = GetThickness(),
        PileCount = 2,
      };
    }

    public static SheetPileProfile GetASheetPileProfile() {
      return new SheetPileProfile() {
        Rotation = Angle.Zero,
        BottomFlangeWidth = LengthOne(),
        FlangeThickness = GetThickness(),
        Depth = LengthOne(),
        TopFlangeWidth = LengthOne(),
        WebThickness = GetThickness(),
        Width = LengthOne(),
      };
    }

    public static TrapezoidProfile GetATrapezoidProfile() {
      return new TrapezoidProfile() {
        Rotation = Angle.Zero,
        BottomWidth = LengthOne(),
        Depth = LengthOne(),
        TopWidth = LengthOne(),
      };
    }

    public static TSectionProfile GetATSectionProfile() {
      return new TSectionProfile {
        Depth = LengthOne(),
        Rotation = Angle.Zero,
        Flange = GetFlange(),
        Web = GetWebConstant(),
      };
    }
  }
}
