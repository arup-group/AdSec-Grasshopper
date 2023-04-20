using System;
using System.Collections.Generic;
using System.Linq;
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
    public IDesignCode DesignCode { get; set; }
    public bool IsValid {
      get {
        if (Section == null) {
          return false;
        }
        return true;
      }
    }
    public Plane LocalPlane { get; set; }
    public ISection Section { get; set; }
    internal Brep SolidBrep => m_profile;
    internal List<Brep> SubBreps => _subProfiles;
    internal string _codeName;
    internal List<Curve> m_linkEdges;
    // cache for preview
    internal Brep m_profile;
    internal DisplayMaterial m_profileColour;
    internal Polyline m_profileEdge;
    internal List<Polyline> m_profileVoidEdges;
    internal List<DisplayMaterial> m_rebarColours;
    internal List<Circle> m_rebarEdges;
    internal List<Brep> m_rebars;
    internal List<DisplayMaterial> m_subColours;
    internal List<Polyline> m_subEdges;
    internal List<Brep> _subProfiles;
    internal List<List<Polyline>> m_subVoidEdges;
    internal string _materialName;
    internal Line previewXaxis;
    internal Line previewYaxis;
    internal Line previewZaxis;

    public AdSecSection(ISection section, IDesignCode code, string codeName, string materialName, Plane local, IPoint subComponentOffset = null) {
      _materialName = materialName;
      Section = section;
      DesignCode = code;
      _codeName = codeName;
      LocalPlane = local;
      CreatePreview(ref m_profile, ref m_profileEdge, ref m_profileVoidEdges, ref m_profileColour, ref m_rebars, ref m_rebarEdges, ref m_linkEdges, ref m_rebarColours, ref _subProfiles, ref m_subEdges, ref m_subVoidEdges, ref m_subColours, subComponentOffset);
    }

    public AdSecSection(IProfile profile, Plane local, AdSecMaterial material, List<AdSecRebarGroup> reinforcement, Oasys.Collections.IList<ISubComponent> subComponents) {
      DesignCode = material.DesignCode.Duplicate().DesignCode;
      _codeName = material.DesignCodeName;
      _materialName = material.GradeName;
      Section = ISection.Create(profile, material.Material);
      Tuple<Oasys.Collections.IList<IGroup>, ICover> rebarAndCover = CreateReinforcementGroupsWithMaxCover(reinforcement);
      Section.ReinforcementGroups = rebarAndCover.Item1;
      if (rebarAndCover.Item2 != null) {
        Section.Cover = rebarAndCover.Item2;
      }
      if (subComponents != null) {
        Section.SubComponents = subComponents;
      }
      LocalPlane = local;
      CreatePreview(ref m_profile, ref m_profileEdge, ref m_profileVoidEdges, ref m_profileColour, ref m_rebars, ref m_rebarEdges, ref m_linkEdges, ref m_rebarColours, ref _subProfiles, ref m_subEdges, ref m_subVoidEdges, ref m_subColours);
    }

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

    internal void CreatePreview(ref Brep profile, ref Polyline profileEdge, ref List<Polyline> profileVoidEdges, ref DisplayMaterial profileColour, ref List<Brep> rebars, ref List<Circle> rebarEdges, ref List<Curve> linkEdges, ref List<DisplayMaterial> rebarColours, ref List<Brep> subProfiles, ref List<Polyline> subEdges, ref List<List<Polyline>> subVoidEdges, ref List<DisplayMaterial> subColours, IPoint offset = null) {
      ISection flat = null;
      if (DesignCode != null) //{ code = Oasys.AdSec.DesignCode.EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex; }
      {
        var adSec = IAdSec.Create(DesignCode);
        flat = adSec.Flatten(Section);
      } else {
        var prof = IPerimeterProfile.Create(Section.Profile);
        flat = ISection.Create(prof, Section.Material);
        if (Section.ReinforcementGroups.Count > 0) {
          flat.ReinforcementGroups = Section.ReinforcementGroups;
        }
        if (Section.SubComponents.Count > 0) {
          flat.SubComponents = Section.SubComponents;
        }
      }

      // create offset if any
      Vector3d offs = Vector3d.Zero;

      if (offset != null) {
        offs = new Vector3d(0,
            offset.Y.As(DefaultUnits.LengthUnitGeometry),
            offset.Z.As(DefaultUnits.LengthUnitGeometry));
      }

      // primary profile
      profile = CreateBrepFromProfile(new AdSecProfileGoo(flat.Profile, LocalPlane));
      profile.Transform(Transform.Translation(offs));
      Tuple<Polyline, List<Polyline>> edges = AdSecProfileGoo.PolylinesFromAdSecProfile(flat.Profile, LocalPlane);
      profileEdge = edges.Item1;
      profileEdge.Transform(Transform.Translation(offs));
      profileVoidEdges = edges.Item2;

      // get material
      string mat = Section.Material.ToString();
      mat = mat.Replace("Oasys.AdSec.Materials.I", "");
      mat = mat.Replace("_Implementation", "");
      Enum.TryParse(mat, out AdSecMaterial.AdSecMaterialType profileType);
      switch (profileType) {
        case AdSecMaterial.AdSecMaterialType.Concrete:
          profileColour = UI.Colour.Concrete;
          break;

        case AdSecMaterial.AdSecMaterialType.Steel:
          profileColour = UI.Colour.Steel;
          break;
      }

      // sub components
      subProfiles = new List<Brep>();
      subColours = new List<DisplayMaterial>();
      subEdges = new List<Polyline>();
      subVoidEdges = new List<List<Polyline>>();
      foreach (ISubComponent sub in flat.SubComponents) {
        Brep temp = CreateBrepFromProfile(new AdSecProfileGoo(sub.Section.Profile, LocalPlane));
        var trans = new Vector3d(
            0,
            sub.Offset.Y.As(DefaultUnits.LengthUnitGeometry),
            sub.Offset.Z.As(DefaultUnits.LengthUnitGeometry));
        temp.Transform(Transform.Translation(trans));
        temp.Transform(Transform.Translation(offs));
        subProfiles.Add(temp);

        Tuple<Polyline, List<Polyline>> subedges = AdSecProfileGoo.PolylinesFromAdSecProfile(sub.Section.Profile, LocalPlane);
        Polyline subedge = subedges.Item1;
        subedge.Transform(Transform.Translation(trans));
        subedge.Transform(Transform.Translation(offs));
        subEdges.Add(subedge);

        List<Polyline> subvoids = subedges.Item2;
        foreach (Polyline crv in subvoids) {
          crv.Transform(Transform.Translation(trans));
          crv.Transform(Transform.Translation(offs));
        }
        subVoidEdges.Add(subvoids);

        string submat = sub.Section.Material.ToString();
        submat = submat.Replace("Oasys.AdSec.Materials.I", "");
        submat = submat.Replace("_Implementation", "");
        Enum.TryParse(submat, out AdSecMaterial.AdSecMaterialType subType);
        DisplayMaterial subColour = null;
        switch (subType) {
          case AdSecMaterial.AdSecMaterialType.Concrete:
            subColour = UI.Colour.Concrete;
            break;

          case AdSecMaterial.AdSecMaterialType.Steel:
            subColour = UI.Colour.Steel;
            break;
        }
        subColours.Add(subColour);
      }

      // rebars
      rebars = new List<Brep>();
      rebarColours = new List<DisplayMaterial>();
      rebarEdges = new List<Circle>();
      linkEdges = new List<Curve>();
      foreach (IGroup rebargrp in flat.ReinforcementGroups) {
        try {
          var snglBrs = (ISingleBars)rebargrp;
          var baredges = new List<Circle>();
          List<Brep> barbreps = CreateBrepsFromSingleRebar(snglBrs, offs, ref baredges, LocalPlane);
          rebars.AddRange(barbreps);
          rebarEdges.AddRange(baredges);

          string rebmat = snglBrs.BarBundle.Material.ToString();
          rebmat = rebmat.Replace("Oasys.AdSec.Materials.I", "");
          rebmat = rebmat.Replace("_Implementation", "");
          Enum.TryParse(rebmat, out AdSecMaterial.AdSecMaterialType rebarType);
          DisplayMaterial rebColour = UI.Colour.Reinforcement;
          switch (rebarType) {
            case AdSecMaterial.AdSecMaterialType.Rebar:
              rebColour = UI.Colour.Reinforcement;
              break;

            case AdSecMaterial.AdSecMaterialType.FRP:
              rebColour = UI.Colour.Reinforcement;
              break;

            case AdSecMaterial.AdSecMaterialType.Tendon:
              rebColour = UI.Colour.Reinforcement;
              break;
          }
          for (int i = 0; i < barbreps.Count; i++) {
            rebarColours.Add(rebColour);
          }
        } catch (Exception) {
          try {
            var linkGroup = (IPerimeterLinkGroup)rebargrp;
            CreateCurvesFromLinkGroup(linkGroup, ref linkEdges, LocalPlane);
          } catch (Exception) {
          }
        }
      }

      // local axis
      if (LocalPlane != null) {
        if (LocalPlane != Plane.WorldXY & LocalPlane != Plane.WorldYZ & LocalPlane != Plane.WorldZX) {
          Area area = Section.Profile.Area();
          double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
          var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
          previewXaxis = new Line(LocalPlane.Origin, LocalPlane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewYaxis = new Line(LocalPlane.Origin, LocalPlane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewZaxis = new Line(LocalPlane.Origin, LocalPlane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
        }
      }
    }

    private Brep CreateBrepFromProfile(AdSecProfileGoo profile) {
      var crvs = new List<Curve> {
        profile.Value.ToPolylineCurve()
      };
      crvs.AddRange(profile.VoidEdges.Select(x => x.ToPolylineCurve()));
      return Brep.CreatePlanarBreps(crvs, 0.001).First(); //TODO: use OasysUnits tolerance
    }

    private List<Brep> CreateBrepsFromSingleRebar(ISingleBars bars, Vector3d offset, ref List<Circle> edgeCurves, Plane local) {
      // transform to local plane
      var mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);
      //offs.Transform(mapToLocal);
      var rebarBreps = new List<Brep>();
      for (int i = 0; i < bars.Positions.Count; i++) {
        var center = new Point3d(
            0,
            bars.Positions[i].Y.As(DefaultUnits.LengthUnitGeometry),
            bars.Positions[i].Z.As(DefaultUnits.LengthUnitGeometry));
        center.Transform(Transform.Translation(offset));
        center.Transform(mapToLocal);
        var localCenter = new Plane(center, local.Normal);
        var edgeCurve = new Circle(localCenter, bars.BarBundle.Diameter.As(DefaultUnits.LengthUnitGeometry) / 2);
        edgeCurves.Add(edgeCurve);
        var crvs = new List<Curve>() { edgeCurve.ToNurbsCurve() };
        rebarBreps.Add(Brep.CreatePlanarBreps(crvs, 0.001).First()); //TODO: use OasysUnits tolerance
      }
      return rebarBreps;
    }

    private void CreateCurvesFromLinkGroup(IPerimeterLinkGroup linkGroup, ref List<Curve> linkEdges, Plane local) {
      // transform to local plane
      var mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);

      // get start point
      var startPt = new Point3d(
              0,
              linkGroup.LinkPath.StartPoint.Y.As(DefaultUnits.LengthUnitGeometry),
              linkGroup.LinkPath.StartPoint.Z.As(DefaultUnits.LengthUnitGeometry));
      startPt.Transform(mapToLocal);

      var centreline = new PolyCurve();
      foreach (IPathSegment<IPoint> path in linkGroup.LinkPath.Segments) {
        try {
          // try cast to line type
          var line = (ILineSegment<IPoint>)path;

          // get next point member and transform to local plane
          var nextPt = new Point3d(
          0,
          line.NextPoint.Y.As(DefaultUnits.LengthUnitGeometry),
          line.NextPoint.Z.As(DefaultUnits.LengthUnitGeometry));
          nextPt.Transform(mapToLocal);

          // create rhino line segments
          var ln = new Line(startPt, nextPt);

          // update starting point for next segment
          startPt = nextPt;

          // add line segment to centreline
          centreline.Append(ln);
        } catch (Exception) {
          // try cast to arc type
          var arc = (IArcSegment<IPoint>)path;

          // centrepoint
          var centrePt = new Point3d(
              0,
              arc.Centre.Y.As(DefaultUnits.LengthUnitGeometry),
              arc.Centre.Z.As(DefaultUnits.LengthUnitGeometry));
          centrePt.Transform(mapToLocal);

          // calculate radius from startPt/previousPt
          double radius = startPt.DistanceTo(centrePt);

          // create rotation transformation
          var xAxis = new Vector3d(startPt - centrePt);
          var yAxis = Vector3d.CrossProduct(local.ZAxis, xAxis);

          var arcPln = new Plane(centrePt, xAxis, yAxis);

          // get segment sweep angle
          double sweepAngle = arc.SweepAngle.As(AngleUnit.Radian);

          // create rhino arc segment
          var arcrh = new Arc(arcPln, radius, sweepAngle);

          // get next point
          startPt = arcrh.EndPoint;

          // add line segment to centreline
          centreline.Append(arcrh);
        }
      }

      // offset curves by link radius
      double barDiameter = linkGroup.BarBundle.Diameter.As(DefaultUnits.LengthUnitGeometry);
      Curve[] offset1 = centreline.Offset(local, barDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp);
      Curve[] offset2 = centreline.Offset(local, barDiameter / 2 * -1, 0.001, CurveOffsetCornerStyle.Sharp);

      if (linkEdges == null) {
        linkEdges = new List<Curve>();
      }

      linkEdges.AddRange(new List<Curve>() { offset1[0], offset2[0] });
      //linkEdges.Add(centreline);
    }

    private Tuple<Oasys.Collections.IList<IGroup>, ICover> CreateReinforcementGroupsWithMaxCover(List<AdSecRebarGroup> reinforcement) {
      var groups = Oasys.Collections.IList<IGroup>.Create();
      ICover cover = null;
      foreach (AdSecRebarGroup grp in reinforcement) {
        // add group to list of groups
        groups.Add(grp.Group);

        // check if cover of group is bigger than any previous ones
        try {
          var link = (ILinkGroup)grp.Group;
          if (grp.Cover != null) {
            if (cover == null || grp.Cover.UniformCover > cover.UniformCover) {
              cover = grp.Cover;
            }
          }
        } catch (Exception) {
          try {
            var link = (IPerimeterGroup)grp.Group;
            if (grp.Cover != null) {
              if (cover == null || grp.Cover.UniformCover > cover.UniformCover) {
                cover = grp.Cover;
              }
            }
          } catch (Exception) {
            try {
              var template = (ITemplateGroup)grp.Group;
              if (grp.Cover != null) {
                if (cover == null || grp.Cover.UniformCover > cover.UniformCover) {
                  cover = grp.Cover;
                }
              }
            } catch (Exception) {
            }
          }
          // not a link group, so we don't set section's cover
        }
      }
      return new Tuple<Oasys.Collections.IList<IGroup>, ICover>(groups, cover);
    }
  }
}
