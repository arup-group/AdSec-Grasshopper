using System.Drawing;
using System.Collections.Generic;
using Rhino.Display;

namespace AdSecGH.UI
{
    /// <summary>
    /// Colour class holding the main colours used in colour scheme. 
    /// Make calls to this class to be able to easy update colours.
    /// 
    /// </summary>
    public class Colour
    {

        // UI colours for custom components
        public static Color SpacerColour
        {
            get { return GsaDarkBlue; }
        }
        public static Brush AnnotationTextDark
        {
            get { return Brushes.Black; }
        }

        // Colours for custom geometry
        public static Color OasysYellow
        {
            get { return Color.FromArgb(255, 251, 180, 22); }
        }
        public static Color OasysBlue
        {
            get { return Color.FromArgb(255, 0, 97, 160); }
        }
        public static Color ArupRed
        {
            get { return Color.FromArgb(255, 230, 30, 40); }
        }
        public static Color StressStrainCurve
        {
            get { return Color.FromArgb(255, 65, 162, 224); }
        }
        public static Color StreszzzsStrainPoint
        {
            get { return Color.FromArgb(255, 224, 126, 65); }
        }
        public static DisplayMaterial FailureNormal
        {
            get
            {
                DisplayMaterial material = new DisplayMaterial
                {
                    Diffuse = Color.FromArgb(50, 73, 73, 73),
                    Emission = OasysBlue,
                    Transparency = 0.6
                };
                return material;
            }
        }
        public static DisplayMaterial FailureSelected
        {
            get
            {
                DisplayMaterial material = new DisplayMaterial
                {
                    Diffuse = Color.FromArgb(50, 73, 73, 73),
                    Emission = OasysYellow,
                    Transparency = 0.6
                };
                return material;
            }
        }
        public static DisplayMaterial Concrete
        {
            get
            {
                DisplayMaterial material = new DisplayMaterial
                {
                    Diffuse = Color.FromArgb(50, 73, 73, 73),
                    Emission = Color.FromArgb(50, 45, 45, 45),
                    Transparency = 0.6
                };
                return material;
            }
        }
        public static DisplayMaterial Steel
        {
            get
            {
                DisplayMaterial material = new DisplayMaterial
                {
                    Diffuse = Color.FromArgb(50, 230, 28, 38),
                    Emission = Color.FromArgb(50, 45, 45, 45),
                    Transparency = 0.6
                };
                return material;
            }
        }
        public static DisplayMaterial Reinforcement
        {
            get
            {
                DisplayMaterial material = new DisplayMaterial
                {
                    Diffuse = Color.FromArgb(50, 0, 0, 0),
                    Emission = Color.FromArgb(50, 45, 45, 45),
                    Transparency = 0.6
                };
                return material;
            }
        }
        // Colours for results
        public static Grasshopper.GUI.Gradient.GH_Gradient Stress_Gradient(List<System.Drawing.Color> colours = null)
        {

            Grasshopper.GUI.Gradient.GH_Gradient gH_Gradient = new Grasshopper.GUI.Gradient.GH_Gradient();

            if (colours.Count < 2 || colours == null)
            {
                gH_Gradient.AddGrip(-1, System.Drawing.Color.FromArgb(0, 0, 206));
                gH_Gradient.AddGrip(-0.666, System.Drawing.Color.FromArgb(0, 127, 229));
                gH_Gradient.AddGrip(-0.333, System.Drawing.Color.FromArgb(90, 220, 186));
                gH_Gradient.AddGrip(0, System.Drawing.Color.FromArgb(205, 254, 114));
                gH_Gradient.AddGrip(0.333, System.Drawing.Color.FromArgb(255, 220, 71));
                gH_Gradient.AddGrip(0.666, System.Drawing.Color.FromArgb(255, 127, 71));
                gH_Gradient.AddGrip(1, System.Drawing.Color.FromArgb(205, 0, 71));
            }
            else
            {
                for (int i = 0; i < colours.Count; i++)
                {
                    double t = 1.0 - 2.0 / ((double)colours.Count - 1.0) * (double)i;
                    gH_Gradient.AddGrip(t, colours[i]);
                }
            }

            return gH_Gradient;
        }

        #region unused GSA scheme
        // GSA colour scheme
        //public static Color GsaGreen
        //{
        //    get { return Color.FromArgb(255, 48, 170, 159); }
        //}
        //public static Color GsaDarkGreen
        //{
        //    get { return Color.FromArgb(255, 27, 141, 133); }
        //}
        //public static Color GsaBlue
        //{
        //    get { return Color.FromArgb(255, 99, 148, 237); }
        //}
        public static Color GsaDarkBlue
        {
            get { return Color.FromArgb(255, 0, 92, 175); }
        }
        //public static Color GsaLightBlue
        //{
        //    get { return Color.FromArgb(255, 130, 169, 241); }
        //}

        public static Color GsaLightGrey
        {
            get { return Color.FromArgb(255, 244, 244, 244); }
        }

        //public static Color GsaDarkGrey
        //{
        //    get { return Color.FromArgb(255, 164, 164, 164); }
        //}

        //public static Color GsaDarkPurple
        //{
        //    get { return Color.FromArgb(255, 136, 0, 136); }
        //}

        //public static Color GsaGold
        //{
        //    get { return Color.FromArgb(255, 255, 183, 0); }
        //}

        ////Set colours for Component UI
        //public static Brush ButtonColor
        //{
        //    get { return new SolidBrush(GsaDarkBlue); }
        //}
        //public static Brush ClickedButtonColor
        //{
        //    get { return new SolidBrush(GsaLightBlue); }
        //}
        //public static Brush InactiveButtonColor
        //{
        //    get { return new SolidBrush(Color.FromArgb(255, 216, 216, 216)); }
        //}

        public static Color BorderColour
        {
            get { return GsaLightGrey; }
        }

        //public static Color ClickedBorderColour
        //{
        //    get { return Color.Black; }
        //}

        //public static Brush InactiveBorderColor
        //{
        //    get { return new SolidBrush(Color.FromArgb(0, 216, 216, 216)); }
        //}



        //public static Brush AnnotationTextDarkGrey
        //{
        //    get { return new SolidBrush(GsaDarkGrey); }
        //}

        //public static Brush AnnotationTextBright
        //{
        //    get { return Brushes.White; }
        //}
        //public static Color ActiveColour
        //{
        //    get { return GsaDarkBlue; }
        //}

        //public static Brush ActiveBrush
        //{
        //    get { return new SolidBrush(ActiveColour); }
        //}


        ////Set colours for custom geometry
        //public static Color Node
        //{
        //    get { return GsaGreen; }
        //}
        //public static Color NodeSelected
        //{
        //    get { return GsaDarkPurple; }
        //}
        //public static Color Support
        //{
        //    get { return Color.FromArgb(255, 0, 100, 0); }
        //}
        //public static Color Release
        //{
        //    get { return Color.FromArgb(255, 153, 32, 32); }
        //}

        //public static Color Member1dNode
        //{
        //    get { return GsaDarkGreen; }
        //}

        //public static Color Member1dNodeSelected
        //{
        //    get { return GsaGold; }
        //}

        //public static Color Element1dNode
        //{
        //    get { return GsaDarkGreen; }
        //}
        //public static Color Element1dNodeSelected
        //{
        //    get { return GsaDarkGreen; }
        //}

        //public static Color Dummy1D
        //{
        //    get { return Color.FromArgb(255, 143, 143, 143); }
        //}

        //public static Color Member1d
        //{
        //    get { return GsaGreen; }
        //}

        //public static Color Element1d
        //{
        //    get { return Color.FromArgb(255, 95, 190, 180); }
        //}


        //public static Color Member1dSelected
        //{
        //    get { return GsaDarkPurple; }
        //}

        //public static Color Element1dSelected
        //{
        //    get { return GsaDarkPurple; }
        //}

        //public static Color Element2dEdge
        //{
        //    get { return GsaBlue; }
        //}

        //public static Color Element2dEdgeSelected
        //{
        //    get { return GsaDarkPurple; }
        //}


        //public static DisplayMaterial Element2dFace
        //{
        //    get
        //    {
        //        DisplayMaterial material = new DisplayMaterial
        //        {
        //            Diffuse = Color.FromArgb(50, 150, 150, 150),
        //            Emission = Color.FromArgb(50, 190, 190, 190),
        //            Transparency = 0.1
        //        };
        //        return material;
        //    }
        //}
        //public static DisplayMaterial Element3dFace
        //{
        //    get
        //    {
        //        DisplayMaterial material = new DisplayMaterial
        //        {
        //            Diffuse = Color.FromArgb(50, 150, 150, 150),
        //            Emission = Color.FromArgb(50, 190, 190, 190),
        //            Transparency = 0.3
        //        };
        //        return material;
        //    }
        //}
        //public static DisplayMaterial FaceCustom(Color colour)
        //{
        //    DisplayMaterial material = new DisplayMaterial()
        //    {
        //        Diffuse = Color.FromArgb(50, colour.R, colour.G, colour.B),
        //        Emission = Color.White, // Color.FromArgb(50, 190, 190, 190),
        //        Transparency = 0.1
        //    };
        //    return material;
        //}

        //public static DisplayMaterial Element2dFaceSelected
        //{
        //    get
        //    {
        //        DisplayMaterial material = new DisplayMaterial
        //        {
        //            Diffuse = Color.FromArgb(5, 150, 150, 150),
        //            Emission = Color.FromArgb(5, 150, 150, 150),
        //            Transparency = 0.2
        //        };
        //        return material;
        //    }
        //}
        //public static Color Member2dEdge
        //{
        //    get { return GsaBlue; }
        //}

        //public static Color Member2dEdgeSelected
        //{
        //    get { return GsaDarkPurple; }
        //}
        //public static DisplayMaterial Member2dFace
        //{
        //    get
        //    {
        //        DisplayMaterial material = new DisplayMaterial
        //        {
        //            Diffuse = Color.FromArgb(50, 150, 150, 150),
        //            Emission = Color.FromArgb(50, 45, 45, 45),
        //            Transparency = 0.1
        //        };
        //        return material;
        //    }
        //}
        //public static DisplayMaterial Dummy2D
        //{
        //    get
        //    {
        //        DisplayMaterial material = new DisplayMaterial
        //        {
        //            Diffuse = Color.FromArgb(1, 143, 143, 143),
        //            Emission = Color.White, //Color.FromArgb(1, 45, 45, 45),
        //            Transparency = 0.9
        //        };
        //        return material;
        //    }
        //}

        //public static DisplayMaterial Member2dFaceSelected
        //{
        //    get
        //    {
        //        DisplayMaterial material = new DisplayMaterial
        //        {
        //            Diffuse = Color.FromArgb(5, 150, 150, 150),
        //            Emission = Color.FromArgb(5, 5, 5, 5),
        //            Transparency = 0.2
        //        };
        //        return material;
        //    }
        //}
        //public static Color Member2dInclPt
        //{
        //    get { return GsaGold; }
        //}

        //public static Color Member2dInclLn
        //{
        //    get { return GsaGold; }
        //}
        #endregion

    }
}
