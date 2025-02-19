using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using OasysGH.Parameters;

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

    public static List<object[]> AllGoos = new List<object[]> {
      new object[] { typeof(AdSecLoadGoo) },
      new object[] { typeof(AdSecDesignCodeGoo) },
      new object[] { typeof(AdSecConcreteCrackCalculationParametersGoo) }
    };

    [Theory]
    [MemberData(nameof(AllGoos))]
    public void CheckNickName(Type type) {
      var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
      string value = property?.GetValue(null) as string;

      Assert.NotNull(value);
      Assert.True(value.Split(' ').Length == 1);
    }

    // [Theory]
    // [InlineData(typeof(AdSecLoadGoo))]
    // [InlineData(typeof(AdSecDesignCodeGoo))]
    // [InlineData(typeof(AdSecConcreteCrackCalculationParametersGoo))]
    // public void CheckNickName(Type type) {
    //   var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
    //   string value = property.GetValue(null) as string;
    //   Assert.NotNull(value);
    //   Assert.True(value.Split(' ').Length == 1);
    // }

    // [Fact]
    // public void CheckNotNull2() {
    //   var derivedTypes = GetTypesOf(typeof(IGH_GeometricGoo));
    //
    //   foreach (var type in derivedTypes) {
    //     var instance = Activator.CreateInstance(type);
    //     Assert.NotNull(instance);
    //   }
    // }
    //
    // [Theory]
    // [InlineData(typeof(AdSecRebarGroupGoo))]
    // public void TestTypeNotNull(Type type) {
    //   var instance = Activator.CreateInstance(type);
    //   Assert.NotNull(instance);
    //   // var duplicate = instance.DuplicateGeometry();
    //   // Assert.Equal(instance, duplicate);
    // }
    //
    // [Theory]
    // [InlineData(typeof(AdSecRebarGroupGoo))]
    // public void TestTDuplivateGeometry(Type type) {
    //   var instance = Activator.CreateInstance(type);
    //   Assert.NotNull(instance);
    //   // var duplicate = instance.DuplicateGeometry();
    //   // Assert.Equal(instance, duplicate);
    // }
    //
    // private static List<Type> GetTypesOf(Type type) {
    //   // Get only the "AdSecGh" assembly
    //   var assembly = Assembly.GetAssembly(typeof(AdSecRebarGroupGoo)); // AdSecGHInfo
    //
    //   List<Type> derivedTypes = new List<Type>();
    //
    //   try {
    //     var types1 = assembly.GetTypes();
    //     var types = types1.Where(t => type.IsAssignableFrom(t) && // Ensure it inherits or implements 'type'
    //         t != type && // Exclude the base type itself
    //         !t.IsAbstract //&&             // Exclude abstract types
    //       //t.GetConstructor(Type.EmptyTypes) != null // Ensure it has a public parameterless constructor
    //     ).ToList();
    //
    //     derivedTypes.AddRange(types);
    //   } catch (ReflectionTypeLoadException ex) {
    //     Console.WriteLine($"Error loading types from assembly: {assembly.FullName}");
    //     foreach (var loaderException in ex.LoaderExceptions) {
    //       Console.WriteLine(loaderException.Message);
    //     }
    //   }
    //
    //   return derivedTypes;
    // }

  }
}
