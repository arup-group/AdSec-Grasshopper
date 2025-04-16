using System;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
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

      CreatePreview(ref _profile, ref _profileEdge, ref _profileVoidEdges, ref _profileColour, ref _rebars,
        ref _rebarEdges, ref _linkEdges, ref _rebarColours, ref _subProfiles, ref _subEdges, ref _subVoidEdges,
        ref _subColours);
    }

    public AdSecSection(
      ISection section, IDesignCode code, string codeName, string materialName, Plane local,
      IPoint subComponentOffset = null) {
      _materialName = materialName;
      Section = section;
      DesignCode = code;
      _codeName = codeName;
      LocalPlane = local;
      CreatePreview(ref _profile, ref _profileEdge, ref _profileVoidEdges, ref _profileColour, ref _rebars,
        ref _rebarEdges, ref _linkEdges, ref _rebarColours, ref _subProfiles, ref _subEdges, ref _subVoidEdges,
        ref _subColours, subComponentOffset);
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
      CreatePreview(ref _profile, ref _profileEdge, ref _profileVoidEdges, ref _profileColour, ref _rebars,
        ref _rebarEdges, ref _linkEdges, ref _rebarColours, ref _subProfiles, ref _subEdges, ref _subVoidEdges,
        ref _subColours);
    }

    public IDesignCode DesignCode { get; set; }
    public bool IsValid => Section != null;
    public Plane LocalPlane { get; set; }
    public ISection Section { get; set; }
    internal Brep SolidBrep => _profile;
    internal List<Brep> SubBreps => _subProfiles;

    public AdSecSection Duplicate() {
      if (this == null) {
        return null;
      }

      var dup = (AdSecSection)MemberwiseClone();
      return dup;
    }

    public override string ToString() {
      return Section.Profile.Description();
    }

    internal void CreatePreview(
      ref Brep profile, ref Polyline profileEdge, ref List<Polyline> profileVoidEdges,
      ref DisplayMaterial profileColour, ref List<Brep> rebars, ref List<Circle> rebarEdges, ref List<Curve> linkEdges,
      ref List<DisplayMaterial> rebarColours, ref List<Brep> subProfiles, ref List<Polyline> subEdges,
      ref List<List<Polyline>> subVoidEdges, ref List<DisplayMaterial> subColours, IPoint offset = null) {
      ISection flat = null;
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

      var offs = Vector3d.Zero;

      if (offset != null) {
        offs = new Vector3d(0, offset.Y.As(DefaultUnits.LengthUnitGeometry),
          offset.Z.As(DefaultUnits.LengthUnitGeometry));
      }

      profile = CreateBrepFromProfile(new AdSecProfileGoo(flat.Profile, LocalPlane));
      profile.Transform(Transform.Translation(offs));
      var edges = AdSecProfileGoo.PolylinesFromAdSecProfile(flat.Profile, LocalPlane);
      profileEdge = edges.Item1;
      profileEdge.Transform(Transform.Translation(offs));
      profileVoidEdges = edges.Item2;

      string materialString = Section.Material.ToString();
      materialString = materialString.Replace("Oasys.AdSec.Materials.I", "");
      materialString = materialString.Replace("_Implementation", "");
      Enum.TryParse(materialString, out AdSecMaterial.AdSecMaterialType profileType);
      switch (profileType) {
        case AdSecMaterial.AdSecMaterialType.Concrete:
          profileColour = Colour.Concrete;
          break;

        case AdSecMaterial.AdSecMaterialType.Steel:
          profileColour = Colour.Steel;
          break;
      }

      subProfiles = new List<Brep>();
      subColours = new List<DisplayMaterial>();
      subEdges = new List<Polyline>();
      subVoidEdges = new List<List<Polyline>>();
      foreach (var subComponent in flat.SubComponents) {
        var brepFromProfile = CreateBrepFromProfile(new AdSecProfileGoo(subComponent.Section.Profile, LocalPlane));
        var transform = new Vector3d(0, subComponent.Offset.Y.As(DefaultUnits.LengthUnitGeometry),
          subComponent.Offset.Z.As(DefaultUnits.LengthUnitGeometry));
        brepFromProfile.Transform(Transform.Translation(transform));
        brepFromProfile.Transform(Transform.Translation(offs));
        subProfiles.Add(brepFromProfile);

        var subedges = AdSecProfileGoo.PolylinesFromAdSecProfile(subComponent.Section.Profile, LocalPlane);
        var subedge = subedges.Item1;
        subedge.Transform(Transform.Translation(transform));
        subedge.Transform(Transform.Translation(offs));
        subEdges.Add(subedge);

        var polylines = subedges.Item2;
        foreach (var polyline in polylines) {
          polyline.Transform(Transform.Translation(transform));
          polyline.Transform(Transform.Translation(offs));
        }

        subVoidEdges.Add(polylines);

        string submaterialString = subComponent.Section.Material.ToString();
        submaterialString = submaterialString.Replace("Oasys.AdSec.Materials.I", "");
        submaterialString = submaterialString.Replace("_Implementation", "");
        Enum.TryParse(submaterialString, out AdSecMaterial.AdSecMaterialType subType);
        DisplayMaterial subColour = null;
        switch (subType) {
          case AdSecMaterial.AdSecMaterialType.Concrete:
            subColour = Colour.Concrete;
            break;

          case AdSecMaterial.AdSecMaterialType.Steel:
            subColour = Colour.Steel;
            break;
        }

        subColours.Add(subColour);
      }

      rebars = new List<Brep>();
      rebarColours = new List<DisplayMaterial>();
      rebarEdges = new List<Circle>();
      linkEdges = new List<Curve>();
      foreach (var rebargroup in flat.ReinforcementGroups) {
        try {
          var singleBars = (ISingleBars)rebargroup;
          var baredges = new List<Circle>();
          var barbreps = CreateBrepsFromSingleRebar(singleBars, offs, ref baredges, LocalPlane);
          rebars.AddRange(barbreps);
          rebarEdges.AddRange(baredges);

          string rebmatrebarMaterialString = singleBars.BarBundle.Material.ToString();
          rebmatrebarMaterialString = rebmatrebarMaterialString.Replace("Oasys.AdSec.Materials.I", string.Empty);
          rebmatrebarMaterialString = rebmatrebarMaterialString.Replace("_Implementation", string.Empty);
          Enum.TryParse(rebmatrebarMaterialString, out AdSecMaterial.AdSecMaterialType rebarType);
          var rebarColour = Colour.Reinforcement;
          switch (rebarType) {
            case AdSecMaterial.AdSecMaterialType.Rebar:
              rebarColour = Colour.Reinforcement;
              break;

            case AdSecMaterial.AdSecMaterialType.FRP:
            case AdSecMaterial.AdSecMaterialType.Tendon:
              rebarColour = Colour.Reinforcement;
              break;
          }

          for (int i = 0; i < barbreps.Count; i++) {
            rebarColours.Add(rebarColour);
          }
        } catch (Exception) {
          try {
            var linkGroup = (IPerimeterLinkGroup)rebargroup;
            CreateCurvesFromLinkGroup(linkGroup, ref linkEdges, LocalPlane);
          } catch (Exception) {
            /* don't expect to fail */
          }
        }
      }

      if (LocalPlane == null || LocalPlane == Plane.WorldXY || LocalPlane == Plane.WorldYZ
        || LocalPlane == Plane.WorldZX) {
        return;
      }

      var area = Section.Profile.Area();
      double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
      var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
      previewXaxis = new Line(LocalPlane.Origin, LocalPlane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
      previewYaxis = new Line(LocalPlane.Origin, LocalPlane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
      previewZaxis = new Line(LocalPlane.Origin, LocalPlane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
    }

    private Brep CreateBrepFromProfile(AdSecProfileGoo profile) {
      var curves = new List<Curve> {
        profile.Polyline.ToPolylineCurve(),
      };
      curves.AddRange(profile.VoidEdges.Select(x => x.ToPolylineCurve()));
      return Brep.CreatePlanarBreps(curves, tolerance)[0]; //TODO: use OasysUnits tolerance
    }

    private List<Brep> CreateBrepsFromSingleRebar(
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

    private void CreateCurvesFromLinkGroup(IPerimeterLinkGroup linkGroup, ref List<Curve> linkEdges, Plane local) {
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

    private Tuple<Oasys.Collections.IList<IGroup>, ICover> CreateReinforcementGroupsWithMaxCover(
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
}
