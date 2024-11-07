using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Parameters;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace Oasys.GH.Helpers {

  public class IAdSecSectionParameter : ParameterAttribute<AdSecSectionGoo> { }

  public class AdSecPointArrayParameter : BaseArrayParameter<AdSecPointGoo> { }
  public class AdSecMaterialArrayParam : BaseArrayParameter<AdSecMaterialGoo> { }

  public static class BusinessExtensions {

    private static readonly Dictionary<Type, Func<Attribute, IGH_Param>> ToGhParam
      = new Dictionary<Type, Func<Attribute, IGH_Param>> {
        {
          typeof(DoubleParameter), a => new Param_Number {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(DoubleArrayParameter), a => new Param_Number {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(IAdSecSectionParameter), a => new AdSecSectionParameter {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(AdSecPointArrayParameter), a => new Param_GenericObject {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(AdSecMaterialArrayParam), a => new AdSecMaterialParameter {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(IntegerArrayParameter), a => new Param_Integer {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        },
      };

    private static readonly Dictionary<Type, Func<Attribute, object>> ToGoo
      = new Dictionary<Type, Func<Attribute, object>> {
        {
          typeof(DoubleParameter), a => new GH_Number((a as DoubleParameter).Value)
        }, {
          typeof(DoubleArrayParameter), a => (a as DoubleArrayParameter).Value
        }, {
          typeof(IAdSecSectionParameter), a => (a as IAdSecSectionParameter).Value
        }, {
          typeof(AdSecPointArrayParameter), a => {
            var points = (a as AdSecPointArrayParameter).Value;
            return points?.ToList();
          }
        }, {
          typeof(AdSecMaterialArrayParam), a => {
            var materials = (a as AdSecMaterialArrayParam).Value;
            return materials?.ToList();
          }
        }, {
          typeof(IntegerArrayParameter), a => (a as IntegerArrayParameter).Value
        },
        // }
      };

    public static GH_ParamAccess GetAccess(this Attribute attribute) {
      var access = (attribute as IAccessible).Access;
      switch (access) {
        case Access.Item: return GH_ParamAccess.item;
        case Access.List: return GH_ParamAccess.list;
        default: throw new ArgumentOutOfRangeException();
      }
    }

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
        if (ToGoo.ContainsKey(attribute.GetType())) {
          var param = component.Params.GetInputParam(attribute.Name);
          object goo = ToGoo[attribute.GetType()](attribute);
          if (param.Access == GH_ParamAccess.item) {
            param.AddVolatileData(new GH_Path(0), 0, goo);
          } else {
            param.AddVolatileDataList(new GH_Path(0), goo as IEnumerable);
          }
        }
      }
    }

    public static void SetOutputValues(
      this IBusinessComponent businessComponent, GH_Component component, IGH_DataAccess dataAccess) {
      foreach (var attribute in businessComponent.GetAllOutputAttributes()) {
        if (ToGoo.ContainsKey(attribute.GetType())) {
          int index = component.Params.IndexOfOutputParam(attribute.Name);
          object goo = ToGoo[attribute.GetType()](attribute);
          dataAccess.SetData(index, goo);
        }
      }
    }

    public static void PopulateInputParams(this IBusinessComponent businessComponent, GH_Component component) {
      RegisterParams(businessComponent.GetAllInputAttributes(), param => component.Params.RegisterInputParam(param));
    }

    private static void RegisterParams(Attribute[] attributesSelector, Action<IGH_Param> action) {
      foreach (var attribute in attributesSelector) {
        if (ToGhParam.ContainsKey(attribute.GetType())) {
          var func = ToGhParam[attribute.GetType()];
          var param = func(attribute);
          action(param);
        }
      }
    }

    public static void PopulateOutputParams(this IBusinessComponent businessComponent, GH_Component component) {
      RegisterParams(businessComponent.GetAllOutputAttributes(), param => component.Params.RegisterOutputParam(param));
    }
  }

}
