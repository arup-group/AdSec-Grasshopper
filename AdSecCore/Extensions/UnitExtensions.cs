using System.Linq;

using AdSecCore.Functions;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore {
  public static class UnitExtensions {

    public static string GetUnit(this LengthUnit lengthUnitGeometry) {
      IQuantity length = new Length(0, lengthUnitGeometry);
      return string.Concat(length.ToString().Where(char.IsLetter));
    }

    public static string NameWithUnits(this Attribute attribute, LengthUnit unit) {
      return NameWithUnits(attribute.Name, unit);
    }

    public static string NameWithUnits(string name, LengthUnit unit) {
      return $"{name} [{unit.GetUnit()}]";
    }
  }
}
