﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.Mesh;

using OasysGH.Units;

using OasysUnits;

using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecInteractionDiagramGoo : GH_GeometricGoo<Polyline>, IGH_PreviewData {
    public enum InteractionCurveType {
      NM,
      MM
    }

    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }
        return Value.BoundingBox;
      }
    }
    public BoundingBox ClippingBox => Boundingbox;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => (_curveType == InteractionCurveType.NM) ? "N-M" : "M-M";
    private readonly ILoadCurve _loadCurve;
    private readonly Angle _angle;
    private readonly InteractionCurveType _curveType;
    private List<Line> _lineAxes = new List<Line>();
    private List<Line> _gridLines = new List<Line>();
    private Rectangle3d _plotBoundary;
    private List<Text3d> _3dTexts;
    /// <summary>
    /// This constuctor will create an N-M type interaction diagram
    /// </summary>
    /// <param name="loadCurve"></param>
    /// <param name="angle"></param>
    internal AdSecInteractionDiagramGoo(ILoadCurve loadCurve, Angle angle, Rectangle3d plotBoundary, InteractionCurveType curveType = InteractionCurveType.NM) {
      if (loadCurve == null) {
        return;
      }
      _angle = angle;
      _loadCurve = loadCurve;
      _curveType = curveType;
      m_value = CurveToPolyline();
      _plotBoundary = plotBoundary;
      UpdatePreview(_plotBoundary);
    }

    public Polyline CurveToPolyline() {
      var points = new List<Point3d>();
      bool isMM = _curveType == InteractionCurveType.MM;
      foreach (ILoad load in _loadCurve.Points) {
        if (isMM) {
          var point = new Point3d(
          load.YY.As(DefaultUnits.MomentUnit), // plot yy on x-axis
          load.ZZ.As(DefaultUnits.MomentUnit), // plot zz on y-axis
          0);
          points.Add(point);
        } else {
          var point = new Point3d(
            load.ZZ.As(DefaultUnits.MomentUnit),
            load.YY.As(DefaultUnits.MomentUnit),
            load.X.As(DefaultUnits.ForceUnit) * -1); // flip y-axis for NM-diagram
          points.Add(point);
        }
      }
      // add first point to the end to make a closed curve
      points.Add(points[0]);
      if (isMM) {
        return new Polyline(points);
      }

      Plane local = Plane.WorldYZ;
      if (!_angle.Radians.Equals(0)) {
        local.Rotate(_angle.Radians * -1, Vector3d.ZAxis);
      }
      // transform to local plane
      var mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(local, Plane.WorldXY);
      var polyline = new Polyline(points);
      polyline.Transform(mapFromLocal);
      return polyline;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecInteractionDiagramGoo))) {
        target = (Q)(object)new AdSecInteractionDiagramGoo(_loadCurve, _angle, _plotBoundary, _curveType);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Curve))) {
        var polylineCurve = m_value.ToPolylineCurve();
        target = (Q)(object)new GH_Curve(polylineCurve);
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value != null) {
        // draw diagram polyline
        if (args.Color == Color.FromArgb(255, 150, 0, 0)) {
          //Grasshopper.Instances.Settings.GetValue("DefaultPreviewColourSelected", System.Drawing.Color.White))
          args.Pipeline.DrawPolyline(Value, UI.Colour.OasysBlue, 2);
        } else {
          args.Pipeline.DrawPolyline(Value, UI.Colour.OasysYellow, 2);
        }

        // draw plot diagram
        args.Pipeline.DrawLines(_gridLines, UI.Colour.OasysDarkGrey, 1);
        args.Pipeline.DrawLines(_lineAxes, UI.Colour.OasysDarkGrey, 2);
        foreach (Text3d txt in _3dTexts) {
          args.Pipeline.Draw3dText(txt, UI.Colour.OasysDarkGrey);
        }
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecInteractionDiagramGoo(_loadCurve, _angle, _plotBoundary, _curveType);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      Polyline duplicatePolyline = m_value.Duplicate();
      duplicatePolyline.Transform(xform);
      return duplicatePolyline.BoundingBox;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      string interactionType = "";
      if (_curveType == InteractionCurveType.NM) {
        interactionType = "N-M (Force-Moment Interaction)";
      } else {
        interactionType = "M-M (Moment-Moment Interaction)";
      }
      return $"AdSec {TypeName} {{{interactionType}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    private void UpdatePreview(Rectangle3d plotBoundary) {
      // get bounding box of load curve polyline
      BoundingBox unitbbox = m_value.BoundingBox;
      // get bounding box of plot boundary
      BoundingBox plotbbox = plotBoundary.BoundingBox;

      // create axes
      var xAxis = new Diagram.GridAxis((float)unitbbox.PointAt(0, 0, 0).X, (float)unitbbox.PointAt(1, 0, 0).X);
      var yAxis = new Diagram.GridAxis((float)unitbbox.PointAt(0, 0, 0).Y, (float)unitbbox.PointAt(0, 1, 0).Y);

      // move to plot boundary
      var translate = new Vector3d(
        plotbbox.PointAt(0, 0, 0).X - xAxis.min_value,
        plotbbox.PointAt(0, 0, 0).Y - yAxis.min_value,
        0);
      m_value.Transform(Rhino.Geometry.Transform.Translation(translate));

      // set plane for NU scaling operation
      Plane pln = Plane.WorldXY;
      pln.Origin = plotbbox.PointAt(0, 0, 0);

      // calculate x-factor
      double sclX = (plotbbox.PointAt(1, 0, 0).X - plotbbox.PointAt(0, 0, 0).X) /
        (xAxis.max_value - xAxis.min_value);

      // calculate y-factor
      double sclY = (plotbbox.PointAt(0, 1, 0).Y - plotbbox.PointAt(0, 0, 0).Y) /
        (yAxis.max_value - yAxis.min_value);

      // scale unit polyline to fit in plot boundary
      m_value.Transform(Rhino.Geometry.Transform.Scale(
        pln, sclX, sclY, 1));

      // set annotation text size
      double size = Math.Min(
          Math.Abs(plotbbox.PointAt(1, 0, 0).X - plotbbox.PointAt(0, 0, 0).X),
          Math.Abs(plotbbox.PointAt(0, 1, 0).Y - plotbbox.PointAt(0, 0, 0).Y)) / 50;

      // create gridLine lines
      _gridLines = new List<Line>();
      _lineAxes = new List<Line>();
      _3dTexts = new List<Text3d>();
      Plane txtPln = Plane.WorldXY;
      // loop through all values in y axis to create x-dir grids
      foreach (float step in yAxis.MajorRange) {
        // create gridline in original unit
        var gridLine = new Line(
          new Point3d(xAxis.min_value, step, 0),
          new Point3d(xAxis.max_value, step, 0));
        // move to plot boundary
        gridLine.Transform(Rhino.Geometry.Transform.Translation(translate));
        // scale to plot boundary
        gridLine.Transform(Rhino.Geometry.Transform.Scale(
          pln, sclX, sclY, 1));
        // if step value is 0 we want to add it to the major axis
        // that we will give a different colour
        if (step.Equals(0)) {
          _lineAxes.Add(gridLine);
        } else {
          _gridLines.Add(gridLine);
        }

        // add step annotation
        txtPln.Origin = new Point3d(
            gridLine.PointAt(0).X - (xAxis.major_step / 2 * sclX),
            gridLine.PointAt(0).Y, 0);

        // add step annotation
        txtPln.Origin = new Point3d(
          gridLine.PointAt(0).X - size,
          gridLine.PointAt(0).Y, 0);
        string displayval = (_curveType == InteractionCurveType.NM) ? (step * -1).ToString() : step.ToString();
        var txt = new Text3d(displayval, txtPln, size) {
          HorizontalAlignment = TextHorizontalAlignment.Right,
          VerticalAlignment = TextVerticalAlignment.Middle
        };
        _3dTexts.Add(txt);

      }

      // do the same as above but for the other axis
      foreach (float step in xAxis.MajorRange) {
        var gridLine = new Line(
          new Point3d(step, yAxis.min_value, 0),
          new Point3d(step, yAxis.max_value, 0));
        gridLine.Transform(Rhino.Geometry.Transform.Translation(translate));
        gridLine.Transform(Rhino.Geometry.Transform.Scale(
          pln, sclX, sclY, 1));
        if (step.Equals(0)) {
          _lineAxes.Add(gridLine);
        } else {
          _gridLines.Add(gridLine);
        }

        txtPln.Origin = new Point3d(
          gridLine.PointAt(0).X,
          gridLine.PointAt(0).Y - size, 0);
        var txt = new Text3d(step.ToString(), txtPln, size) {
          HorizontalAlignment = TextHorizontalAlignment.Center,
          VerticalAlignment = TextVerticalAlignment.Top
        };
        _3dTexts.Add(txt);
      }
      // add the boundary lines
      _lineAxes.AddRange(plotBoundary.ToPolyline().GetSegments());

      // Create axis labels
      string momentAxis = $"Moment [{Moment.GetAbbreviation(DefaultUnits.MomentUnit)}]";
      string myyAxis = $"Myy [{Moment.GetAbbreviation(DefaultUnits.MomentUnit)}]";
      string mzzAxis = $"Mzz [{Moment.GetAbbreviation(DefaultUnits.MomentUnit)}]";
      IQuantity force = new Force(0, DefaultUnits.ForceUnit);
      string forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
      string forceAxis = $"Axial force [{forceUnitAbbreviation}]";

      string annoXaxis = (_curveType == InteractionCurveType.NM) ? momentAxis : myyAxis;
      string annoYaxis = (_curveType == InteractionCurveType.NM) ? forceAxis : mzzAxis;

      // find largest annotation value string length
      double offset = Math.Max(
          Math.Abs(Math.Min(xAxis.min_value, yAxis.min_value)).ToString().Length + 1,
          Math.Max(xAxis.max_value, yAxis.max_value).ToString().Length);

      txtPln.Origin = new Point3d(
        plotbbox.PointAt(0.5, 0, 0).X,
        plotbbox.PointAt(0, 0, 0).Y - (size * 3), 0);
      var txtX = new Text3d(annoXaxis, txtPln, size) {
        HorizontalAlignment = TextHorizontalAlignment.Center,
        VerticalAlignment = TextVerticalAlignment.Top
      };
      _3dTexts.Add(txtX);
      txtPln.Origin = new Point3d(
        plotbbox.PointAt(0, 0, 0).X - (size * offset * 1.1),
        plotbbox.PointAt(0, 0.5, 0).Y, 0);
      txtPln.Rotate(Math.PI / 2, Vector3d.ZAxis);
      var txtY = new Text3d(annoYaxis, txtPln, size) {
        HorizontalAlignment = TextHorizontalAlignment.Center,
        VerticalAlignment = TextVerticalAlignment.Bottom
      };
      _3dTexts.Add(txtY);
    }
  }
}
