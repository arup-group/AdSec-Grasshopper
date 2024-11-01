using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Parameters;

using AdSecGH.Helpers.GH;
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

using Attribute = Oasys.Business.Attribute;

namespace AdSecGH.Components {
  public class FlattenRebarComponent : IBusinessComponent {

    public IAdSecSectionParameter Section { get; set; } = new IAdSecSectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to get single rebars from",
      Access = Access.Item,
    };

    public AdSecPointArrayParameter Position { get; set; } = new AdSecPointArrayParameter {
      Name = "Position [" + DefaultUnits.LengthUnitGeometry + "]",
      NickName = "Vx",
      Description = "Rebar position as 2D vertex in the section's local yz-plane",
      Access = Access.List,
    };

    public DoubleArrayParameter Diameter { get; set; } = new DoubleArrayParameter {
      Name = "Diameter [" + DefaultUnits.LengthUnitGeometry + "]",
      NickName = "Ø",
      Description = "Bar Diameter",
      Access = Access.List,
    };

    public IntegerArrayParameter BundleCount { get; set; } = new IntegerArrayParameter {
      Name = "Bundle Count",
      NickName = "N",
      Description = "Count per bundle (1, 2, 3 or 4)",
      Access = Access.List,
    };

    public DoubleArrayParameter PreLoad { get; set; } = new DoubleArrayParameter {
      Name = "PreLoad",
      NickName = "P",
      Description = "The pre-load per reinforcement bar. Positive value is tension.",
      Access = Access.List,
    };

    public ComponentAttribute Metadata { get; set; } = new ComponentAttribute {
      Name = "FlattenRebar",
      NickName = "FRb",
      Description = "Flatten all rebars in a section into single bars.",
    };
    public ComponentOrganisation Organisation { get; set; } = new ComponentOrganisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };

    public Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Section,
      };
    }

    public Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Position,
        Diameter,
        BundleCount,
        PreLoad,
      };
    }

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() { throw new NotImplementedException(); }
  }

  public class FlattenRebar : BusinessOasysGlue<FlattenRebarComponent> {

    public FlattenRebar() {
      Hidden = true;
    }

    public override Guid ComponentGuid => new Guid("879ecac6-464d-4286-9113-c15fcf03e4e6");

    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.FlattenRebar;

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

      pManager.AddGenericParameter("Position [" + lengthUnitAbbreviation + "]", "Vx",
        "Rebar position as 2D vertex in the section's local yz-plane ", GH_ParamAccess.list);
      pManager.HideParameter(0);
      pManager.AddGenericParameter("Diameter [" + lengthUnitAbbreviation + "]", "Ø", "Bar Diameter",
        GH_ParamAccess.list);
      pManager.AddIntegerParameter("Bundle Count", "N", "Count per bundle (1, 2, 3 or 4)", GH_ParamAccess.list);
      pManager.AddGenericParameter("PreLoad", "P", "The pre-load per reinforcement bar. Positive value is tension.",
        GH_ParamAccess.list);
      pManager.AddGenericParameter("Material", "Mat", "Material Type", GH_ParamAccess.list);
    }

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
