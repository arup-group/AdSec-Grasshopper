using System;
using System.Collections.Generic;
using System.Reflection;

using AdSecGH.Components;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecParametersReflectionTests {

    public Type[] AllGoos = {
      typeof(AdSecLoadGoo),
      typeof(AdSecDesignCodeGoo),
      typeof(AdSecConcreteCrackCalculationParametersGoo)
    };

    public IGH_Goo[] InstanceOfGoos = {
      new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()))
    };

    [Fact]
    public void CheckInstancesDuplicate() {
      foreach (var goo in InstanceOfGoos) {
        var gooDuplicate = goo.Duplicate();
        Assert.NotNull(gooDuplicate);
        Assert.Equal(goo.ToString(), gooDuplicate.ToString());
      }
    }

    [Fact]
    public void CheckNickname() {
      foreach (var type in AllGoos) {
        var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.True(!IsNullOrEmptyOrWhitespace(value), $"Failed for {type}");
      }
    }

    private static bool IsNullOrEmptyOrWhitespace(string value) {
      return string.IsNullOrWhiteSpace(value) || value == string.Empty;
    }

    [Fact]
    void CheckName() {
      foreach (var type in AllGoos) {
        var property = type.GetProperty("Name", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.True(!string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }

    [Fact]
    void CheckDescription() {
      foreach (var type in AllGoos) {
        var property = type.GetProperty("Description", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.True(!string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }
  }
}
