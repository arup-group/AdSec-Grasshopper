﻿using System;
using System.Collections.Generic;

namespace AdSecGHCore.Helpers {
  public class DoubleComparer : IEqualityComparer<double> {
    private readonly double _epsilon = 0.01; //default accuracy in %
    private readonly bool _margin;

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
}