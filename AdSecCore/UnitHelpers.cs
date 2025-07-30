
using System;

using OasysUnits;

namespace AdSecCore {
  public static class UnitHelpers {

    public static T ParseToQuantity<T>(object goo, Enum units) {
      if (goo is int intNumber) {
        if (Quantity.TryFrom(intNumber, units, out var quantity)) {
          return (T)quantity;
        }
      } else if (goo is double number) {
        if (Quantity.TryFrom(number, units, out var quantity)) {
          return (T)quantity;
        }
      } else if (goo is string txt) {
        if (double.TryParse(goo.ToString(), out double numberFromText)) {
          if (Quantity.TryFrom(numberFromText, units, out var quantity)) {
            return (T)quantity;
          }
        } else if (Quantity.TryParse(typeof(T), txt, out var quantity)) {
          return (T)quantity;
        }
      } else if (Quantity.TryFrom(0, units, out var quantityObject) && goo is IQuantity quantity && quantity.GetType() == quantityObject.GetType()) {
        return (T)quantity;
      }
      throw new InvalidCastException($"Could not parse the input {goo} to the desired quantity {units}.");
    }
  }
}
