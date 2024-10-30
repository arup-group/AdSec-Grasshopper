using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;

using OasysGH.Components;

namespace AdSecGHTests.Helpers {
  public static class ModFactory {
    public static Type[] GetAllAssemblyTypes(string assemblyName = "AdSecGH.dll", string nameSpace = "") {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
        typeof(GH_OasysDropDownComponent),
      });
    }

    public static GH_Component[] CreateInstancesWithCreateInterface() {
      return GetInstances().Select(x => x as GH_Component).ToArray();
    }

    private static IEnumerable<object> GetInstances() {
      var allInterfaceTypes = GetAllInterfaceTypes();
      var objects = allInterfaceTypes.Select(Activator.CreateInstance);
      return objects;
    }
  }
}
