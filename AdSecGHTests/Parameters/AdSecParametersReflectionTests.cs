using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OasysGH.Parameters;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecParametersReflectionTests {

    [Fact]
    public void CheckNotNull() {
      var derivedTypes = GetTypesOf(typeof(GH_OasysGoo<>));

      foreach (var type in derivedTypes) {
        var instance = Activator.CreateInstance(type);
        Assert.NotNull(instance);
      }
    }

    [Fact]
    public void CheckDuplicateGeometry() {
      var derivedTypes = GetTypesOf(typeof(GH_OasysGeometricGoo<>));

      foreach (var type in derivedTypes) {
        dynamic instance = Activator.CreateInstance(type);
        var duplicate = instance.DuplicateGeometry();
        Assert.NotNull(duplicate);
        Assert.Equal(instance, duplicate);
      }
    }

    private static List<Type> GetTypesOf(Type type) {
      // Get only the "AdSecGh" assembly
      var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "AdSecGh");

      List<Type> derivedTypes = new List<Type>();

      foreach (var assembly in assemblies) {
        try {
          var types = assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && t != type && !t.IsAbstract).ToList();

          derivedTypes.AddRange(types);
        } catch (ReflectionTypeLoadException ex) {
          Console.WriteLine($"Error loading types from assembly: {assembly.FullName}");
          foreach (var loaderException in ex.LoaderExceptions) {
            Console.WriteLine(loaderException.Message);
          }
        }
      }

      return derivedTypes;
    }
  }
}
