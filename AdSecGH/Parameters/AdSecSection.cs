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
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdSecGH.Parameters {
  public class AdSecSection {
    public IDesignCode DesignCode { get; set; }
    public bool IsValid {
      get {
        if (this.Section == null)
          return false;
        return true;
      }
    }
    public Plane LocalPlane { get; set; }
    public ISection Section { get; set; }
    internal Brep SolidBrep => m_profile;
    internal List<Brep> SubBreps => m_subProfiles;
    internal string codeName;
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
    internal List<Brep> m_subProfiles;
    internal List<List<Polyline>> m_subVoidEdges;
    internal string materialName;
    internal Line previewXaxis;
    internal Line previewYaxis;
    internal Line previewZaxis;

    public AdSecSection(ISection section, IDesignCode code, string codeName, string materialName, Plane local, IPoint subComponentOffset = null) {
      this.materialName = materialName;
      this.Section = section;
      this.DesignCode = code;
      this.codeName = codeName;
      this.LocalPlane = local;
      this.CreatePreview(ref m_profile, ref m_profileEdge, ref m_profileVoidEdges, ref m_profileColour, ref m_rebars, ref m_rebarEdges, ref m_linkEdges, ref m_rebarColours, ref m_subProfiles, ref m_subEdges, ref m_subVoidEdges, ref m_subColours, subComponentOffset);
    }

    public AdSecSection(IProfile profile, Plane local, AdSecMaterial material, List<AdSecRebarGroup> reinforcement, Oasys.Collections.IList<ISubComponent> subComponents) {
      this.DesignCode = material.DesignCode.Duplicate().DesignCode;
      codeName = material.DesignCodeName;
      materialName = material.GradeName;
      this.Section = ISection.Create(profile, material.Material);
      Tuple<Oasys.Collections.IList<IGroup>, ICover> rebarAndCover = this.CreateReinforcementGroupsWithMaxCover(reinforcement);
      this.Section.ReinforcementGroups = rebarAndCover.Item1;
      if (rebarAndCover.Item2 != null)
        this.Section.Cover = rebarAndCover.Item2;
      if (subComponents != null)
        this.Section.SubComponents = subComponents;
      this.LocalPlane = local;
      CreatePreview(ref m_profile, ref m_profileEdge, ref m_profileVoidEdges, ref m_profileColour, ref m_rebars, ref m_rebarEdges, ref m_linkEdges, ref m_rebarColours, ref m_subProfiles, ref m_subEdges, ref m_subVoidEdges, ref m_subColours);
    }

    public AdSecSection Duplicate() {
      if (this == null)
        return null;
      AdSecSection dup = (AdSecSection)this.MemberwiseClone();
      return dup;
    }

    public override string ToString() {
      return Section.Profile.Description();
    }

    internal void CreatePreview(ref Brep profile, ref Polyline profileEdge, ref List<Polyline> profileVoidEdges, ref DisplayMaterial profileColour, ref List<Brep> rebars, ref List<Circle> rebarEdges, ref List<Curve> linkEdges, ref List<DisplayMaterial> rebarColours, ref List<Brep> subProfiles, ref List<Polyline> subEdges, ref List<List<Polyline>> subVoidEdges, ref List<DisplayMaterial> subColours, IPoint offset = null) {
      ISection flat = null;
      if (this.DesignCode != null) //{ code = Oasys.AdSec.DesignCode.EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex; }
      {
        IAdSec adSec = IAdSec.Create(this.DesignCode);
        flat = adSec.Flatten(this.Section);
      }
      else {
        IPerimeterProfile prof = IPerimeterProfile.Create(this.Section.Profile);
        flat = ISection.Create(prof, this.Section.Material);
        if (this.Section.ReinforcementGroups.Count > 0)
          flat.ReinforcementGroups = this.Section.ReinforcementGroups;
        if (this.Section.SubComponents.Count > 0)
          flat.SubComponents = this.Section.SubComponents;
      }

      // create offset if any
      Vector3d offs = Vector3d.Zero;

      if (offset != null) {
        offs = new Vector3d(0,
            offset.Y.As(DefaultUnits.LengthUnitGeometry),
            offset.Z.As(DefaultUnits.LengthUnitGeometry));
      }

      // primary profile
      profile = CreateBrepFromProfile(new AdSecProfileGoo(flat.Profile, this.LocalPlane));
      profile.Transform(Transform.Translation(offs));
      Tuple<Polyline, List<Polyline>> edges = AdSecProfileGoo.PolylinesFromAdSecProfile(flat.Profile, this.LocalPlane);
      profileEdge = edges.Item1;
      profileEdge.Transform(Transform.Translation(offs));
      profileVoidEdges = edges.Item2;

      // get material
      AdSecMaterial.AdSecMaterialType profileType;
      string mat = this.Section.Material.ToString();
      mat = mat.Replace("Oasys.AdSec.Materials.I", "");
      mat = mat.Replace("_Implementation", "");
      Enum.TryParse(mat, out profileType);
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
        Brep temp = CreateBrepFromProfile(new AdSecProfileGoo(sub.Section.Profile, this.LocalPlane));
        Vector3d trans = new Vector3d(
            0,
            sub.Offset.Y.As(DefaultUnits.LengthUnitGeometry),
            sub.Offset.Z.As(DefaultUnits.LengthUnitGeometry));
        temp.Transform(Transform.Translation(trans));
        temp.Transform(Transform.Translation(offs));
        subProfiles.Add(temp);

        Tuple<Polyline, List<Polyline>> subedges = AdSecProfileGoo.PolylinesFromAdSecProfile(sub.Section.Profile, this.LocalPlane);
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
        AdSecMaterial.AdSecMaterialType subType;
        Enum.TryParse(submat, out subType);
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
          ISingleBars snglBrs = (ISingleBars)rebargrp;
          List<Circle> baredges = new List<Circle>();
          List<Brep> barbreps = CreateBrepsFromSingleRebar(snglBrs, offs, ref baredges, this.LocalPlane);
          rebars.AddRange(barbreps);
          rebarEdges.AddRange(baredges);

          string rebmat = snglBrs.BarBundle.Material.ToString();
          rebmat = rebmat.Replace("Oasys.AdSec.Materials.I", "");
          rebmat = rebmat.Replace("_Implementation", "");
          AdSecMaterial.AdSecMaterialType rebarType;
          Enum.TryParse(rebmat, out rebarType);
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
          for (int i = 0; i < barbreps.Count; i++)
            rebarColours.Add(rebColour);
        }
        catch (Exception) {
          try {
            IPerimeterLinkGroup linkGroup = (IPerimeterLinkGroup)rebargrp;
            CreateCurvesFromLinkGroup(linkGroup, ref linkEdges, this.LocalPlane);
          }
          catch (Exception) {
          }
        }
      }

      // local axis
      if (this.LocalPlane != null) {
        if (this.LocalPlane != Plane.WorldXY & this.LocalPlane != Plane.WorldYZ & this.LocalPlane != Plane.WorldZX) {
          Area area = this.Section.Profile.Area();
          double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
          Length length = new Length(pythogoras * 0.15, LengthUnit.Meter);
          previewXaxis = new Line(this.LocalPlane.Origin, this.LocalPlane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewYaxis = new Line(this.LocalPlane.Origin, this.LocalPlane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewZaxis = new Line(this.LocalPlane.Origin, this.LocalPlane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
        }
      }
    }

    private Brep CreateBrepFromProfile(AdSecProfileGoo profile) {
      List<Curve> crvs = new List<Curve>();
      crvs.Add(profile.Value.ToPolylineCurve());
      crvs.AddRange(profile.VoidEdges.Select(x => x.ToPolylineCurve()));
      return Brep.CreatePlanarBreps(crvs, 0.001).First(); //TODO: use OasysUnits tolerance
    }

    private List<Brep> CreateBrepsFromSingleRebar(ISingleBars bars, Vector3d offset, ref List<Circle> edgeCurves, Plane local) {
      // transform to local plane
      Transform mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);
      //offs.Transform(mapToLocal);
      List<Brep> rebarBreps = new List<Brep>();
      for (int i = 0; i < bars.Positions.Count; i++) {
        Point3d center = new Point3d(
            0,
            bars.Positions[i].Y.As(DefaultUnits.LengthUnitGeometry),
            bars.Positions[i].Z.As(DefaultUnits.LengthUnitGeometry));
        center.Transform(Transform.Translation(offset));
        center.Transform(mapToLocal);
        Plane localCenter = new Plane(center, local.Normal);
        Circle edgeCurve = new Circle(localCenter, bars.BarBundle.Diameter.As(DefaultUnits.LengthUnitGeometry) / 2);
        edgeCurves.Add(edgeCurve);
        List<Curve> crvs = new List<Curve>() { edgeCurve.ToNurbsCurve() };
        rebarBreps.Add(Brep.CreatePlanarBreps(crvs, 0.001).First()); //TODO: use OasysUnits tolerance
      }
      return rebarBreps;
    }

    private void CreateCurvesFromLinkGroup(IPerimeterLinkGroup linkGroup, ref List<Curve> linkEdges, Plane local) {
      // transform to local plane
      Transform mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);

      // get start point
      Point3d startPt = new Point3d(
              0,
              linkGroup.LinkPath.StartPoint.Y.As(DefaultUnits.LengthUnitGeometry),
              linkGroup.LinkPath.StartPoint.Z.As(DefaultUnits.LengthUnitGeometry));
      startPt.Transform(mapToLocal);

      PolyCurve centreline = new PolyCurve();
      foreach (IPathSegment<IPoint> path in linkGroup.LinkPath.Segments) {
        try {
          // try cast to line type
          ILineSegment<IPoint> line = (ILineSegment<IPoint>)path;

          // get next point member and transform to local plane
          Point3d nextPt = new Point3d(
          0,
          line.NextPoint.Y.As(DefaultUnits.LengthUnitGeometry),
          line.NextPoint.Z.As(DefaultUnits.LengthUnitGeometry));
          nextPt.Transform(mapToLocal);

          // create rhino line segments
          Line ln = new Line(startPt, nextPt);

          // update starting point for next segment
          startPt = nextPt;

          // add line segment to centreline
          centreline.Append(ln);
        }
        catch (Exception) {
          // try cast to arc type
          IArcSegment<IPoint> arc = (IArcSegment<IPoint>)path;

          // centrepoint
          Point3d centrePt = new Point3d(
              0,
              arc.Centre.Y.As(DefaultUnits.LengthUnitGeometry),
              arc.Centre.Z.As(DefaultUnits.LengthUnitGeometry));
          centrePt.Transform(mapToLocal);

          // calculate radius from startPt/previousPt
          double radius = startPt.DistanceTo(centrePt);

          // create rotation transformation
          Vector3d xAxis = new Vector3d(startPt - centrePt);
          Vector3d yAxis = Vector3d.CrossProduct(local.ZAxis, xAxis);

          Plane arcPln = new Plane(centrePt, xAxis, yAxis);

          // get segment sweep angle
          double sweepAngle = arc.SweepAngle.As(AngleUnit.Radian);

          // create rhino arc segment
          Arc arcrh = new Arc(arcPln, radius, sweepAngle);

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

      if (linkEdges == null)
        linkEdges = new List<Curve>();

      linkEdges.AddRange(new List<Curve>() { offset1[0], offset2[0] });
      //linkEdges.Add(centreline);
    }

    private Tuple<Oasys.Collections.IList<IGroup>, ICover> CreateReinforcementGroupsWithMaxCover(List<AdSecRebarGroup> reinforcement) {
      Oasys.Collections.IList<IGroup> groups = Oasys.Collections.IList<IGroup>.Create();
      ICover cover = null;
      foreach (AdSecRebarGroup grp in reinforcement) {
        // add group to list of groups
        groups.Add(grp.Group);

        // check if cover of group is bigger than any previous ones
        try {
          ILinkGroup link = (ILinkGroup)grp.Group;
          if (grp.Cover != null) {
            if (cover == null || grp.Cover.UniformCover > cover.UniformCover) {
              cover = grp.Cover;
            }
          }
        }
        catch (Exception) {
          try {
            IPerimeterGroup link = (IPerimeterGroup)grp.Group;
            if (grp.Cover != null) {
              if (cover == null || grp.Cover.UniformCover > cover.UniformCover) {
                cover = grp.Cover;
              }
            }
          }
          catch (Exception) {
            try {
              ITemplateGroup template = (ITemplateGroup)grp.Group;
              if (grp.Cover != null) {
                if (cover == null || grp.Cover.UniformCover > cover.UniformCover) {
                  cover = grp.Cover;
                }
              }
            }
            catch (Exception) {
            }
          }
          // not a link group, so we don't set section's cover
        }
      }
      return new Tuple<Oasys.Collections.IList<IGroup>, ICover>(groups, cover);
    }
  }
}
