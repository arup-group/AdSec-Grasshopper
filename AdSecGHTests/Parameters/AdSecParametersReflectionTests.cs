using System;
using System.Collections.Generic;
using System.Reflection;

using AdSecGH.Parameters;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecParametersReflectionTests {

    // private IGH_Goo[] goos;

    // AdSecParametersReflectionTests() {
    //   goos = new[] {
    //     new AdSecRebarGroupGoo(),
    //     // new AdSecConcreteCrackCalculationParametersGoo(),
    //     // new AdSecCrackGoo(),
    //   };
    // }

    // [Fact]
    // public void CheckNotNull() {
    //   var derivedTypes = GetTypesOf(typeof(IGH_Goo));
    //
    //   foreach (var type in derivedTypes) {
    //     var instance = Activator.CreateInstance(type);
    //     Assert.NotNull(instance);
    //   }
    // }

    // public static List<object[]> AllGoos = new List<object[]> {
    //   new object[] { typeof(AdSecLoadGoo) },
    //   new object[] { typeof(AdSecDesignCodeGoo) },
    //   new object[] { typeof(AdSecConcreteCrackCalculationParametersGoo) }
    // };
    //
    // [Theory]
    // [MemberData(nameof(AllGoos))]
    // public void CheckNickName(Type type) {
    //   var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
    //   string value = property?.GetValue(null) as string;
    //
    //   Assert.NotNull(value);
    //   Assert.True(value.Split(' ').Length == 1);
    // }
    //
    // [Theory]
    // [InlineData(typeof(AdSecLoadGoo))]
    // [InlineData(typeof(AdSecDesignCodeGoo))]
    // [InlineData(typeof(AdSecConcreteCrackCalculationParametersGoo))]
    // public void CheckNickName2(Type type) {
    //   var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
    //   string value = property?.GetValue(null) as string;
    //
    //   Assert.NotNull(value);
    //   Assert.True(value.Split(' ').Length == 1);
    // }

    public Type[] AllGoos = {
      typeof(AdSecLoadGoo),
      typeof(AdSecDesignCodeGoo),
      typeof(AdSecConcreteCrackCalculationParametersGoo)
    };

    [Fact]
    public void CheckNickname() {
      foreach (var type in AllGoos) {
        var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.True(!IsNullOrEmptyOrWhitespace(value), $"Failed for {type}");
      }
    }

    private static bool IsNullOrEmptyOrWhitespace(string value)
    {
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
