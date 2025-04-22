using System.Collections.Generic;
using System.Drawing;

using Grasshopper.GUI.Gradient;

using Rhino.Display;

namespace AdSecGH.UI {
  /// <summary>
  ///   Colour class holding the main colours used in colour scheme.
  ///   Make calls to this class to be able to easy update colours.
  /// </summary>
  public static class Colour {
    public static Brush AnnotationTextBright => Brushes.White;
    public static Brush AnnotationTextDark => Brushes.Black;
    public static Color ArupRed => Color.FromArgb(255, 230, 30, 40);
    public static Color BorderColour => UILightGrey;
    public static Color ButtonBorderColour => UILightGrey;
    //Set colours for Component UI
    public static Brush ButtonColour => new SolidBrush(GsaDarkBlue);
    public static Color ClickedBorderColour => Color.White;
    public static Brush ClickedButtonColour => new SolidBrush(WhiteOverlay(GsaDarkBlue, 0.32));
    public static DisplayMaterial Concrete {
      get {
        var material = new DisplayMaterial {
          Diffuse = Color.FromArgb(50, 73, 73, 73),
          Emission = Color.FromArgb(50, 45, 45, 45),
          Transparency = 0.6
        };
        return material;
      }
    }
    public static DisplayMaterial FailureNormal {
      get {
        var material = new DisplayMaterial {
          Diffuse = Color.FromArgb(50, 73, 73, 73),
          Emission = OasysBlue,
          Transparency = 0.6
        };
        return material;
      }
    }
    public static DisplayMaterial FailureSelected {
      get {
        var material = new DisplayMaterial {
          Diffuse = Color.FromArgb(50, 73, 73, 73),
          Emission = OasysYellow,
          Transparency = 0.6
        };
        return material;
      }
    }
    public static Color GsaDarkBlue => Color.FromArgb(255, 0, 92, 175);
    // GSA colour scheme
    public static Color GsaLightBlue => Color.FromArgb(255, 130, 169, 241);
    public static Color HoverBorderColour => Color.White;
    public static Brush HoverButtonColour => new SolidBrush(WhiteOverlay(GsaDarkBlue, 0.16));
    public static Brush HoverInactiveButtonColour => new SolidBrush(Color.FromArgb(255, 216, 216, 216));
    public static Brush InactiveButtonColour => new SolidBrush(UILightGrey);
    public static Color OasysBlue => Color.FromArgb(255, 0, 97, 160);
    public static Color OasysDarkGrey => Color.FromArgb(255, 73, 73, 73);
    // Colours for custom geometry
    public static Color OasysYellow => Color.FromArgb(255, 251, 180, 22);
    public static DisplayMaterial Reinforcement {
      get {
        var material = new DisplayMaterial {
          Diffuse = Color.FromArgb(50, 0, 0, 0),
          Emission = Color.FromArgb(50, 45, 45, 45),
          Transparency = 0.6
        };
        return material;
      }
    }
    // UI colours for custom components
    public static Color SpacerColour => GsaDarkBlue;
    public static DisplayMaterial Steel {
      get {
        var material = new DisplayMaterial {
          Diffuse = Color.FromArgb(50, 230, 28, 38),
          Emission = Color.FromArgb(50, 45, 45, 45),
          Transparency = 0.6
        };
        return material;
      }
    }
    public static Color StressStrainCurve => Color.FromArgb(255, 65, 162, 224);
    public static Color StreszzzsStrainPoint => Color.FromArgb(255, 224, 126, 65);
    public static Color UILightGrey => Color.FromArgb(255, 244, 244, 244);

    public static Color Overlay(Color original, Color overlay, double ratio) {
      return Color.FromArgb(255,
          (int)((ratio * overlay.R) + ((1 - ratio) * original.R)),
          (int)((ratio * overlay.G) + ((1 - ratio) * original.G)),
          (int)((ratio * overlay.B) + ((1 - ratio) * original.B)));
    }

    // Colours for results
    public static GH_Gradient Stress_Gradient(List<Color> colours = null) {
      var gH_Gradient = new GH_Gradient();

      if (colours == null || colours.Count < 2) {
        gH_Gradient.AddGrip(-1, Color.FromArgb(0, 0, 206));
        gH_Gradient.AddGrip(-0.666, Color.FromArgb(0, 127, 229));
        gH_Gradient.AddGrip(-0.333, Color.FromArgb(90, 220, 186));
        gH_Gradient.AddGrip(0, Color.FromArgb(205, 254, 114));
        gH_Gradient.AddGrip(0.333, Color.FromArgb(255, 220, 71));
        gH_Gradient.AddGrip(0.666, Color.FromArgb(255, 127, 71));
        gH_Gradient.AddGrip(1, Color.FromArgb(205, 0, 71));
      } else {
        for (int i = 0; i < colours.Count; i++) {
          double t = 1.0 - (2.0 / ((double)colours.Count - 1.0) * (double)i);
          gH_Gradient.AddGrip(t, colours[i]);
        }
      }

      return gH_Gradient;
    }

    public static Color WhiteOverlay(Color original, double ratio) {
      Color white = Color.White;
      return Color.FromArgb(255,
          (int)((ratio * white.R) + ((1 - ratio) * original.R)),
          (int)((ratio * white.G) + ((1 - ratio) * original.G)),
          (int)((ratio * white.B) + ((1 - ratio) * original.B)));
    }
  }
}
