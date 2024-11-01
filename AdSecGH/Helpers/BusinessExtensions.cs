using System;
using System.Collections.Generic;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace Oasys.GH.Helpers {

  public static class BusinessExtensions {

    private static readonly Dictionary<Type, Func<Attribute, IGH_Param>> ToGhParam
      = new Dictionary<Type, Func<Attribute, IGH_Param>> {
        {
          typeof(DoubleParameter), a => new Param_Number {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
          }
        }, {
          typeof(PointAttribute), a => new Param_GenericObject {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
          }
        },
      };

    private static readonly Dictionary<Type, Func<Attribute, IGH_Goo>> ToGoo
      = new Dictionary<Type, Func<Attribute, IGH_Goo>> {
        {
          typeof(PointAttribute), a => new GH_ObjectWrapper {
            Value = new AdSecPointGoo((a as PointAttribute)?.Value),
          }
        }, {
          typeof(DoubleParameter), a => new GH_Number((a as DoubleParameter).Value)
        },
      };

    public static void SetDefaultValues(this IBusinessComponent businessComponent) {
      foreach (var attribute in businessComponent.GetAllInputAttributes()) {
        if (attribute is IDefault @default) {
          @default.SetDefault();
        }
      }
    }

    public static void SetDefaultValues(this IBusinessComponent businessComponent, GH_Component component) {
      businessComponent.SetDefaultValues();
      foreach (var attribute in businessComponent.GetAllInputAttributes()) {
        int index = component.Params.IndexOfInputParam(attribute.Name);
        var param = component.Params.Input[index];
        var goo = ToGoo[attribute.GetType()](attribute);
        param.AddVolatileData(new GH_Path(0), 0, goo);
      }
    }

    public static void SetOutputValues(
      this IBusinessComponent businessComponent, GH_Component component, IGH_DataAccess dataAccess) {
      foreach (var attribute in businessComponent.GetAllOutputAttributes()) {
        int index = component.Params.IndexOfOutputParam(attribute.Name);
        var goo = ToGoo[attribute.GetType()](attribute);
        dataAccess.SetData(index, goo);
      }
    }

    public static void PopulateInputParams(this IBusinessComponent businessComponent, GH_Component component) {
      RegisterParams(businessComponent.GetAllInputAttributes(), param => component.Params.RegisterInputParam(param));
    }

    private static void RegisterParams(Attribute[] attributesSelector, Action<IGH_Param> action) {
      foreach (var attribute in attributesSelector) {
        var func = ToGhParam[attribute.GetType()];
        var param = func(attribute);
        action(param);
      }
    }

    public static void PopulateOutputParams(this IBusinessComponent businessComponent, GH_Component component) {
      RegisterParams(businessComponent.GetAllOutputAttributes(), param => component.Params.RegisterOutputParam(param));
    }
  }

}
