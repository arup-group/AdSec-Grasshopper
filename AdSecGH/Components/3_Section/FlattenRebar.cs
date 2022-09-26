using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysGH.Parameters;
using OasysUnits;

namespace AdSecGH.Components
{
  public class FlattenRebar : GH_OasysComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("879ecac6-464d-4286-9113-c15fcf03e4e6");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.FlattenRebar;

    public FlattenRebar()
      : base("FlattenRebars", "FRb", "Flatten all rebars in a section into single bars.",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat4())
    { this.Hidden = true; } // sets the initial state of the component to hidden
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    #endregion

    #region Input and output

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to get single rebars from", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      IQuantity length = new Length(0, Units.LengthUnit);
      string lengthUnitAbbreviation = string.Concat(length.ToString().Where(char.IsLetter));
      IQuantity stress = new Pressure(0, Units.StressUnit);
      string stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));
      string strainUnitAbbreviation = Strain.GetAbbreviation(Units.StrainUnit);
      IQuantity force = new Force(0, Units.ForceUnit);
      string forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));

      pManager.AddGenericParameter("Position [" + lengthUnitAbbreviation + "]", "Vx", "Rebar position as 2D vertex in the section's local yz-plane ", GH_ParamAccess.list);
      pManager.HideParameter(0);
      pManager.AddGenericParameter("Diameter [" + lengthUnitAbbreviation + "]", "Ø", "Bar Diameter", GH_ParamAccess.list);
      pManager.AddIntegerParameter("Bundle Count", "N", "Count per bundle (1, 2, 3 or 4)", GH_ParamAccess.list);
      pManager.AddGenericParameter("PreLoad", "P", "The pre-load per reinforcement bar. Positive value is tension.", GH_ParamAccess.list);
      pManager.AddGenericParameter("Material", "Mat", "Material Type", GH_ParamAccess.list);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // get section input
      AdSecSection section = GetInput.AdSecSection(this, DA, 0);

      // create flattened section
      ISection flat = null;
      if (section.DesignCode != null) //{ code = Oasys.AdSec.DesignCode.EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex; }
      {
        IAdSec adSec = IAdSec.Create(section.DesignCode);
        flat = adSec.Flatten(section.Section);
      }
      else
      {
        IPerimeterProfile prof = IPerimeterProfile.Create(section.Section.Profile);
        flat = ISection.Create(prof, section.Section.Material);
      }

      List<AdSecPointGoo> pointGoos = new List<AdSecPointGoo>();
      List<GH_UnitNumber> diameters = new List<GH_UnitNumber>();
      List<int> counts = new List<int>();
      List<GH_UnitNumber> prestresses = new List<GH_UnitNumber>();
      List<string> materialType = new List<string>();

      // loop through rebar groups in flattened section
      foreach (IGroup rebargrp in flat.ReinforcementGroups)
      {
        try // first try if not a link group type
        {
          ISingleBars snglBrs = (ISingleBars)rebargrp;
          foreach (IPoint pos in snglBrs.Positions)
          {
            // position
            pointGoos.Add(new AdSecPointGoo(pos));

            // diameter
            diameters.Add(new GH_UnitNumber(new Length(snglBrs.BarBundle.Diameter.As(Units.LengthUnit), Units.LengthUnit)));

            // bundle count
            counts.Add(snglBrs.BarBundle.CountPerBundle);

            // preload
            if (snglBrs.Preload != null)
            {
              try
              {
                IPreForce force = (IPreForce)snglBrs.Preload;
                prestresses.Add(new GH_UnitNumber(new Force(force.Force.As(Units.ForceUnit), Units.ForceUnit)));
              }
              catch (Exception)
              {
                try
                {
                  IPreStress stress = (IPreStress)snglBrs.Preload;
                  prestresses.Add(new GH_UnitNumber(new Pressure(stress.Stress.As(Units.StressUnit), Units.StressUnit)));
                }
                catch (Exception)
                {
                  IPreStrain strain = (IPreStrain)snglBrs.Preload;
                  prestresses.Add(new GH_UnitNumber(new Strain(strain.Strain.As(Units.StrainUnit), Units.StrainUnit)));
                }
              }
            }

            // material
            string rebmat = snglBrs.BarBundle.Material.ToString();
            rebmat = rebmat.Replace("Oasys.AdSec.Materials.I", "");
            rebmat = rebmat.Replace("_Implementation", "");
            materialType.Add(rebmat);

          }
        }
        catch (Exception)
        {
          // do nothing if rebar is link
        }
      }

      // ### output ###

      DA.SetDataList(0, pointGoos);
      DA.SetDataList(1, diameters);
      DA.SetDataList(2, counts);
      DA.SetDataList(3, prestresses);
      DA.SetDataList(4, materialType);
    }
  }
}
