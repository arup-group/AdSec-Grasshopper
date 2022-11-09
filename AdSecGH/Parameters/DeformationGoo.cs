using System;
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
using OasysUnits;

namespace AdSecGH.Parameters
{
  public class AdSecDeformationGoo : GH_Goo<IDeformation>
  {
    public AdSecDeformationGoo(IDeformation deformation)
    : base(deformation)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Deformation";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecDeformationGoo(this.Value);
    }
    public override string ToString()
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(Units.StrainUnit);
      IQuantity curvature = new Curvature(0, Units.CurvatureUnit);
      string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
      return "AdSec " + TypeName + " {"
          + Math.Round(this.Value.X.As(Units.StrainUnit), 4) + strainUnitAbbreviation + ", "
          + Math.Round(this.Value.YY.As(Units.CurvatureUnit), 4) + curvatureUnitAbbreviation + ", "
          + Math.Round(this.Value.ZZ.As(Units.CurvatureUnit), 4) + curvatureUnitAbbreviation + "}";
    }
  }
}
