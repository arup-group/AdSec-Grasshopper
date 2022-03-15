using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino;
using Grasshopper.Documentation;
using Rhino.Collections;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using UnitsNet;
using Oasys.Units;
using System.Drawing;
using Rhino.Display;
using Oasys.Geometry.Paths2D;
using AdSecGHAdapter;

namespace AdSecGH.Parameters
{
    /// <summary>
    /// AdSec Material class, this class defines the basic properties and methods for any AdSec Material
    /// </summary>
    public class AdSecSection
    {
        public ISection Section
        {
            get { return m_section; }
            set { m_section = value; }
        }
        public IDesignCode DesignCode
        {
            get { return m_code; }
            set { m_code = value; }
        }
        public Plane LocalPlane
        {
            get { return m_plane; }
            set { m_plane = value; }
        }
        #region fields
        private IDesignCode m_code;
        internal string codeName;
        internal string materialName;
        private ISection m_section;
        private Plane m_plane;
        internal Line previewXaxis;
        internal Line previewYaxis;
        internal Line previewZaxis;
        #endregion
        internal Brep SolidBrep => m_profile;
        internal List<Brep> SubBreps => m_subProfiles;
        #region constructors
        public AdSecSection(ISection section, IDesignCode code, string codeName, string materialName, Plane local, IPoint subComponentOffset = null)
        {
            this.materialName = materialName;
            m_section = section;
            m_code = code;
            this.codeName = codeName;
            m_plane = local;
            CreatePreview(m_code, m_section, m_plane, ref m_profile, ref m_profileEdge, ref m_profileVoidEdges, ref m_profileColour,
                ref m_rebars, ref m_rebarEdges, ref m_linkEdges, ref m_rebarColours, ref m_subProfiles, ref m_subEdges, ref m_subVoidEdges,
                ref m_subColours, subComponentOffset);
        }

        public AdSecSection(IProfile profile, Plane local, AdSecMaterial material,
            List<AdSecRebarGroup> reinforcement,
            Oasys.Collections.IList<ISubComponent> subComponents)
        {
            m_code = material.DesignCode.Duplicate().DesignCode;
            codeName = material.DesignCodeName;
            materialName = material.GradeName;
            m_section = ISection.Create(profile, material.Material);
            Tuple<Oasys.Collections.IList<IGroup>, ICover> rebarAndCover = CreateReinforcementGroupsWithMaxCover(reinforcement);
            m_section.ReinforcementGroups = rebarAndCover.Item1;
            if (rebarAndCover.Item2 != null)
                m_section.Cover = rebarAndCover.Item2;
            if (subComponents != null)
                m_section.SubComponents = subComponents;
            m_plane = local;
            CreatePreview(m_code, m_section, m_plane, ref m_profile, ref m_profileEdge, ref m_profileVoidEdges, ref m_profileColour,
                ref m_rebars, ref m_rebarEdges, ref m_linkEdges, ref m_rebarColours, ref m_subProfiles, ref m_subEdges, ref m_subVoidEdges,
                ref m_subColours);
        }

        // cache for preview
        internal Brep m_profile;
        internal DisplayMaterial m_profileColour;
        internal Polyline m_profileEdge;
        internal List<Polyline> m_profileVoidEdges;
        internal List<Brep> m_rebars;
        internal List<Circle> m_rebarEdges;
        internal List<Curve> m_linkEdges;
        internal List<DisplayMaterial> m_rebarColours;
        internal List<Brep> m_subProfiles;
        internal List<Polyline> m_subEdges;
        internal List<List<Polyline>> m_subVoidEdges;
        internal List<DisplayMaterial> m_subColours;

        internal void CreatePreview(IDesignCode code, ISection section, Plane local,
            ref Brep profile, ref Polyline profileEdge, ref List<Polyline> profileVoidEdges, ref DisplayMaterial profileColour,
            ref List<Brep> rebars, ref List<Circle> rebarEdges, ref List<Curve> linkEdges, ref List<DisplayMaterial> rebarColours,
            ref List<Brep> subProfiles, ref List<Polyline> subEdges, ref List<List<Polyline>> subVoidEdges, ref List<DisplayMaterial> subColours,
            IPoint offset = null)
        {
            ISection flat = null;
            if (code != null) //{ code = Oasys.AdSec.DesignCode.EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex; }
            {
                IAdSec adSec = IAdSec.Create(code);
                flat = adSec.Flatten(section);
            }
            else
            {
                IPerimeterProfile prof = IPerimeterProfile.Create(section.Profile);
                flat = ISection.Create(prof, section.Material);
                if (section.ReinforcementGroups.Count > 0)
                    flat.ReinforcementGroups = section.ReinforcementGroups;
                if (section.SubComponents.Count > 0)
                    flat.SubComponents = section.SubComponents;
            }

            // create offset if any
            Vector3d offs = Vector3d.Zero;

            if (offset != null)
            {
                offs = new Vector3d(0,
                    offset.Y.As(Units.LengthUnit),
                    offset.Z.As(Units.LengthUnit));
            }


            // primary profile
            profile = CreateBrepFromProfile(new AdSecProfileGoo(flat.Profile, local));
            profile.Transform(Transform.Translation(offs));
            Tuple<Polyline, List<Polyline>> edges = AdSecProfileGoo.PolylinesFromAdSecProfile(flat.Profile, local);
            profileEdge = edges.Item1;
            profileEdge.Transform(Transform.Translation(offs));
            profileVoidEdges = edges.Item2;

            // get material
            AdSecMaterial.AdSecMaterialType profileType;
            string mat = section.Material.ToString();
            mat = mat.Replace("Oasys.AdSec.Materials.I", "");
            mat = mat.Replace("_Implementation", "");
            Enum.TryParse(mat, out profileType);
            switch (profileType)
            {
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
            foreach (ISubComponent sub in flat.SubComponents)
            {
                Brep temp = CreateBrepFromProfile(new AdSecProfileGoo(sub.Section.Profile, local));
                Vector3d trans = new Vector3d(
                    0,
                    sub.Offset.Y.As(Units.LengthUnit),
                    sub.Offset.Z.As(Units.LengthUnit));
                temp.Transform(Transform.Translation(trans));
                temp.Transform(Transform.Translation(offs));
                subProfiles.Add(temp);

                Tuple<Polyline, List<Polyline>> subedges = AdSecProfileGoo.PolylinesFromAdSecProfile(sub.Section.Profile, local);
                Polyline subedge = subedges.Item1;
                subedge.Transform(Transform.Translation(trans));
                subedge.Transform(Transform.Translation(offs));
                subEdges.Add(subedge);

                List<Polyline> subvoids = subedges.Item2;
                foreach (Polyline crv in subvoids)
                {
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
                switch (subType)
                {
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
            foreach (IGroup rebargrp in flat.ReinforcementGroups)
            {
                try
                {
                    ISingleBars snglBrs = (ISingleBars)rebargrp;
                    List<Circle> baredges = new List<Circle>();
                    List<Brep> barbreps = CreateBrepsFromSingleRebar(snglBrs, offs, ref baredges, local);
                    rebars.AddRange(barbreps);
                    rebarEdges.AddRange(baredges);

                    string rebmat = snglBrs.BarBundle.Material.ToString();
                    rebmat = rebmat.Replace("Oasys.AdSec.Materials.I", "");
                    rebmat = rebmat.Replace("_Implementation", "");
                    AdSecMaterial.AdSecMaterialType rebarType;
                    Enum.TryParse(rebmat, out rebarType);
                    DisplayMaterial rebColour = UI.Colour.Reinforcement;
                    switch (rebarType)
                    {
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
                catch (Exception)
                {
                    try
                    {
                        IPerimeterLinkGroup linkGroup = (IPerimeterLinkGroup)rebargrp;
                        CreateCurvesFromLinkGroup(linkGroup, ref linkEdges, local);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            // local axis
            if (local != null)
            {
                if (local != Plane.WorldXY & local != Plane.WorldYZ & local != Plane.WorldZX)
                {
                    Area area = this.m_section.Profile.Area();
                    double pythogoras = Math.Sqrt(area.As(UnitsNet.Units.AreaUnit.SquareMeter));
                    Length length = new Length(pythogoras * 0.15, UnitsNet.Units.LengthUnit.Meter);
                    previewXaxis = new Line(local.Origin, local.XAxis, length.As(Units.LengthUnit));
                    previewYaxis = new Line(local.Origin, local.YAxis, length.As(Units.LengthUnit));
                    previewZaxis = new Line(local.Origin, local.ZAxis, length.As(Units.LengthUnit));
                }
            }
        }

        private Brep CreateBrepFromProfile(AdSecProfileGoo profile)
        {
            List<Curve> crvs = new List<Curve>();
            crvs.Add(profile.Value.ToPolylineCurve());
            crvs.AddRange(profile.VoidEdges.Select(x => x.ToPolylineCurve()));
            return Brep.CreatePlanarBreps(crvs, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).First();
        }
        private void CreateCurvesFromLinkGroup(IPerimeterLinkGroup linkGroup, ref List<Curve> linkEdges, Plane local)
        {
            // transform to local plane
            Transform mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);

            // get start point
            Point3d startPt = new Point3d(
                    0,
                    linkGroup.LinkPath.StartPoint.Y.As(Units.LengthUnit),
                    linkGroup.LinkPath.StartPoint.Z.As(Units.LengthUnit));
            startPt.Transform(mapToLocal);

            PolyCurve centreline = new PolyCurve();
            foreach (IPathSegment<IPoint> path in linkGroup.LinkPath.Segments)
            {
                try
                {
                    // try cast to line type
                    ILineSegment<IPoint> line = (ILineSegment<IPoint>)path;

                    // get next point member and transform to local plane
                    Point3d nextPt = new Point3d(
                    0,
                    line.NextPoint.Y.As(Units.LengthUnit),
                    line.NextPoint.Z.As(Units.LengthUnit));
                    nextPt.Transform(mapToLocal);

                    // create rhino line segments
                    Line ln = new Line(startPt, nextPt);

                    // update starting point for next segment
                    startPt = nextPt;

                    // add line segment to centreline
                    centreline.Append(ln);
                }
                catch (Exception)
                {
                    // try cast to arc type
                    IArcSegment<IPoint> arc = (IArcSegment<IPoint>)path;

                    // centrepoint
                    Point3d centrePt = new Point3d(
                        0,
                        arc.Centre.Y.As(Units.LengthUnit),
                        arc.Centre.Z.As(Units.LengthUnit));
                    centrePt.Transform(mapToLocal);

                    // calculate radius from startPt/previousPt
                    double radius = startPt.DistanceTo(centrePt);

                    // create rotation transformation
                    Vector3d xAxis = new Vector3d(startPt - centrePt);
                    Vector3d yAxis = Vector3d.CrossProduct(local.ZAxis, xAxis);

                    Plane arcPln = new Plane(centrePt, xAxis, yAxis);

                    // get segment sweep angle
                    double sweepAngle = arc.SweepAngle.As(UnitsNet.Units.AngleUnit.Radian);

                    // create rhino arc segment
                    Arc arcrh = new Arc(arcPln, radius, sweepAngle);

                    // get next point
                    startPt = arcrh.EndPoint;

                    // add line segment to centreline
                    centreline.Append(arcrh);
                }
            }

            // offset curves by link radius
            double barDiameter = linkGroup.BarBundle.Diameter.As(Units.LengthUnit);
            Curve[] offset1 = centreline.Offset(local, barDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp);
            Curve[] offset2 = centreline.Offset(local, barDiameter / 2 * -1, 0.001, CurveOffsetCornerStyle.Sharp);

            if (linkEdges == null)
                linkEdges = new List<Curve>();

            linkEdges.AddRange(new List<Curve>() { offset1[0], offset2[0] });
            //linkEdges.Add(centreline);
        }

        private List<Brep> CreateBrepsFromSingleRebar(ISingleBars bars, Vector3d offset, ref List<Circle> edgeCurves, Plane local)
        {
            // transform to local plane
            Transform mapToLocal = Transform.PlaneToPlane(Plane.WorldYZ, local);
            //offs.Transform(mapToLocal);
            List<Brep> rebarBreps = new List<Brep>();
            for (int i = 0; i < bars.Positions.Count; i++)
            {
                Point3d center = new Point3d(
                    0,
                    bars.Positions[i].Y.As(Units.LengthUnit),
                    bars.Positions[i].Z.As(Units.LengthUnit));
                center.Transform(Transform.Translation(offset));
                center.Transform(mapToLocal);
                Plane localCenter = new Plane(center, local.Normal);
                Circle edgeCurve = new Circle(localCenter, bars.BarBundle.Diameter.As(Units.LengthUnit) / 2);
                edgeCurves.Add(edgeCurve);
                List<Curve> crvs = new List<Curve>() { edgeCurve.ToNurbsCurve() };
                rebarBreps.Add(Brep.CreatePlanarBreps(crvs, RhinoDoc.ActiveDoc.ModelRelativeTolerance).First());
            }
            return rebarBreps;
        }

        private Tuple<Oasys.Collections.IList<IGroup>, ICover> CreateReinforcementGroupsWithMaxCover(List<AdSecRebarGroup> reinforcement)
        {
            Oasys.Collections.IList<IGroup> groups = Oasys.Collections.IList<IGroup>.Create();
            ICover cover = null;
            foreach (AdSecRebarGroup grp in reinforcement)
            {
                // add group to list of groups
                groups.Add(grp.Group);

                // check if cover of group is bigger than any previous ones
                try
                {
                    ILinkGroup link = (ILinkGroup)grp.Group;
                    if (grp.Cover != null)
                    {
                        if (cover == null || grp.Cover.UniformCover > cover.UniformCover)
                        {
                            cover = grp.Cover;
                        }
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        IPerimeterGroup link = (IPerimeterGroup)grp.Group;
                        if (grp.Cover != null)
                        {
                            if (cover == null || grp.Cover.UniformCover > cover.UniformCover)
                            {
                                cover = grp.Cover;
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                    // not a link group, so we don't set section's cover
                }
            }
            return new Tuple<Oasys.Collections.IList<IGroup>, ICover>(groups, cover);
        }

        public AdSecSection Duplicate()
        {
            if (this == null) { return null; }
            AdSecSection dup = (AdSecSection)this.MemberwiseClone();
            return dup;
        }
        #endregion

        #region properties
        public bool IsValid
        {
            get
            {
                if (this.Section == null) { return false; }
                return true;
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return Section.Profile.Description();
        }

        #endregion
    }

    /// <summary>
    /// Geometry Goo wrapper class, makes sure class can be used and previewed in Grasshopper.
    /// </summary>
    public class AdSecSectionGoo : GH_GeometricGoo<AdSecSection>, IGH_PreviewData
    {
        #region constructors
        public AdSecSectionGoo()
        {
            this.Value = null;
        }
        public AdSecSectionGoo(AdSecSection section)
        {
            if (section == null)
                section = null;
            else
                this.Value = section.Duplicate();
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return DuplicateAdSecSection();
        }
        public AdSecSectionGoo DuplicateAdSecSection()
        {
            if (Value == null)
                return null;
            else
                return new AdSecSectionGoo(Value.Duplicate());
        }
        #endregion

        #region properties
        public override bool IsValid
        {
            get
            {
                if (Value == null) { return false; }
                if (Value.SolidBrep == null || Value.IsValid == false) { return false; }
                return true;
            }
        }
        public override string IsValidWhyNot
        {
            get
            {
                if (Value.IsValid) { return string.Empty; }
                return Value.IsValid.ToString();
            }
        }
        public override string ToString()
        {
            if (Value == null)
                return "Null AdSec Section";
            else
                return "AdSec " + TypeName + " {" + Value.ToString() + "}";
        }
        public override string TypeName => "Section";
        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                if (Value.SolidBrep == null) { return BoundingBox.Empty; }
                return Value.SolidBrep.GetBoundingBox(false);
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null) { return BoundingBox.Empty; }
            if (Value.SolidBrep == null) { return BoundingBox.Empty; }
            return Value.SolidBrep.GetBoundingBox(xform);
        }
        #endregion

        #region casting methods
        public override bool CastTo<Q>(out Q target)
        {
            // This function is called when Grasshopper needs to convert this 
            // AdSec type into some other type Q.            
            if (InteropAdSecComputeTypes.IsPresent())
            {
                Type type = InteropAdSecComputeTypes.GetType(typeof(IAdSecSection));
                if (typeof(Q).IsAssignableFrom(type))
                {
                    if (Value == null)
                        target = default;
                    else
                    {
                        target = (Q)(object)InteropAdSecComputeTypes.CastToSection(Value.Section, Value.codeName, Value.materialName);
                    }
                    return true;
                }
            }
            if (typeof(Q).IsAssignableFrom(typeof(AdSecSectionGoo)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value.Duplicate();
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(AdSecSection)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)new AdSecSection(Value.Section, Value.DesignCode, Value.codeName, Value.materialName, Value.LocalPlane);
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(AdSecProfileGoo)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)new AdSecProfileGoo(Value.Section.Profile, Value.LocalPlane);
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(Brep)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value.SolidBrep.DuplicateBrep();
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_Brep)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value.SolidBrep.DuplicateBrep();
                return true;
            }



            target = default;
            return false;
        }
        public override bool CastFrom(object source)
        {
            // This function is called when Grasshopper needs to convert other data 
            // into this AdSec type.

            if (source == null) { return false; }

            return false;
        }
        #endregion

        #region transformation methods
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            //return new AdSecSectionGoo(Value.Transform(xform));
            return null;
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            //return new AdSecSectionGoo(Value.Morph(xmorph));
            return null;
        }

        #endregion

        #region drawing methods
        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            //Draw shape.
            if (Value.SolidBrep != null)
            {
                // draw profile
                args.Pipeline.DrawBrepShaded(Value.SolidBrep, Value.m_profileColour);
                // draw subcomponents
                for (int i = 0; i < Value.m_subProfiles.Count; i++)
                {
                    args.Pipeline.DrawBrepShaded(Value.m_subProfiles[i], Value.m_subColours[i]);
                }
                // draw rebars
                for (int i = 0; i < Value.m_rebars.Count; i++)
                {
                    args.Pipeline.DrawBrepShaded(Value.m_rebars[i], Value.m_rebarColours[i]);
                }
            }
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null) { return; }

            Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
            if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
            {
                args.Pipeline.DrawPolyline(Value.m_profileEdge, UI.Colour.OasysBlue, 2);
                if (Value.m_profileVoidEdges != null)
                {
                    foreach (Polyline crv in Value.m_profileVoidEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
                    }
                }
                if (Value.m_subEdges != null)
                {
                    foreach (Polyline crv in Value.m_subEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
                    }
                }
                if (Value.m_subVoidEdges != null)
                {
                    foreach (List<Polyline> crvs in Value.m_subVoidEdges)
                    {
                        foreach (Polyline crv in crvs)
                        {
                            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
                        }
                    }
                }
                if (Value.m_rebarEdges != null)
                {
                    foreach (Circle crv in Value.m_rebarEdges)
                    {
                        args.Pipeline.DrawCircle(crv, Color.Black, 1);
                    }
                }
                if (Value.m_linkEdges != null)
                {
                    foreach (Curve crv in Value.m_linkEdges)
                    {
                        args.Pipeline.DrawCurve(crv, Color.Black, 1);
                    }
                }
            }
            else // selected
            {
                args.Pipeline.DrawPolyline(Value.m_profileEdge, UI.Colour.OasysYellow, 3);
                if (Value.m_profileVoidEdges != null)
                {
                    foreach (Polyline crv in Value.m_profileVoidEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
                    }
                }
                if (Value.m_subEdges != null)
                {
                    foreach (Polyline crv in Value.m_subEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
                    }
                }
                if (Value.m_subVoidEdges != null)
                {
                    foreach (List<Polyline> crvs in Value.m_subVoidEdges)
                    {
                        foreach (Polyline crv in crvs)
                        {
                            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
                        }
                    }
                }
                if (Value.m_rebarEdges != null)
                {
                    foreach (Circle crv in Value.m_rebarEdges)
                    {
                        args.Pipeline.DrawCircle(crv, UI.Colour.UILightGrey, 2);
                    }
                }
                if (Value.m_linkEdges != null)
                {
                    foreach (Curve crv in Value.m_linkEdges)
                    {
                        args.Pipeline.DrawCurve(crv, UI.Colour.UILightGrey, 2);
                    }
                }
            }
            // local axis
            if (Value.previewXaxis != null)
            {
                args.Pipeline.DrawLine(Value.previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
                args.Pipeline.DrawLine(Value.previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
                args.Pipeline.DrawLine(Value.previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
            }
        }
        #endregion
    }

    /// <summary>
    /// This class provides a Parameter interface for the Data_GsaMember2d type.
    /// </summary>
    public class AdSecSectionParameter : GH_PersistentGeometryParam<AdSecSectionGoo>, IGH_PreviewObject
    {
        public AdSecSectionParameter()
          : base(new GH_InstanceDescription("Section", "Sec", "Maintains a collection of AdSec Section data.", Components.Ribbon.CategoryName.Name(), Components.Ribbon.SubCategoryName.Cat9()))
        {
        }

        public override Guid ComponentGuid => new Guid("fa647c2d-4767-49f1-a574-32bf66a66568");

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override Bitmap Icon => Properties.Resources.SectionParam;

        //We do not allow users to pick parameter, 
        //therefore the following 4 methods disable all this ui.
        protected override GH_GetterResult Prompt_Plural(ref List<AdSecSectionGoo> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref AdSecSectionGoo value)
        {
            return GH_GetterResult.cancel;
        }
        protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomSingleValueItem()
        {
            System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }
        protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomMultiValueItem()
        {
            System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }

        #region preview methods
        public BoundingBox ClippingBox
        {
            get
            {
                return Preview_ComputeClippingBox();
            }
        }
        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //Use a standard method to draw gunk, you don't have to specifically implement this.
            Preview_DrawMeshes(args);
        }
        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            //Use a standard method to draw gunk, you don't have to specifically implement this.
            Preview_DrawWires(args);
        }

        private bool m_hidden = false;
        public bool Hidden
        {
            get { return m_hidden; }
            set { m_hidden = value; }
        }
        public bool IsPreviewCapable
        {
            get { return true; }
        }
        #endregion
    }
}
