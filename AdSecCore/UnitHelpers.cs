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
        if (double.TryParse(goo.ToString(), out double result)) {
          if (Quantity.TryFrom(result, units, out var quantity)) {
            return (T)quantity;
          }
        } else if (Quantity.TryParse(typeof(T), txt, out var length)) {
          return (T)length;
        }
      }

      throw new NotImplementedException();
    }
  }
}
