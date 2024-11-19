using System;
using System.Collections.Generic;

using AdSecCore;
using AdSecCore.Parameters;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.Business;
using Oasys.Profiles;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGH.Components {
  public class FlattenRebarComponent : IBusinessComponent {

    public ISectionParameter Section { get; set; } = new ISectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to get single rebars from",
      Access = Access.Item,
    };

    public IPointArrayParameter Position { get; set; } = new IPointArrayParameter {
      Name = "Position",
      NickName = "Vx",
      Description = "Rebar position as 2D vertex in the section's local yz-plane",
      Access = Access.List,
    };

    public DoubleArrayParameter Diameter { get; set; } = new DoubleArrayParameter {
      Name = "Diameter",
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

    public StringArrayParam Material { get; set; } = new StringArrayParam {
      Name = "Material",
      NickName = "Mat",
      Description = "Material Type",
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

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Section,
      };
    }

    public virtual Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Position,
        Diameter,
        BundleCount,
        PreLoad,
        Material,
      };
    }

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() {
      var flattenSection = Section.Value.FlattenSection();
      var lengthUnitGeometry = ContextUnits.Instance.LengthUnitGeometry;
      // Output process
      var diameters = new List<double>();
      var positions = new List<IPoint>();
      var bundleCounts = new List<int>();
      var preloads = new List<double>();
      var materials = new List<string>();
      foreach (var reinforcementGroup in flattenSection.ReinforcementGroups) {
        if (reinforcementGroup is ISingleBars singleBars) {
          foreach (var position in singleBars.Positions) {
            positions.Add(position);
            bundleCounts.Add(singleBars.BarBundle.CountPerBundle);
            diameters.Add(singleBars.BarBundle.Diameter.ToUnit(lengthUnitGeometry).Value);

            preloads.Add(GetPreLoad(singleBars.Preload));

            materials.Add(MaterialCleanUp(singleBars.BarBundle.Material.ToString()));
          }
        }
      }

      Position.Value = positions.ToArray();
      Diameter.Value = diameters.ToArray();
      BundleCount.Value = bundleCounts.ToArray();
      PreLoad.Value = preloads.ToArray();
      Material.Value = materials.ToArray();
    }

    private static double GetPreLoad(IPreload preLoad) {
      if (preLoad is IPreForce singleBarsPreload) {
        return singleBarsPreload.Force.ToUnit(ContextUnits.Instance.ForceUnit).Value;
      }

      if (preLoad is IPreStrain singleBarsPreload2) {
        return singleBarsPreload2.Strain.ToUnit(ContextUnits.Instance.StrainUnit).Value;
      }

      if (preLoad is IPreStress singleBarsPreload3) {
        return singleBarsPreload3.Stress.ToUnit(ContextUnits.Instance.PressureUnit).Value;
      }

      return 0;
    }

    public static string MaterialCleanUp(string rebarMat) {
      const string namespacePrefix = "Oasys.AdSec.Materials.I";
      rebarMat = rebarMat.Replace(namespacePrefix, string.Empty);
      const string suffix = "_Implementation";
      rebarMat = rebarMat.Replace(suffix, string.Empty);
      return rebarMat;
    }
  }

}
