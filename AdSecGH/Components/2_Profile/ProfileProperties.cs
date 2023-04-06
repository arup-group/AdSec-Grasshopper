using System;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.Profiles;
using Oasys.Profiles.Properties;
using OasysGH;
using OasysGH.Components;
using OasysGH.Parameters;
using OasysGH.Units;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
    public class ProfileProperties : GH_OasysComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bb540a8e-7e7e-4299-933f-08689d7b2af8");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.ProfileInfo;

    public ProfileProperties() : base("Profile Properties",
      "Prop",
      "Properties of an AdSec Profile",
      CategoryName.Name(),
      SubCategoryName.Cat2())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Profile", "Pf", "AdSec Profile defining the Section solid boundary", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      string lengthUnitAbbreviation = Length.GetAbbreviation(DefaultUnits.LengthUnitGeometry);
      string areahUnitAbbreviation = lengthUnitAbbreviation + "\u00B2";
      string modulusUnitAbbreviation = lengthUnitAbbreviation + "\u00B3";
      string inertiaUnitAbbreviation = lengthUnitAbbreviation + "\u2074";

      pManager.AddGenericParameter("Area [" + areahUnitAbbreviation + "]", "A", "The profile's area.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Elastic Centroid, Z [" + lengthUnitAbbreviation + "]", "ECz", "The profile's elastic centroid, Z-position.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Elastic Centroid, Y [" + lengthUnitAbbreviation + "]", "ECy", "The profile's elastic centroid, Y-position.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Radius of Gyration, Z [" + lengthUnitAbbreviation + "]", "RGz", "The profile's radius of gyration, Z-position.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Radius of Gyration, Y [" + lengthUnitAbbreviation + "]", "RGy", "The profile's radius of gyration, Y-position.", GH_ParamAccess.item);
      pManager.AddNumberParameter("Shear Factor, YY", "Avy", "The profile's shear area factor, YY-ratio.", GH_ParamAccess.item);
      pManager.AddNumberParameter("Shear Factor, ZZ", "Avz", "The profile's shear area factor, ZZ-ratio.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Elastic Modulus, Y [" + modulusUnitAbbreviation + "]", "Wely", "The profile's elastic section modulus, Y-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Elastic Modulus, Z [" + modulusUnitAbbreviation + "]", "Welz", "The profile's elastic section modulus, Z-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Plastic Modulus, Y [" + modulusUnitAbbreviation + "]", "Wply", "The profile's plastic section modulus, Y-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Plastic Modulus, Z [" + modulusUnitAbbreviation + "]", "Wplz", "The profile's plastic section modulus, Z-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Torsion Constant [" + inertiaUnitAbbreviation + "]", "J", "The profile's torsion constant.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Local Moment of Inertia, Y [" + inertiaUnitAbbreviation + "]", "Iy", "The profile's second moment of area about local Y-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Local Moment of Inertia, Z [" + inertiaUnitAbbreviation + "]", "Iz", "The profile's second moment of area about local Z-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Local Moment of Inertia, YZ [" + inertiaUnitAbbreviation + "]", "Iyz", "The profile's second moment of area about local YZ-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Principal Moment of Inertia, U [" + inertiaUnitAbbreviation + "]", "Iu", "The profile's second moment of area about the principal U-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Principal Moment of Inertia, V [" + inertiaUnitAbbreviation + "]", "Iv", "The profile's second moment of area about the principal V-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Principal axis angle [rad]", "Pa", "The angle of the principal axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Surface area [" + lengthUnitAbbreviation + "]", "S/L", "The profile's surface area per unit length. This does not include the surface area of a void in case of hollow sections.", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // 0 profile
      AdSecProfileGoo profile = AdSecInput.AdSecProfileGoo(this, DA, 0);

      LengthUnit lengthUnit = DefaultUnits.LengthUnitGeometry;
      BaseUnits SI = UnitSystem.SI.BaseUnits;
      BaseUnits baseUnits = new BaseUnits(lengthUnit, SI.Mass, SI.Time, SI.Current, SI.Temperature, SI.Amount, SI.LuminousIntensity);
      UnitSystem unitSystem = new UnitSystem(baseUnits);
      AreaUnit areaUnit = new Area(1, unitSystem).Unit;

      SectionModulusUnit wUnit = SectionModulusUnit.Undefined;
      switch (lengthUnit)
      {
        case LengthUnit.Millimeter:
          wUnit = SectionModulusUnit.CubicMillimeter;
          break;
        case LengthUnit.Centimeter:
          wUnit = SectionModulusUnit.CubicCentimeter;
          break;
        case LengthUnit.Meter:
          wUnit = SectionModulusUnit.CubicMeter;
          break;
        case LengthUnit.Foot:
          wUnit = SectionModulusUnit.CubicFoot;
          break;
        case LengthUnit.Inch:
          wUnit = SectionModulusUnit.CubicInch;
          break;
      }

      AreaMomentOfInertiaUnit iUnit = new AreaMomentOfInertia(1, unitSystem).Unit;

      // area
      DA.SetData(0, new GH_UnitNumber(new Area(profile.Profile.Area().As(areaUnit), unitSystem)));

      // elastic centroid
      IPoint elcntrd = profile.Profile.ElasticCentroid();
      DA.SetData(1, new GH_UnitNumber(new Length(elcntrd.Z.As(lengthUnit), lengthUnit)));
      DA.SetData(2, new GH_UnitNumber(new Length(elcntrd.Y.As(lengthUnit), lengthUnit)));

      // radius of gyration
      IRadiusOfGyration rGyro = profile.Profile.RadiusOfGyration();
      DA.SetData(3, new GH_UnitNumber(new Length(rGyro.Z.As(lengthUnit), lengthUnit)));
      DA.SetData(4, new GH_UnitNumber(new Length(rGyro.Y.As(lengthUnit), lengthUnit)));

      // shear factor
      IShearAreaFactor fShear = profile.Profile.ShearAreaFactor();
      DA.SetData(5, fShear.YY.DecimalFractions);
      DA.SetData(6, fShear.ZZ.DecimalFractions);

      // elastic W
      ISectionModulus Wel = profile.Profile.ElasticModulus();
      DA.SetData(7, new GH_UnitNumber(new SectionModulus(Wel.Y.As(wUnit), wUnit)));
      DA.SetData(8, new GH_UnitNumber(new SectionModulus(Wel.Z.As(wUnit), wUnit)));
      // plastic W
      ISectionModulus Wpl = profile.Profile.PlasticModulus();
      DA.SetData(9, new GH_UnitNumber(new SectionModulus(Wpl.Y.As(wUnit), wUnit)));
      DA.SetData(10, new GH_UnitNumber(new SectionModulus(Wpl.Z.As(wUnit), wUnit)));

      // torsion constant
      profile.Profile.TorsionConstant();
      ITorsionConstant J = profile.Profile.TorsionConstant();
      DA.SetData(11, new GH_UnitNumber(new AreaMomentOfInertia(J.J.As(iUnit), iUnit)));

      // local axis I
      ILocalAxisSecondMomentOfArea locW = profile.Profile.LocalAxisSecondMomentOfArea();
      DA.SetData(12, new GH_UnitNumber(new AreaMomentOfInertia(locW.YY.As(iUnit), iUnit)));
      DA.SetData(13, new GH_UnitNumber(new AreaMomentOfInertia(locW.ZZ.As(iUnit), iUnit)));
      DA.SetData(14, new GH_UnitNumber(new AreaMomentOfInertia(locW.YZ.As(iUnit), iUnit)));

      // principle axis I
      IPrincipalAxisSecondMomentOfArea prinW = profile.Profile.PrincipalAxisSecondMomentOfArea();
      DA.SetData(15, new GH_UnitNumber(new AreaMomentOfInertia(prinW.UU.As(iUnit), iUnit)));
      DA.SetData(16, new GH_UnitNumber(new AreaMomentOfInertia(prinW.VV.As(iUnit), iUnit)));
      DA.SetData(17, new GH_UnitNumber(new Angle(prinW.Angle.As(AngleUnit.Radian), AngleUnit.Radian)));

      // surface area per length
      DA.SetData(18, new GH_UnitNumber(new Length(profile.Profile.SurfaceAreaPerUnitLength().As(lengthUnit), lengthUnit)));
    }
  }
}
