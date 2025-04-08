using System;
using System.Linq;

using AdSecCore;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecRebarGroupGoo : GH_Goo<AdSecRebarGroup> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Rebar Group";
    internal ICover Cover {
      get => Value.Cover;
      set => Value.Cover = value;
    }

    public AdSecRebarGroupGoo() { }

    public AdSecRebarGroupGoo(IGroup group) {
      Value = new AdSecRebarGroup(group);
    }

    public AdSecRebarGroupGoo(AdSecRebarGroup goo) {
      Value = goo ?? new AdSecRebarGroup();
    }

    public override bool CastFrom(object source) {
      if (!(source is AdSecRebarGroup group)) {
        return false;
      }

      Value = group;
      return true;
    }

    public override bool CastTo<Q>(ref Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecRebarGroup))) {
        target = Value == null ? default : (Q)(object)Value.Duplicate();
        return true;
      }

      target = default;
      return false;
    }

    public override IGH_Goo Duplicate() {
      var dup = new AdSecRebarGroupGoo(Value);
      if (Value.Cover != null) {
        dup.Value.Cover = ICover.Create(Value.Cover.UniformCover);
      }

      return dup;
    }

    public override string ToString() {
      var doubleComparer = new DoubleComparer();
      string toString = string.Empty;
      string preLoad = string.Empty;
      switch (Value.Group) {
        case ILongitudinalGroup longitudinal: {
            if (longitudinal.Preload != null) {
              preLoad = GetPreLoad(longitudinal, doubleComparer, preLoad);
            }

            toString = GetGroupDescription();

            break;
          }

        case ILinkGroup _:
          toString = $"Link, {Value.Cover.UniformCover.ToUnit(DefaultUnits.LengthUnitGeometry)} cover";
          break;
      }

      return $"AdSec {TypeName} {{{toString}{preLoad}}}";
    }

    private string GetGroupDescription() {
      string toString;
      switch (Value.Group) {
        case ITemplateGroup _:
          toString = $"Template Group, {Value.Cover.UniformCover.ToUnit(DefaultUnits.LengthUnitGeometry)} cover";
          break;
        case IPerimeterGroup _:
          toString = $"Perimeter Group, {Value.Cover.UniformCover.ToUnit(DefaultUnits.LengthUnitGeometry)} cover";
          break;
        case IArcGroup _:
          toString = "Arc Type Layout";
          break;
        case ICircleGroup _:
          toString = "Circle Type Layout";
          break;
        case ILineGroup _:
          toString = "Line Type Layout";
          break;
        case ISingleBars _:
          toString = "SingleBars Type Layout";
          break;
        default:
          toString = $"Type: {Value?.Group.GetType()} not supported!";
          break;
      }

      return toString;
    }

    private static string GetPreLoad(ILongitudinalGroup longitudinal, DoubleComparer doubleComparer, string preLoad) {
      switch (longitudinal.Preload) {
        case IPreForce force when doubleComparer.Equals(force.Force.Value, 0): {
            preLoad = GetForcePreLoad(force);
            break;
          }
        case IPreStress stress when doubleComparer.Equals(stress.Stress.Value, 0): {
            preLoad = GetStressPreLoad(stress);
            break;
          }
        case IPreStrain strain when doubleComparer.Equals(strain.Strain.Value, 0): {
            preLoad = GetStrainPreLoad(strain);
            break;
          }
      }

      return preLoad;
    }

    private static string GetStrainPreLoad(IPreStrain strain) {
      string unitstrainAbbreviation = Strain.GetAbbreviation(DefaultUnits.MaterialStrainUnit);
      return $", {Math.Round(strain.Strain.As(DefaultUnits.MaterialStrainUnit), 4)}{unitstrainAbbreviation} prestress";
    }

    private static string GetStressPreLoad(IPreStress stress) {
      IQuantity quantityStress = new Pressure(0, DefaultUnits.StressUnitResult);
      string unitstressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
      return $", {Math.Round(stress.Stress.As(DefaultUnits.StressUnitResult), 4)}{unitstressAbbreviation} prestress";
    }

    private static string GetForcePreLoad(IPreForce force) {
      IQuantity quantityForce = new Force(0, DefaultUnits.ForceUnit);
      string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
      return $", {Math.Round(force.Force.As(DefaultUnits.ForceUnit), 4)}{unitforceAbbreviation} prestress";
    }
  }
}
