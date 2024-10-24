﻿using System;
using System.Collections.Generic;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.Profiles;

using OasysGH;
using OasysGH.Components;
using OasysGH.Parameters;
using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Components {
  public class FlattenRebar : GH_OasysComponent {
    public override Guid ComponentGuid => new Guid("879ecac6-464d-4286-9113-c15fcf03e4e6");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.FlattenRebar;

    public FlattenRebar() : base(
      "FlattenRebars",
      "FRb",
      "Flatten all rebars in a section into single bars.",
      CategoryName.Name(),
      SubCategoryName.Cat4()) {
      Hidden = true;
    }


    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to get single rebars from", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      IQuantity length = new Length(0, DefaultUnits.LengthUnitGeometry);
      string lengthUnitAbbreviation = string.Concat(length.ToString().Where(char.IsLetter));
      IQuantity stress = new Pressure(0, DefaultUnits.StressUnitResult);
      string stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));
      string strainUnitAbbreviation = Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      IQuantity force = new Force(0, DefaultUnits.ForceUnit);
      string forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));

      pManager.AddGenericParameter("Position [" + lengthUnitAbbreviation + "]", "Vx", "Rebar position as 2D vertex in the section's local yz-plane ", GH_ParamAccess.list);
      pManager.HideParameter(0);
      pManager.AddGenericParameter("Diameter [" + lengthUnitAbbreviation + "]", "Ø", "Bar Diameter", GH_ParamAccess.list);
      pManager.AddIntegerParameter("Bundle Count", "N", "Count per bundle (1, 2, 3 or 4)", GH_ParamAccess.list);
      pManager.AddGenericParameter("PreLoad", "P", "The pre-load per reinforcement bar. Positive value is tension.", GH_ParamAccess.list);
      pManager.AddGenericParameter("Material", "Mat", "Material Type", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get section input
      AdSecSection section = AdSecInput.AdSecSection(this, DA, 0);

      // create flattened section
      ISection flat = null;
      if (section.DesignCode != null) {
        var adSec = IAdSec.Create(section.DesignCode);
        flat = adSec.Flatten(section.Section);
      } else {
        var prof = IPerimeterProfile.Create(section.Section.Profile);
        flat = ISection.Create(prof, section.Section.Material);
      }

      var pointGoos = new List<AdSecPointGoo>();
      var diameters = new List<GH_UnitNumber>();
      var counts = new List<int>();
      var prestresses = new List<GH_UnitNumber>();
      var materialType = new List<string>();

      // loop through rebar groups in flattened section
      foreach (IGroup rebargrp in flat.ReinforcementGroups) {
        try // first try if not a link group type
        {
          var snglBrs = (ISingleBars)rebargrp;
          foreach (IPoint pos in snglBrs.Positions) {
            // position
            pointGoos.Add(new AdSecPointGoo(pos));

            // diameter
            diameters.Add(new GH_UnitNumber(snglBrs.BarBundle.Diameter.ToUnit(DefaultUnits.LengthUnitGeometry)));

            // bundle count
            counts.Add(snglBrs.BarBundle.CountPerBundle);

            // preload
            if (snglBrs.Preload != null) {
              try {
                var force = (IPreForce)snglBrs.Preload;
                prestresses.Add(new GH_UnitNumber(force.Force.ToUnit(DefaultUnits.ForceUnit)));
              } catch (Exception) {
                try {
                  var stress = (IPreStress)snglBrs.Preload;
                  prestresses.Add(new GH_UnitNumber(stress.Stress.ToUnit(DefaultUnits.StressUnitResult)));
                } catch (Exception) {
                  var strain = (IPreStrain)snglBrs.Preload;
                  prestresses.Add(new GH_UnitNumber(strain.Strain.ToUnit(DefaultUnits.StrainUnitResult)));
                }
              }
            }

            // material
            string rebmat = snglBrs.BarBundle.Material.ToString();
            rebmat = rebmat.Replace("Oasys.AdSec.Materials.I", "");
            rebmat = rebmat.Replace("_Implementation", "");
            materialType.Add(rebmat);
          }
        } catch (Exception) {
          // do nothing if rebar is link
        }
      }

      DA.SetDataList(0, pointGoos);
      DA.SetDataList(1, diameters);
      DA.SetDataList(2, counts);
      DA.SetDataList(3, prestresses);
      DA.SetDataList(4, materialType);
    }
  }
}
