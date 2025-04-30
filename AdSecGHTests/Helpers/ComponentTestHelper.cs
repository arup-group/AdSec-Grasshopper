using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  public static class ComponentTestHelper {
    public static object GetOutput(GH_Component component, int index = 0, int branch = 0, int item = 0, bool forceUpdate = false) {
      if (forceUpdate || component.Params.Output[index].VolatileDataCount == 0) {
        component.ExpireSolution(true);
        component.Params.Output[index].CollectData();
      }
      return component.Params.Output[index].VolatileData.get_Branch(branch)[item];
    }

    public static void CheckOutputIsNotNull(GH_Component component) {
      for (int i = 0; i < component.Params.Output.Count; i++) {
        var output = ComponentTestHelper.GetOutput(component, i);
        Assert.NotNull(output);
      }
    }

    public static void SetInput(GH_Component component, int value, int index = 0) {
      var input = new Param_Integer();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Integer(value));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, string text_input, int index = 0) {
      var input = new Param_String();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_String(text_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, bool bool_input, int index = 0) {
      var input = new Param_Boolean();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Boolean(bool_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, double number_input, int index = 0) {
      var input = new Param_Number();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Number(number_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, object generic_input, int index = 0) {
      var input = new Param_GenericObject();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_ObjectWrapper(generic_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, GH_Structure<IGH_Goo> tree, int index = 0) {
      var input = new Param_GenericObject();
      input.CreateAttributes();
      input.SetPersistentData(tree);
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, List<object> generic_input, int index = 0) {
      var input = new Param_GenericObject();
      input.CreateAttributes();
      input.Access = GH_ParamAccess.list;
      foreach (object obj in generic_input) {
        input.PersistentData.Append(new GH_ObjectWrapper(obj));
      }
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, Point3d point_input, int index = 0) {
      var input = new Param_Point();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Point(point_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, Curve curve_input, int index = 0) {
      var input = new Param_Curve();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Curve(curve_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, Brep brep_input, int index = 0) {
      var input = new Param_Brep();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Brep(brep_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetInput(GH_Component component, Mesh mesh_input, int index = 0) {
      var input = new Param_Mesh();
      input.CreateAttributes();
      input.PersistentData.Append(new GH_Mesh(mesh_input));
      component.Params.Input[index].AddSource(input);
    }

    public static void SetListInput(GH_Component component, List<object> objs, int index = 0) {
      var input = new Param_GenericObject();
      input.CreateAttributes();
      input.Access = GH_ParamAccess.list;
      foreach (object obj in objs) {
        input.PersistentData.Append(new GH_ObjectWrapper(obj));
      }
      component.Params.Input[index].AddSource(input);
    }

    public static void ComputeData(GH_Component component) {
      component.ExpireSolution(true);
      component.CollectData();
      component.ComputeData();
    }
  }
}
