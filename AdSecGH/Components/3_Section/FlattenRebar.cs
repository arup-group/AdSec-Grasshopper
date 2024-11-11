using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.Business;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Parameters;
using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGH.Components {
  public static class UnitExtensions {

    public static string GetUnit(this LengthUnit lengthUnitGeometry) {
      IQuantity length = new Length(0, lengthUnitGeometry);
      return string.Concat(length.ToString().Where(char.IsLetter));
    }

    public static string NameWithUnits(this Attribute attribute, LengthUnit unit) {
      return $"{attribute.Name} [{unit.GetUnit()}]";
    }
  }

  public class FlattenRebarGhComponent : FlattenRebarComponent {

    public FlattenRebarGhComponent() {
      AdSecSection.OnValueChanged += goo => Section.OnValueChanged?.Invoke(Section.Value);
      var adSecSection = AdSecSection as Attribute;
      FromAttribute(ref adSecSection, Section);
      var adSecPoint = AdSecPoint as Attribute;
      FromAttribute(ref adSecPoint, Position);
      AdSecPoint.Name = Position.NameWithUnits(DefaultUnits.LengthUnitGeometry);
      Diameter.Name = Diameter.NameWithUnits(DefaultUnits.LengthUnitGeometry);
    }

    public AdSecPointArrayParameter AdSecPoint { get; set; } = new AdSecPointArrayParameter();
    public IAdSecSectionParameter AdSecSection { get; set; } = new IAdSecSectionParameter();

    private static void FromAttribute(ref Attribute update, Attribute from) {
      update.Name = from.Name;
      update.NickName = from.NickName;
      update.Description = from.Description;

      if (from is IAccessible accessible) {
        if (update is IAccessible AdSecSection) {
          AdSecSection.Access = accessible.Access;
        }
      }
    }

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecPoint,
        Diameter,
        BundleCount,
        PreLoad,
        Material,
      };
    }
  }

  public class FlattenRebar : BusinessOasysGlue<FlattenRebarGhComponent> {

    public FlattenRebar() {
      Hidden = true;
    }

    public override Guid ComponentGuid => new Guid("879ecac6-464d-4286-9113-c15fcf03e4e6");

    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.FlattenRebar;

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get section input
      var flattenSection = AdSecSection.GetFlattenSection(this, DA, 0);

      var pointGoos = new List<AdSecPointGoo>();
      var diameters = new List<GH_UnitNumber>();
      var counts = new List<int>();
      var prestresses = new List<GH_UnitNumber>();
      var materialType = new List<string>();

      // loop through rebar groups in flattened section
      foreach (var rebargrp in flattenSection.ReinforcementGroups) {
        try // first try if not a link group type
        {
          var snglBrs = (ISingleBars)rebargrp;
          foreach (var pos in snglBrs.Positions) {
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
            // string RebarMaterial = ;
            string reinforcementMat = FlattenRebarComponent.MaterialCleanUp(snglBrs.BarBundle.Material.ToString());
            materialType.Add(reinforcementMat);
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
