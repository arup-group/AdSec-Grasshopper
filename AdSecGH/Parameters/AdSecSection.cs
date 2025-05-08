using System;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.Geometry.Paths2D;
using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSection {
    private const double tolerance = 0.001;
    internal string _codeName;
    internal string _materialName;
    internal List<Brep> _subProfiles;
    internal List<Curve> _linkEdges = new List<Curve>();
    internal Brep _profile;
    internal DisplayMaterial _profileColour;
    internal Polyline _profileEdge;
    internal List<Polyline> _profileVoidEdges = new List<Polyline>();
    internal List<DisplayMaterial> _rebarColours;
    internal List<Circle> _rebarEdges = new List<Circle>();
    internal List<Brep> _rebars;
    internal List<DisplayMaterial> _subColours;
    internal List<Polyline> _subEdges = new List<Polyline>();
    internal List<List<Polyline>> _subVoidEdges = new List<List<Polyline>>();
    internal Line previewXaxis;
    internal Line previewYaxis;
    internal Line previewZaxis;

    public AdSecSection(SectionDesign sectionDesign) {
      Section = sectionDesign.Section;
      DesignCode = sectionDesign.DesignCode.IDesignCode;
      _codeName = sectionDesign.DesignCode.DesignCodeName;
      _materialName = sectionDesign.MaterialName;
      LocalPlane = sectionDesign.LocalPlane.ToGh();

      AssignPreviewData();
    }

    public AdSecSection(
      ISection section, IDesignCode code, string codeName, string materialName, Plane local,
      IPoint subComponentOffset = null) {
      _materialName = materialName;
      Section = section;
      DesignCode = code;
      _codeName = codeName;
      LocalPlane = local;
      AssignPreviewData(subComponentOffset);
    }

    public AdSecSection(
      IProfile profile, Plane local, AdSecMaterial material, List<AdSecRebarGroup> reinforcement, Oasys.Collections.IList<ISubComponent> subComponents) {
      DesignCode = material.DesignCode.Duplicate().DesignCode;
      _codeName = material.DesignCodeName;
      _materialName = material.GradeName;
      Section = ISection.Create(profile, material.Material);
      var rebarAndCover = CreateReinforcementGroupsWithMaxCover(reinforcement);
      Section.ReinforcementGroups = rebarAndCover.Item1;
      if (rebarAndCover.Item2 != null) {
        Section.Cover = rebarAndCover.Item2;
      }

      if (subComponents != null) {
        Section.SubComponents = subComponents;
      }

      LocalPlane = local;
      AssignPreviewData();
    }

    public IDesignCode DesignCode { get; set; }
    public bool IsValid => SolidBrep != null || SolidBrep.IsValid;
    public Plane LocalPlane { get; set; }
    public ISection Section { get; set; }
    internal Brep SolidBrep => _profile;
    internal List<Brep> SubBreps => _subProfiles;

    public AdSecSection Duplicate() {
      return IsValid ? (AdSecSection)MemberwiseClone() : null;
    }

    public override string ToString() {
      return Section.Profile.Description();
    }

    private void GenerateLocalPlanePreviewAxes() {
      if (LocalPlane == Plane.WorldXY || LocalPlane == Plane.WorldYZ || LocalPlane == Plane.WorldZX) {
        return;
      }

      var area = Section.Profile.Area();
      double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
      var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
      previewXaxis = new Line(LocalPlane.Origin, LocalPlane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
      previewYaxis = new Line(LocalPlane.Origin, LocalPlane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
      previewZaxis = new Line(LocalPlane.Origin, LocalPlane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
    }

    private static Transform GetCombinedTransform(IPoint offset, Vector3d currentOffset) {
      var localOffset = new Vector3d(0, offset.Y.As(DefaultUnits.LengthUnitGeometry),
        offset.Z.As(DefaultUnits.LengthUnitGeometry));

      return Transform.Translation(localOffset + currentOffset);
    }

    public static void MapMaterialToProfileColour(IMaterial material, out DisplayMaterial profileColour) {
      string materialString = material.ToString();
      materialString = materialString.Replace("Oasys.AdSec.Materials.I", "").Replace("_Implementation", "");
      Enum.TryParse(materialString, out AdSecMaterial.AdSecMaterialType profileType);

      switch (profileType) {
        case AdSecMaterial.AdSecMaterialType.Concrete:
          profileColour = Colour.Concrete;
          break;

        case AdSecMaterial.AdSecMaterialType.Steel:
          profileColour = Colour.Steel;
          break;
        case AdSecMaterial.AdSecMaterialType.FRP:
        case AdSecMaterial.AdSecMaterialType.Tendon:
        case AdSecMaterial.AdSecMaterialType.Rebar:
          profileColour = Colour.Reinforcement;
          break;
        default:
          profileColour = Colour.Reinforcement;
          break;
      }
    }

    private void GenerateProfileGeometryWithOffset(
      out Brep profile, out Polyline profileEdge, out List<Polyline> profileVoidEdges, ISection flat,
      Vector3d currentOffset) {
      profile = CreateBrepFromProfile(new AdSecProfileGoo(flat.Profile, LocalPlane));
      profile.Transform(Transform.Translation(currentOffset));
      var edges = AdSecProfileGoo.PolylinesFromAdSecProfile(flat.Profile, LocalPlane);
      profileEdge = edges.Item1;
      profileEdge.Transform(Transform.Translation(currentOffset));
      profileVoidEdges = edges.Item2;
    }

    private static Vector3d ApplyOffsetToVector(IPoint offsetPoint, Vector3d currentOffset) {
      if (offsetPoint != null) {
        currentOffset = new Vector3d(0, offsetPoint.Y.As(DefaultUnits.LengthUnitGeometry),
          offsetPoint.Z.As(DefaultUnits.LengthUnitGeometry));
      }

      return currentOffset;
    }

    private ISection SetFlattenSection() {
      ISection flat;
      if (DesignCode != null) {
        var adSec = IAdSec.Create(DesignCode);
        flat = adSec.Flatten(Section);
      } else {
        var perimeterProfile = IPerimeterProfile.Create(Section.Profile);
        flat = ISection.Create(perimeterProfile, Section.Material);
        if (Section.ReinforcementGroups.Count > 0) {
          flat.ReinforcementGroups = Section.ReinforcementGroups;
        }

        if (Section.SubComponents.Count > 0) {
          flat.SubComponents = Section.SubComponents;
        }
      }

      return flat;
    }

    internal void AssignPreviewData(IPoint subComponentOffset = null) {
      CreatePreview(out var profileData, out var reinforcementData, out var subData, subComponentOffset);
      _profile = profileData.Profile;
      _profileEdge = profileData.ProfileEdge;
      _profileVoidEdges = profileData.ProfileVoidEdges;
      _profileColour = profileData.ProfileColour;
      _rebars = reinforcementData.Rebars;
      _rebarEdges = reinforcementData.RebarEdges;
      _linkEdges = reinforcementData.LinkEdges;
      _rebarColours = reinforcementData.RebarColours;
      _subProfiles = subData.SubProfiles;
      _subEdges = subData.SubEdges;
      _subVoidEdges = subData.SubVoidEdges;
      _subColours = subData.SubColours;
    }

    internal void CreatePreview(
      out ProfilePreviewData profileData, out ReinforcementPreviewData reinforcementData,
      out SubComponentsPreviewData subData, IPoint pointOffset = null) {
      var flat = SetFlattenSection();
      var currentOffset = ApplyOffsetToVector(pointOffset, Vector3d.Zero);

      profileData = GenerateProfilePreview(flat, currentOffset);
      subData = GenerateSubComponentsPreview(flat, currentOffset);
      reinforcementData = GenerateReinforcementPreview(flat, currentOffset);

      GenerateLocalPlanePreviewAxes();
    }

    internal ProfilePreviewData GenerateProfilePreview(ISection flat, Vector3d currentOffset) {
      GenerateProfileGeometryWithOffset(out var profile, out var edge, out var voids, flat, currentOffset);
      MapMaterialToProfileColour(Section.Material, out var colour);

      return new ProfilePreviewData {
        Profile = profile,
        ProfileEdge = edge,
        ProfileVoidEdges = voids,
        ProfileColour = colour,
      };
    }

    private SubComponentsPreviewData GenerateSubComponentsPreview(ISection flat, Vector3d currentOffset) {
      var subProfiles = new List<Brep>();
      var subEdges = new List<Polyline>();
      var subVoidEdges = new List<List<Polyline>>();
      var subColours = new List<DisplayMaterial>();

      foreach (var subComponent in flat.SubComponents) {
        var subProfile = subComponent.Section.Profile;
        var transform = GetCombinedTransform(subComponent.Offset, currentOffset);

        var brepFromProfile = CreateBrepFromProfile(new AdSecProfileGoo(subProfile, LocalPlane));
        brepFromProfile.Transform(transform);
        subProfiles.Add(brepFromProfile);

        var edges = AdSecProfileGoo.PolylinesFromAdSecProfile(subProfile, LocalPlane);
        var mainEdge = edges.Item1;
        var voidEdges = edges.Item2;

        mainEdge.Transform(transform);
        foreach (var polyline in voidEdges) {
          polyline.Transform(transform);
        }

        subEdges.Add(mainEdge);
        subVoidEdges.Add(voidEdges);

        MapMaterialToProfileColour(subComponent.Section.Material, out var subColour);
        subColours.Add(subColour);
      }

      return new SubComponentsPreviewData {
        SubProfiles = subProfiles,
        SubEdges = subEdges,
        SubVoidEdges = subVoidEdges,
        SubColours = subColours,
      };
    }

    private ReinforcementPreviewData GenerateReinforcementPreview(ISection flat, Vector3d currentOffset) {
      var rebars = new List<Brep>();
      var rebarColours = new List<DisplayMaterial>();
      var rebarEdges = new List<Circle>();
      var linkEdges = new List<Curve>();

      foreach (var group in flat.ReinforcementGroups) {
        switch (group) {
          case ISingleBars singleBars: {
              var barEdges = new List<Circle>();
              var barBreps = CreateBrepsFromSingleRebar(singleBars, currentOffset, ref barEdges, LocalPlane);

              rebars.AddRange(barBreps);
              rebarEdges.AddRange(barEdges);

              MapMaterialToProfileColour(singleBars.BarBundle.Material, out var color);
              rebarColours.AddRange(Enumerable.Repeat(color, barBreps.Count));
              break;
            }
          case IPerimeterLinkGroup linkGroup: {
              CreateCurvesFromLinkGroup(linkGroup, ref linkEdges, LocalPlane);
              break;
            }
        }
      }

      return new ReinforcementPreviewData {
        Rebars = rebars,
        RebarEdges = rebarEdges,
        RebarColours = rebarColours,
        LinkEdges = linkEdges,
      };
    }

    private static Brep CreateBrepFromProfile(AdSecProfileGoo profile) {
      var curves = new List<Curve> {
        profile.Polyline.ToPolylineCurve(),
      };
      curves.AddRange(profile.VoidEdges.Select(x => x.ToPolylineCurve()));
      return Brep.CreatePlanarBreps(curves, tolerance)[0]; //TODO: use OasysUnits tolerance
    }

    private static List<Brep> CreateBrepsFromSingleRebar(
      ISingleBars bars, Vector3d offset, ref List<Circle> edgeCurves, Plane local) {
      var mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);
      var rebarBreps = new List<Brep>();
      foreach (var position in bars.Positions) {
        var center = new Point3d(0, position.Y.As(DefaultUnits.LengthUnitGeometry),
          position.Z.As(DefaultUnits.LengthUnitGeometry));
        center.Transform(Transform.Translation(offset));
        center.Transform(mapToLocal);
        var localCenter = new Plane(center, local.Normal);
        var edgeCurve = new Circle(localCenter, bars.BarBundle.Diameter.As(DefaultUnits.LengthUnitGeometry) / 2);
        edgeCurves.Add(edgeCurve);
        var curves = new List<Curve> {
          edgeCurve.ToNurbsCurve(),
        };
        rebarBreps.Add(Brep.CreatePlanarBreps(curves, tolerance)[0]); //TODO: use OasysUnits tolerance
      }

      return rebarBreps;
    }

    private static void CreateCurvesFromLinkGroup(
      IPerimeterLinkGroup linkGroup, ref List<Curve> linkEdges, Plane local) {
      var mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);
      var startPoint = new Point3d(0, linkGroup.LinkPath.StartPoint.Y.As(DefaultUnits.LengthUnitGeometry),
        linkGroup.LinkPath.StartPoint.Z.As(DefaultUnits.LengthUnitGeometry));
      startPoint.Transform(mapToLocal);

      var centreLine = new PolyCurve();
      foreach (var path in linkGroup.LinkPath.Segments) {
        try {
          var line = (ILineSegment<IPoint>)path;
          var nextPoint = new Point3d(0, line.NextPoint.Y.As(DefaultUnits.LengthUnitGeometry),
            line.NextPoint.Z.As(DefaultUnits.LengthUnitGeometry));
          nextPoint.Transform(mapToLocal);

          var rhinoLine = new Line(startPoint, nextPoint);
          startPoint = nextPoint;
          centreLine.Append(rhinoLine);
        } catch (Exception) {
          var arc = (IArcSegment<IPoint>)path;
          var centrePoint = new Point3d(0, arc.Centre.Y.As(DefaultUnits.LengthUnitGeometry),
            arc.Centre.Z.As(DefaultUnits.LengthUnitGeometry));
          centrePoint.Transform(mapToLocal);

          double radius = startPoint.DistanceTo(centrePoint);
          var xAxis = new Vector3d(startPoint - centrePoint);
          var yAxis = Vector3d.CrossProduct(local.ZAxis, xAxis);
          var arcPlane = new Plane(centrePoint, xAxis, yAxis);
          double sweepAngle = arc.SweepAngle.As(AngleUnit.Radian);
          var rhinoArc = new Arc(arcPlane, radius, sweepAngle);
          startPoint = rhinoArc.EndPoint;
          centreLine.Append(rhinoArc);
        }
      }

      double barDiameter = linkGroup.BarBundle.Diameter.As(DefaultUnits.LengthUnitGeometry);
      var offset1 = centreLine.Offset(local, barDiameter / 2, tolerance, CurveOffsetCornerStyle.Sharp);
      var offset2 = centreLine.Offset(local, barDiameter / 2 * -1, tolerance, CurveOffsetCornerStyle.Sharp);

      if (linkEdges == null) {
        linkEdges = new List<Curve>();
      }

      linkEdges.AddRange(new List<Curve> {
        offset1[0],
        offset2[0],
      });
    }

    private static Tuple<Oasys.Collections.IList<IGroup>, ICover> CreateReinforcementGroupsWithMaxCover(
      List<AdSecRebarGroup> reinforcements) {
      var groups = Oasys.Collections.IList<IGroup>.Create();
      ICover cover = null;
      foreach (var reinforcement in reinforcements) {
        groups.Add(reinforcement.Group);

        if (reinforcement.Group is ILinkGroup || reinforcement.Group is IPerimeterGroup
          || reinforcement.Group is ITemplateGroup) {
          GetCover(reinforcement, ref cover);
        }
      }

      return new Tuple<Oasys.Collections.IList<IGroup>, ICover>(groups, cover);
    }

    private static void GetCover(AdSecRebarGroup reinforcement, ref ICover cover) {
      if (reinforcement.Cover != null && (cover == null || reinforcement.Cover.UniformCover > cover.UniformCover)) {
        cover = reinforcement.Cover;
      }
    }
  }

  internal class ProfilePreviewData {
    public Brep Profile { get; set; }
    public Polyline ProfileEdge { get; set; }
    public List<Polyline> ProfileVoidEdges { get; set; }
    public DisplayMaterial ProfileColour { get; set; }
  }

  internal class ReinforcementPreviewData {
    public List<Brep> Rebars { get; set; } = new List<Brep>();
    public List<Circle> RebarEdges { get; set; } = new List<Circle>();
    public List<Curve> LinkEdges { get; set; } = new List<Curve>();
    public List<DisplayMaterial> RebarColours { get; set; } = new List<DisplayMaterial>();
  }

  internal class SubComponentsPreviewData {
    public List<Brep> SubProfiles { get; set; } = new List<Brep>();
    public List<Polyline> SubEdges { get; set; } = new List<Polyline>();
    public List<List<Polyline>> SubVoidEdges { get; set; } = new List<List<Polyline>>();
    public List<DisplayMaterial> SubColours { get; set; } = new List<DisplayMaterial>();
  }

}
