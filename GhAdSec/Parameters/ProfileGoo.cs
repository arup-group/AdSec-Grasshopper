﻿using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Display;
using Oasys.Profiles;
using UnitsNet.Units;


namespace GhAdSec.Parameters
{
    public class AdSecProfileGoo : GH_GeometricGoo<Polyline>, IGH_PreviewData
    {
        public AdSecProfileGoo(Polyline polygon, LengthUnit lengthUnit)
        : base(polygon)
        {
            // Create from polygon
            IPerimeterProfile perimprofile = IPerimeterProfile.Create();

            perimprofile.SolidPolygon = PolygonFromRhinoPolyline(polygon, lengthUnit);
            // create Profile
            m_profile = perimprofile;
            m_voidEdges = null;
        }
        public AdSecProfileGoo(Polyline solid, List<Polyline> voids, LengthUnit lengthUnit)
        {
            // Create from polygon
            IPerimeterProfile perimprofile = IPerimeterProfile.Create();

            perimprofile.SolidPolygon = PolygonFromRhinoPolyline(solid, lengthUnit);

            Oasys.Collections.IList<IPolygon> adsecVoids = Oasys.Collections.IList<IPolygon>.Create();
            foreach (Polyline vdCrv in voids)
            {
                adsecVoids.Add(PolygonFromRhinoPolyline(vdCrv, lengthUnit));
            }
            perimprofile.VoidPolygons = adsecVoids;

            // create Profile
            m_profile = (IProfile)perimprofile;
            m_voidEdges = voids;
        }
        public AdSecProfileGoo(IProfile profile)
        {
            m_profile = profile;
            Tuple<Polyline, List<Polyline>> edges = PolylinesFromAdSecProfile(profile);
            m_value = edges.Item1;
            m_voidEdges = edges.Item2;
        }

        public AdSecProfileGoo(Brep brep, LengthUnit lengthUnit)
        {
            IPerimeterProfile profile = IPerimeterProfile.Create();
            // get edge curves from Brep
            Curve[] edgeSegments = brep.DuplicateEdgeCurves();
            Curve[] edges = Curve.JoinCurves(edgeSegments);

            // find the best fit plane
            List<Point3d> ctrl_pts = new List<Point3d>();
            if (edges[0].TryGetPolyline(out Polyline tempCrv))
                ctrl_pts = tempCrv.ToList();
            else
            {
                throw new Exception("Cannot convert edge to Polyline");
            }
            Plane.FitPlaneToPoints(ctrl_pts, out Plane plane);
            Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.ChangeBasis(Plane.WorldXY, plane);

            List<Point3d> solidpts = new List<Point3d>();
            foreach (Point3d pt3d in ctrl_pts)
            {
                pt3d.Transform(xform);
                solidpts.Add(pt3d);
            }
            Polyline solid = new Polyline(solidpts);
            profile.SolidPolygon = PolygonFromRhinoPolyline(solid, lengthUnit);

            if (edges.Length > 1)
            {
                List<Polyline> voidEdges = new List<Polyline>();
                Oasys.Collections.IList<IPolygon> voids = Oasys.Collections.IList<IPolygon>.Create();
                for (int i = 1; i < edges.Length; i++)
                {
                    ctrl_pts.Clear();
                    List<Point3d> voidpts = new List<Point3d>();
                    if (!edges[i].IsPlanar())
                    {
                        for (int j = 0; j < edges.Length; j++)
                            edges[j] = Curve.ProjectToPlane(edges[j], plane);
                    }
                    if (edges[i].TryGetPolyline(out tempCrv))
                    {
                        ctrl_pts = tempCrv.ToList();
                        
                        foreach (Point3d pt3d in ctrl_pts)
                        {
                            pt3d.Transform(xform);
                            voidpts.Add(pt3d);
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot convert internal edge  to Polyline");
                    }
                    Polyline voidCrv = new Polyline(voidpts);
                    voids.Add(PolygonFromRhinoPolyline(voidCrv, lengthUnit));
                    voidEdges.Add(voidCrv);
                }
                profile.VoidPolygons = voids;
            }

            m_value = solid;
            m_profile = (IProfile)profile;
        }

        private IProfile m_profile;
        private List<Polyline> m_voidEdges;

        public IProfile Profile
        {
            get { return m_profile; }
        }

        public bool IsReflectedY
        {
            get { return m_profile.IsReflectedY; }
            set
            {
                m_profile = this.Clone();
                m_profile.IsReflectedY = value;
            }
        }
        public bool IsReflectedZ
        {
            get { return m_profile.IsReflectedZ; }
            set
            {
                m_profile = this.Clone();
                m_profile.IsReflectedZ = value;
            }
        }
        public UnitsNet.Angle Rotation
        {
            get { return m_profile.Rotation; }
            set
            {
                m_profile = this.Clone();
                m_profile.Rotation = value;
            }
        }

        internal static Oasys.Collections.IList<IPoint> PtsFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit)
        {
            if (polyline == null) { return null; }

            Oasys.Collections.IList<IPoint> pts = Oasys.Collections.IList<IPoint>.Create();
            IPoint pt = null;

            for (int i = 0; i < polyline.Count; i++)
            {
                Point3d point3d = polyline[i];
                pt = IPoint.Create(
                    new UnitsNet.Length(point3d.X, lengthUnit),
                    new UnitsNet.Length(point3d.Y, lengthUnit));
                pts.Add(pt);
            }

            return pts;
        }

        internal static IPolygon PolygonFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit)
        {
            IPolygon polygon = IPolygon.Create();
            polygon.Points = PtsFromRhinoPolyline(polyline, lengthUnit);
            return polygon;
        }

        internal static List<Point3d> PtsFromAdSecPolygon(IPolygon polygon)
        {
            if (polygon == null) { return null; }

            Oasys.Collections.IList<IPoint> apts = polygon.Points;
            List<Point3d> rhPts = new List<Point3d>();

            foreach (IPoint apt in apts)
            {
                Point3d pt = new Point3d(
                    apt.Y.As(GhAdSec.DocumentUnits.LengthUnit), 
                    apt.Z.As(GhAdSec.DocumentUnits.LengthUnit), 
                    0);
                rhPts.Add(pt);
            }
            
            // add first point to end of list for closed polyline
            rhPts.Add(rhPts[0]);

            return rhPts;
        }

        internal static Tuple<List<Point3d>, List<List<Point3d>>> PtsFromAdSecPermiter(IPerimeterProfile perimeterProfile)
        {
            if (perimeterProfile == null) { return null; }
            
            IPolygon solid = perimeterProfile.SolidPolygon;
            List<Point3d> rhEdgePts = PtsFromAdSecPolygon(solid);

            Oasys.Collections.IList<IPolygon> voids = perimeterProfile.VoidPolygons;
            List<List<Point3d>> rhVoidPts = new List<List<Point3d>>();
            foreach (IPolygon vpol in voids)
            {
                rhVoidPts.Add(PtsFromAdSecPolygon(vpol));
            }

            return new Tuple<List<Point3d>, List<List<Point3d>>>(rhEdgePts, rhVoidPts);
        }

        internal static Tuple<Polyline, List<Polyline>> PolylinesFromAdSecProfile(IProfile profile)
        {
            IPerimeterProfile perimeter = IPerimeterProfile.Create(profile);
            Tuple<List<Point3d>, List<List<Point3d>>> pts = PtsFromAdSecPermiter(perimeter);

            Polyline solid = new Polyline(pts.Item1);
            List<Polyline> voids = new List<Polyline>();
            foreach (List<Point3d> plvoid in pts.Item2)
            {
                voids.Add(new Polyline(plvoid));
            }

            return new Tuple<Polyline, List<Polyline>>(solid, voids);
        }

        public IProfile Clone()
        {
            IProfile dup = null;
            
            // angle
            if (this.m_profile.GetType().ToString().Equals(typeof(IAngleProfile).ToString() + "_Implementation"))
            {
                IAngleProfile angle = (IAngleProfile)this.m_profile;
                dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
            }

            // catalogue
            else if (this.m_profile.GetType().ToString().Equals(typeof(ICatalogueProfile).ToString() + "_Implementation"))
            {
                dup = ICatalogueProfile.Create(this.m_profile.Description());
            }

            // channel
            else if (this.m_profile.GetType().ToString().Equals(typeof(IChannelProfile).ToString() + "_Implementation"))
            {
                IChannelProfile channel = (IChannelProfile)this.m_profile;
                dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
            }

            // circle hollow
            else if (this.m_profile.GetType().ToString().Equals(typeof(ICircleHollowProfile).ToString() + "_Implementation"))
            {
                ICircleHollowProfile circleHollow = (ICircleHollowProfile)this.m_profile;
                dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
            }

            // circle
            else if (this.m_profile.GetType().ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
            {
                ICircleProfile circle = (ICircleProfile)this.m_profile;
                dup = ICircleProfile.Create(circle.Diameter);
            }

            // ICruciformSymmetricalProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(ICruciformSymmetricalProfile).ToString() + "_Implementation"))
            {
                ICruciformSymmetricalProfile cruciformSymmetrical = (ICruciformSymmetricalProfile)this.m_profile;
                dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
            }

            // IEllipseHollowProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IEllipseHollowProfile).ToString() + "_Implementation"))
            {
                IEllipseHollowProfile ellipseHollow = (IEllipseHollowProfile)this.m_profile;
                dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
            }

            // IEllipseProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IEllipseProfile).ToString() + "_Implementation"))
            {
                IEllipseProfile ellipse = (IEllipseProfile)this.m_profile;
                dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
            }

            // IGeneralCProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IGeneralCProfile).ToString() + "_Implementation"))
            {
                IGeneralCProfile generalC = (IGeneralCProfile)this.m_profile;
                dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
            }

            // IGeneralZProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IGeneralZProfile).ToString() + "_Implementation"))
            {
                IGeneralZProfile generalZ = (IGeneralZProfile)this.m_profile;
                dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
            }

            // IIBeamAsymmetricalProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IIBeamAsymmetricalProfile).ToString() + "_Implementation"))
            {
                IIBeamAsymmetricalProfile iBeamAsymmetrical = (IIBeamAsymmetricalProfile)this.m_profile;
                dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
            }

            // IIBeamCellularProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IIBeamCellularProfile).ToString() + "_Implementation"))
            {
                IIBeamCellularProfile iBeamCellular = (IIBeamCellularProfile)this.m_profile;
                dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
            }

            // IIBeamSymmetricalProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IIBeamSymmetricalProfile).ToString() + "_Implementation"))
            {
                IIBeamSymmetricalProfile iBeamSymmetrical = (IIBeamSymmetricalProfile)this.m_profile;
                dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
            }

            // IRectangleHollowProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IRectangleHollowProfile).ToString() + "_Implementation"))
            {
                IRectangleHollowProfile rectangleHollow = (IRectangleHollowProfile)this.m_profile;
                dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
            }

            // IRectangleProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IRectangleProfile).ToString() + "_Implementation"))
            {
                IRectangleProfile rectangle = (IRectangleProfile)this.m_profile;
                dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
            }

            // IRectoEllipseProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IRectoEllipseProfile).ToString() + "_Implementation"))
            {
                IRectoEllipseProfile rectoEllipse = (IRectoEllipseProfile)this.m_profile;
                dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
            }

            // ISecantPileProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(ISecantPileProfile).ToString() + "_Implementation"))
            {
                ISecantPileProfile secantPile = (ISecantPileProfile)this.m_profile;
                dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
            }

            // ISheetPileProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(ISheetPileProfile).ToString() + "_Implementation"))
            {
                ISheetPileProfile sheetPile = (ISheetPileProfile)this.m_profile;
                dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
            }

            // IStadiumProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(IStadiumProfile).ToString() + "_Implementation"))
            {
                IStadiumProfile stadium = (IStadiumProfile)this.m_profile;
                dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
            }

            // ITrapezoidProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(ITrapezoidProfile).ToString() + "_Implementation"))
            {
                ITrapezoidProfile trapezoid = (ITrapezoidProfile)this.m_profile;
                dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
            }

            // ITSectionProfile
            else if (this.m_profile.GetType().ToString().Equals(typeof(ITSectionProfile).ToString() + "_Implementation"))
            {
                ITSectionProfile tSection = (ITSectionProfile)this.m_profile;
                dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
            }

            // IPerimeterProfile (last chance...)
            else
            {
                dup = IPerimeterProfile.Create(this.m_profile);
            }

            // modifications
            dup.IsReflectedY = this.m_profile.IsReflectedY;
            dup.IsReflectedZ = this.m_profile.IsReflectedZ;
            dup.Rotation = this.m_profile.Rotation;

            return dup;
        }

        public override string ToString()
        {
            return "AdSec " + TypeName + " {" + m_profile.Description() + "}";
        }
        public override string TypeName => "Profile";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new AdSecProfileGoo(this.Clone());
        }
        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                return Value.BoundingBox;
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return Value.BoundingBox;
        }
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return null;
        }
        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return null;
        }

        public override object ScriptVariable()
        {
            return m_profile;
        }
        public override bool CastTo<TQ>(out TQ target)
        {
            if (typeof(TQ).IsAssignableFrom(typeof(AdSecProfileGoo)))
            {
                target = (TQ)(object)this;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(Line)))
            {
                target = (TQ)(object)Value;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Curve)))
            {
                target = (TQ)(object)new GH_Curve(new PolylineCurve(Value));
                return true;
            }

            target = default(TQ);
            return false;
        }
        public override bool CastFrom(object source)
        {
            if (source == null) return false;

            // try cast using GH_Convert, if that doesnt work we are doomed
            Curve crv = null;
            if (GH_Convert.ToCurve(source, ref crv, GH_Conversion.Both))
            {
                Polyline poly;
                if (crv.TryGetPolyline(out poly))
                {
                    AdSecProfileGoo temp = new AdSecProfileGoo(poly, GhAdSec.DocumentUnits.LengthUnit);
                    this.m_value = temp.m_value;
                    this.m_profile = temp.m_profile;
                    this.m_voidEdges = temp.m_voidEdges;
                    return true;
                }
            }

            return false;
        }

        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value != null)
            {
                if (args.Color == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // not selected
                {
                    args.Pipeline.DrawPolyline(Value, GhAdSec.UI.Colour.OasysBlue, 2);
                    if (this.m_voidEdges != null)
                    {
                        foreach (Polyline crv in m_voidEdges)
                        {
                            args.Pipeline.DrawPolyline(crv, GhAdSec.UI.Colour.OasysBlue, 1);
                        }
                    }
                }
                else // selected
                {
                    args.Pipeline.DrawPolyline(Value, GhAdSec.UI.Colour.OasysYellow, 3);
                    if (this.m_voidEdges != null)
                    {
                        foreach (Polyline crv in m_voidEdges)
                        {
                            args.Pipeline.DrawPolyline(crv, GhAdSec.UI.Colour.OasysYellow, 2);
                        }
                    }
                }
            }
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }
    }
}