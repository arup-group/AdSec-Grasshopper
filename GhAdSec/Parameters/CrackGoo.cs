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
using Oasys.AdSec.Materials;
using Oasys.Profiles;
using Oasys.AdSec;
using UnitsNet;

namespace GhAdSec.Parameters
{
    public class AdSecCrackGoo : GH_GeometricGoo<ICrack>, IGH_PreviewData
    {
        public AdSecCrackGoo(ICrack crack, Plane local)
        : base(crack)
        {
            this.m_value = crack;
            m_plane = local;

            // create point from crack position in global axis
            Point3d point = new Point3d(
                crack.Position.Y.As(GhAdSec.DocumentUnits.LengthUnit),
                crack.Position.Z.As(GhAdSec.DocumentUnits.LengthUnit),
                0);

            // remap to local coordinate system
            Rhino.Geometry.Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, local);
            point.Transform(mapFromLocal);
            m_point = point;

            // move starting point of line by half the width
            Vector3d halfCrack = new Vector3d(local.ZAxis);
            halfCrack.Unitize();
            halfCrack = new Vector3d(
                halfCrack.X * crack.Width.As(GhAdSec.DocumentUnits.LengthUnit) / 2,
                halfCrack.Y * crack.Width.As(GhAdSec.DocumentUnits.LengthUnit) / 2,
                halfCrack.Z * crack.Width.As(GhAdSec.DocumentUnits.LengthUnit) / 2);

            Transform move = Rhino.Geometry.Transform.Translation(halfCrack);
            Point3d crackStart = new Point3d(m_point);
            crackStart.Transform(move);

            // create line in opposite direction from move point
            Vector3d crackWidth = new Vector3d(halfCrack);
            crackWidth.Unitize();
            crackWidth = new Vector3d(
                crackWidth.X * crack.Width.As(GhAdSec.DocumentUnits.LengthUnit) * -1,
                crackWidth.Y * crack.Width.As(GhAdSec.DocumentUnits.LengthUnit) * -1,
                crackWidth.Z * crack.Width.As(GhAdSec.DocumentUnits.LengthUnit) * -1);

            m_line = new Line(crackStart, crackWidth);
        }
        private Point3d m_point = Point3d.Unset;
        private Plane m_plane;
        private Line m_line;
        
        public override bool IsValid => true;

        public override string TypeName => "Crack";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                if (m_line == null) { return BoundingBox.Empty; }
                LineCurve crv = new LineCurve(m_line);
                return crv.GetBoundingBox(false);
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new AdSecCrackGoo(this.Value, m_plane);
        }
        public override string ToString()
        {
            IQuantity length = new UnitsNet.Length(0, GhAdSec.DocumentUnits.LengthUnit);
            string unitAbbreviation = string.Concat(length.ToString().Where(char.IsLetter));
            return "AdSec " + TypeName + " {"
                + "Y:" + Math.Round(this.Value.Position.Y.As(GhAdSec.DocumentUnits.LengthUnit), 4) + unitAbbreviation + ", "
                + "Z:" + Math.Round(this.Value.Position.Z.As(GhAdSec.DocumentUnits.LengthUnit), 4) + unitAbbreviation + ", "
                + "Width:" + Math.Round(this.Value.Width.As(GhAdSec.DocumentUnits.LengthUnit), 4) + unitAbbreviation + "}";
        }
        public override bool CastTo<TQ>(out TQ target)
        {
            if (typeof(TQ).IsAssignableFrom(typeof(AdSecCrackGoo)))
            {
                target = (TQ)(object)new AdSecCrackGoo(this.Value, this.m_plane);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(Point3d)))
            {
                target = (TQ)(object)m_point;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Point)))
            {
                target = (TQ)(object)new GH_Point(m_point);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(Line)))
            {
                target = (TQ)(object)m_line;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Line)))
            {
                target = (TQ)(object)new GH_Line(m_line);
                return true;
            }

            target = default(TQ);
            return false;
        }
        public override bool CastFrom(object source)
        {
            if (source == null) return false;

            return false;
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (m_point.IsValid)
            {
                if (args.Color == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // not selected
                    args.Pipeline.DrawLine(m_line, GhAdSec.UI.Colour.OasysBlue, 5);
                else
                    args.Pipeline.DrawLine(m_line, GhAdSec.UI.Colour.OasysYellow, 7);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            AdSecCrackGoo dup = new AdSecCrackGoo(Value, m_plane);
            return dup;
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null) { return BoundingBox.Empty; }
            if (m_point == null) { return BoundingBox.Empty; }
            LineCurve crv = new LineCurve(m_line);
            return crv.GetBoundingBox(xform);
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return null;
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return null;
        }
    }
}