using System;
using System.Collections.Generic;

namespace AdSecGH.Helpers {
  public class DoubleComparer : IEqualityComparer<double> {
    private double _epsilon = 0.01; //default accuracy in %
    private bool _margin = false;

    public DoubleComparer(double epsilon = 0.01, bool useEpsilonAsMargin = false) {
      _epsilon = epsilon;
      _margin = useEpsilonAsMargin;
    }

    public bool Equals(double x, double y) {
      x = Math.Round(x, 6);
      y = Math.Round(y, 6);

      if (x == y) {
        return true;
      }

      if (_margin) {
        if (Math.Abs(x - y) < _epsilon) {
          return true;
        }
      } else {
        double error = Math.Abs(x - y) / (x + y) * 0.5;
        return error < _epsilon;
      }

      return false;
    }

    public int GetHashCode(double obj) {
      return obj.GetHashCode();
    }

  }

  internal class Result {

    internal static double RoundToSignificantDigits(double d, int digits) {
      if (d == 0.0) {
        return 0.0;
      } else {
        double leftSideNumbers = Math.Floor(Math.Log10(Math.Abs(d))) + 1;
        double scale = Math.Pow(10, leftSideNumbers);
        double result = scale * Math.Round(d / scale, digits, MidpointRounding.AwayFromZero);

        // Clean possible precision error.
        if ((int)leftSideNumbers >= digits) {
          return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        } else {
          if (Math.Abs(digits - (int)leftSideNumbers) > 15) {
            return 0.0;
          }

          return Math.Round(result, digits - (int)leftSideNumbers, MidpointRounding.AwayFromZero);
        }
      }
    }

    internal static List<double> SmartRounder(double max, double min) {
      // find the biggest abs value of max and min
      double val = Math.Max(Math.Abs(max), Math.Abs(min));

      // round that with 4 significant digits
      double scale = RoundToSignificantDigits(val, 4);

      // list to hold output values
      var roundedvals = new List<double>();

      // do max
      if (max == 0) {
        roundedvals.Add(0);
      } else {
        double tempmax = scale * Math.Round(max / scale, 4);
        tempmax = Math.Ceiling(tempmax * 1000) / 1000;
        roundedvals.Add(tempmax);
      }

      // do min
      if (min == 0) {
        roundedvals.Add(0);
      } else {
        double tempmin = scale * Math.Round(min / scale, 4);
        tempmin = Math.Floor(tempmin * 1000) / 1000;
        roundedvals.Add(tempmin);
      }

      return roundedvals;
    }
  }
}
