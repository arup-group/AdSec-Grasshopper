using System;
using System.Collections.Generic;

namespace AdSecGH.Helpers {
  internal class Diagram {
    /// <summary>
    /// Axis scales a min/max value appropriately for the purpose of graphs
    /// <remarks>Code taken and modified from http://peltiertech.com/WordPress/calculate-nice-axis-scales-in-excel-vba/</remarks>
    /// </summary>
    internal readonly struct GridAxis {
      public float[] MajorRange {
        get {
          float[] res = new float[major_count + 1];
          for (int i = 0; i < res.Length; i++) {
            res[i] = min_value + (major_step * i);
          }
          return res;
        }
      }
      public float[] MinorRange {
        get {
          float[] res = new float[minor_count + 1];
          for (int i = 0; i < res.Length; i++) {
            res[i] = min_value + (minor_step * i);
          }
          return res;
        }
      }
      public readonly int major_count;
      public readonly float major_step;
      public readonly float max_value;
      public readonly float min_value;
      public readonly int minor_count;
      public readonly float minor_step;

      /// <summary>
      /// Initialize Axis from range of values.
      /// </summary>
      /// <param name="x_min">Low end of range to be included</param>
      /// <param name="x_max">High end of range to be included</param>
      internal GridAxis(float x_min, float x_max) {
        //Check if the max and min are the same
        if (x_min == x_max) {
          x_max *= 1.01f;
          x_min /= 1.01f;
        }
        //Check if dMax is bigger than dMin - swap them if not
        if (x_max < x_min) {
          (x_max, x_min) = (x_min, x_max);
        }

        //Make dMax a little bigger and dMin a little smaller (by 1% of their difference)
        float delta = (x_max - x_min) / 2;
        float x_mid = (x_max + x_min) / 2;

        x_max = x_mid + (1.01f * delta);
        x_min = x_mid - (1.01f * delta);

        //What if they are both 0?
        if (x_max == 0 && x_min == 0) {
          x_max = 1;
        }

        //This bit rounds the maximum and minimum values to reasonable values
        //to chart.  If not done, the axis numbers will look very silly
        //Find the range of values covered
        double pwr = Math.Log(x_max - x_min) / Math.Log(10);
        double scl = Math.Pow(10, pwr - Math.Floor(pwr));
        //Find the scaling factor
        if (scl > 0 && scl <= 2.5) {
          major_step = 0.2f;
          minor_step = 0.05f;
        } else if (scl > 2.5 && scl < 5) {
          major_step = 0.5f;
          minor_step = 0.1f;
        } else if (scl > 5 && scl < 7.5) {
          major_step = 1f;
          minor_step = 0.2f;
        } else {
          major_step = 2f;
          minor_step = 0.5f;
        }
        major_step = (float)(Math.Pow(10, Math.Floor(pwr)) * major_step);
        minor_step = (float)(Math.Pow(10, Math.Floor(pwr)) * minor_step);
        major_count = (int)Math.Ceiling((x_max - x_min) / major_step);
        minor_count = (int)Math.Ceiling((x_max - x_min) / minor_step);
        int i_1 = (int)Math.Floor(x_min / major_step);
        int i_2 = (int)Math.Ceiling(x_max / major_step);
        min_value = i_1 * major_step;
        max_value = i_2 * major_step;
      }
    }

    internal static float CalcStepSize(float range, float targetSteps) {
      // calculate an initial guess at step size
      float tempStep = range / targetSteps;

      // get the magnitude of the step size
      float mag = (float)Math.Floor(Math.Log10(tempStep));
      float magPow = (float)Math.Pow(10, mag);

      // calculate most significant digit of the new step size
      float magMsd = (int)((tempStep / magPow) + 0.5);

      // promote the MSD to either 1, 2, or 5
      if (magMsd > 5) {
        magMsd = 10;
      } else if (magMsd > 2) {
        magMsd = 5;
      } else if (magMsd > 1) {
        magMsd = 2;
      }

      return magMsd * magPow;
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
