using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecGH.Components;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH.Components;

using Xunit;

namespace AdSecGHTests.Helpers {

  public static class ModFactory {
    public static Type[] GetAllAssemblyTypes(string assemblyName = "AdSec.dll", string nameSpace = "") {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();

      foreach (var item in assemblies) {
        Console.WriteLine(item);
        var other = item.GetLoadedModules();
        other.ToList().ForEach(m => Console.WriteLine(m));
      }

      var assembly = assemblies.FirstOrDefault(x => x.ManifestModule.Name.Equals(assemblyName)
        || x.ManifestModule.ScopeName.Equals(assemblyName));

      var classes = assembly.GetTypes();
      if (nameSpace != string.Empty) {
        classes = classes.Where(x => x.Namespace == nameSpace).ToArray();
      }

      return classes;
    }

    public static IEnumerable<Type> ImplementsInterface(this Type[] collection, Type[] interfaces) {
      return collection.Where(x => x.IsClass && !x.IsAbstract && interfaces.All(y => y.IsAssignableFrom(x)));
    }

    public static IEnumerable<Type> GetAllInterfaceTypes(Type[] interfaces) {
      var allAssemblyTypes = GetAllAssemblyTypes();
      return allAssemblyTypes.ImplementsInterface(interfaces);
    }

    public static IEnumerable<Type> GetAllInterfaceTypes() {
      return GetAllInterfaceTypes(new[] {
        typeof(GH_Component),
        typeof(GH_OasysComponent),
      });
    }

    public static GH_Component[] CreateInstancesWithCreateInterface() {
      var allInterfaceTypes = GetAllInterfaceTypes();
      var components = allInterfaceTypes.Select(Activator.CreateInstance).Select(x => x as GH_Component).ToArray();
      return components;
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class DropToCanvasTests {
    //[Fact]
    //public void TestAllComponents() {
    //  var instances = ModFactory.CreateInstancesWithCreateInterface();
    //  foreach (var instance in instances) {
    //    Assert.NotNull(instance);
    //  }
    //}

    [Fact]
    public void TestAllComponentsByType() {
      // Get All Assemblies
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      // Get The One for AdSecGH
      var assembly = assemblies.FirstOrDefault(x => x.ManifestModule.Name.Equals("AdSecGH.dll")
        || x.ManifestModule.ScopeName.Equals("AdSecGH.dll"));
      // Get All Types matching GH_Component or GH_OasysDropdownComponent
      var types = assembly.GetTypes().Where(x
        => x.IsClass && !x.IsAbstract && typeof(GH_OasysDropDownComponent).IsAssignableFrom(x)
        && x != typeof(CreateDesignCode) && x != typeof(DummyOasysDropdown)).ToArray();
      foreach (var type in types) {
        var instance = (GH_Component)Activator.CreateInstance(type);
        instance.ExpireSolution(true);
        Assert.True(instance.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count == 0);
      }
    }
  }

  public class Duplicates {

    public static bool AreEqual(object objA, object objB, bool excludeGuid = false) {
      if (!(excludeGuid && objA.Equals(typeof(Guid)))) {
        Assert.Equal(objA.ToString(), objB.ToString());
      }

      var typeA = objA.GetType();
      var typeB = objB.GetType();

      var propertyInfoA = typeA.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var propertyInfoB = typeB.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

      for (int i = 0; i < propertyInfoA.Length; i++) {
        var propertyA = propertyInfoA[i];
        var propertyB = propertyInfoB[i];

        if (!propertyA.CanWrite && !propertyB.CanWrite) {
          continue;
        }

        if (!propertyA.CanWrite || !propertyB.CanWrite) {
          Assert.Equal(objA, objB);
        }

        object objPropertyValueA;
        object objPropertyValueB;
        var propertyTypeA = propertyA.PropertyType;
        var propertyTypeB = propertyB.PropertyType;

        try {
          objPropertyValueA = propertyA.GetValue(objA, null);
          objPropertyValueB = propertyB.GetValue(objB, null);

          // check wether property is an interface
          if (propertyTypeA.IsInterface) {
            if (objPropertyValueA != null) {
              propertyTypeA = objPropertyValueA.GetType();
            }
          }

          if (propertyTypeB.IsInterface) {
            if (objPropertyValueB != null) {
              propertyTypeB = objPropertyValueB.GetType();
            }
          }

          // check wether property is an enumerable
          if (typeof(IEnumerable).IsAssignableFrom(propertyTypeA) && !typeof(string).IsAssignableFrom(propertyTypeA)) {
            if (typeof(IEnumerable).IsAssignableFrom(propertyTypeB)
              && !typeof(string).IsAssignableFrom(propertyTypeB)) {
              if (objPropertyValueA == null || objPropertyValueB == null) {
                Assert.Equal(objPropertyValueA, objPropertyValueB);
              } else {
                var enumerableA = ((IEnumerable)objPropertyValueA).Cast<object>();
                var enumerableB = ((IEnumerable)objPropertyValueB).Cast<object>();

                Type enumrableTypeA = null;
                Type enumrableTypeB = null;
                if (enumerableA.GetType().GetGenericArguments().Length > 0) {
                  enumrableTypeA = enumerableA.GetType().GetGenericArguments()[0];
                }

                if (enumerableB.GetType().GetGenericArguments().Length > 0) {
                  enumrableTypeB = enumerableB.GetType().GetGenericArguments()[0];
                }

                Assert.Equal(enumrableTypeA, enumrableTypeB);

                // if type is a struct, we have to check the actual list items
                // this will fail if list is actually of type "System.Object"..
                if (enumrableTypeA.ToString() is "System.Object") {
                  if (enumerableA.Any()) {
                    enumrableTypeA = enumerableA.First().GetType();
                  } else {
                    continue; // can´t get type of struct in empty list?
                  }
                }

                if (enumrableTypeB.ToString() is "System.Object") {
                  if (enumerableB.Any()) {
                    enumrableTypeB = enumerableB.First().GetType();
                  } else {
                    continue; // can´t get type of struct in empty list?
                  }
                }

                var genericListTypeA = typeof(List<>).MakeGenericType(enumrableTypeA);
                var genericListTypeB = typeof(List<>).MakeGenericType(enumrableTypeB);
                Assert.Equal(genericListTypeA, genericListTypeB);

                var enumeratorB = enumerableB.GetEnumerator();

                using (var enumeratorA = enumerableA.GetEnumerator()) {
                  while (enumeratorA.MoveNext()) {
                    Assert.True(enumeratorB.MoveNext());
                    AreEqual(enumeratorA.Current, enumeratorB.Current);
                  }
                }
              }
            } else {
              Assert.Equal(objPropertyValueA, objPropertyValueB);
            }
          }
          // check whether property type is value type, enum or string type
          else if (propertyTypeA.IsValueType || propertyTypeA.IsEnum || propertyTypeA.Equals(typeof(string))) {
            if (excludeGuid && propertyTypeA.Equals(typeof(Guid))) {
              continue;
            }

            Assert.Equal(objPropertyValueA, objPropertyValueB);
          } else if (objPropertyValueA == null || objPropertyValueB == null) {
            Assert.Equal(objPropertyValueA, objPropertyValueB);
          } else
          // property type is object/complex type, so need to recursively call this method until the end of the tree is reached
          {
            AreEqual(objPropertyValueA, objPropertyValueB, excludeGuid);
          }
        } catch (TargetParameterCountException) {
          propertyTypeA = propertyA.PropertyType;
        }
      }

      return true;
    }
  }
}
