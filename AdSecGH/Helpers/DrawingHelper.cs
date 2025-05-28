using System;
using System.Drawing;

using Grasshopper.Kernel;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Helpers {
  public static class DrawingHelper {
    public static Color LightRed => Color.FromArgb(255, 244, 96, 96);
    public static Color LightGreen => Color.FromArgb(255, 96, 244, 96);
    public static Color LightBlue => Color.FromArgb(255, 96, 96, 234);

    public static void Draw(DisplayPipeline pipeline, DrawInstructions geometry) {
      switch (geometry) {
        case DrawPolyline drawPolyline:
          pipeline.DrawPolyline(drawPolyline.Polyline, drawPolyline.Color, drawPolyline.Thickness);
          break;
        case DrawCircle drawCircle:
          pipeline.DrawCircle(drawCircle.Circle, drawCircle.Color, drawCircle.Thickness);
          break;
        case DrawCurve drawCurve:
          pipeline.DrawCurve(drawCurve.Curve, drawCurve.Color, drawCurve.Thickness);
          break;
        case DrawDottedLine drawDottedLine:
          pipeline.DrawDottedLine(drawDottedLine.Curve, drawDottedLine.Color);
          break;
        case DrawSolidLine drawSolidLine:
          pipeline.DrawLine(drawSolidLine.Curve, drawSolidLine.Color);
          break;
        case DrawBrepShaded drawBrepShaded:
          pipeline.DrawBrepShaded(drawBrepShaded.Brep, drawBrepShaded.DisplayMaterial);
          break;
      }
    }

    public static void DrawLocalAxis(GH_PreviewWireArgs args, Line lineX, Line lineY, Line lineZ) {
      if (args == null) {
        throw new ArgumentNullException(nameof(args));
      }

      args.Pipeline.DrawLine(lineX, LightRed, 1);
      args.Pipeline.DrawLine(lineY, LightGreen, 1);
      args.Pipeline.DrawLine(lineZ, LightBlue, 1);
    }

  }

  public class DrawInstructions {
    public Color Color { get; set; }
    public int Thickness { get; set; }
    public virtual object Geometry { get; set; }
  }

  public class DrawPolyline : DrawInstructions {
    public Polyline Polyline { get; set; }
    public override object Geometry => Polyline;
  }

  public class DrawCircle : DrawInstructions {
    public Circle Circle { get; set; }
    public override object Geometry => Circle;
  }

  public class DrawCurve : DrawInstructions {
    public Curve Curve { get; set; }
    public override object Geometry => Curve;
  }

  public class DrawDottedLine : DrawInstructions {
    public Line Curve { get; set; }
  }

  public class DrawSolidLine : DrawInstructions {
    public Line Curve { get; set; }
  }

  public class DrawBrepShaded : DrawInstructions {
    public Brep Brep { get; set; }
    public DisplayMaterial DisplayMaterial { get; set; }
  }
}
