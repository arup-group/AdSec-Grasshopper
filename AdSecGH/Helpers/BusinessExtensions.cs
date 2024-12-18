﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using OasysGH.Units;

using OasysUnits;

using Attribute = AdSecCore.Functions.Attribute;

namespace Oasys.GH.Helpers {

  public class AdSecSectionParameter : ParameterAttribute<AdSecSectionGoo> { }
  public class AdSecPointArrayParameter : BaseArrayParameter<AdSecPointGoo> { }
  public class AdSecPointParameter : ParameterAttribute<AdSecPointGoo> { }
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
          typeof(AdSecSectionParameter), a => new AdSecGH.Parameters.AdSecSectionParameter {
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
          typeof(AdSecPointParameter), a => new Param_GenericObject {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(PointParameter), a => new Param_GenericObject {
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
        }, {
          typeof(StringArrayParam), a => new Param_String {
            Name = a.Name,
            NickName = a.NickName,
            Description = a.Description,
            Access = GetAccess(a),
          }
        }, {
          typeof(LengthParameter), a => new Param_GenericObject {
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
          typeof(AdSecSectionParameter), a => (a as AdSecSectionParameter).Value
        }, {
          typeof(AdSecPointArrayParameter), a => {
            var points = (a as AdSecPointArrayParameter).Value;
            return points?.ToList();
          }
        }, {
          typeof(AdSecPointParameter), a => (a as AdSecPointParameter).Value
        }, {
          typeof(AdSecMaterialArrayParam), a => {
            var materials = (a as AdSecMaterialArrayParam).Value;
            return materials?.ToList();
          }
        }, {
          typeof(IntegerArrayParameter), a => (a as IntegerArrayParameter).Value
        }, {
          typeof(StringArrayParam), a => (a as StringArrayParam).Value
        },
      };

    private static readonly Dictionary<Type, Func<object, object>> GooToParam
      = new Dictionary<Type, Func<object, object>> {
        {
          typeof(LengthParameter), goo => {
            return UnitHelpers.ParseToQuantity<Length>(goo, DefaultUnits.LengthUnitGeometry);
          }
        }, {
          typeof(AdSecSectionParameter), goo => {
            dynamic gooDynamic = goo;
            return new AdSecSectionGoo(gooDynamic);
          }
        }, {
          typeof(DoubleParameter), goo => {
            if (goo is double value) {
              return value;
            }

            return null;
          }
        },
      };

    public static void UpdateProperties(this IFunction BusinessComponent, GH_Component component) {
      component.Name = BusinessComponent.Metadata.Name;
      component.NickName = BusinessComponent.Metadata.NickName;
      component.Description = BusinessComponent.Metadata.Description;
      component.Category = BusinessComponent.Organisation.Category;
      component.SubCategory = BusinessComponent.Organisation.SubCategory;
    }

    public static GH_ParamAccess GetAccess(this Attribute attribute) {
      var access = (attribute as IAccessible).Access;
      switch (access) {
        case Access.Item: return GH_ParamAccess.item;
        case Access.List: return GH_ParamAccess.list;
        default: throw new ArgumentOutOfRangeException();
      }
    }

    public static void SetDefaultValues(this IFunction function) {
      foreach (var attribute in function.GetAllInputAttributes()) {
        if (attribute is IDefault @default) {
          @default.SetDefault();
        }
      }
    }

    public static void SetDefaultValues(this IFunction function, GH_Component component) {
      function.SetDefaultValues();
      foreach (var attribute in function.GetAllInputAttributes().Where(x => ToGoo.ContainsKey(x.GetType()))) {
        var param = component.Params.GetInputParam(attribute.Name);
        object goo = ToGoo[attribute.GetType()](attribute);
        if (param.Access == GH_ParamAccess.item) {
          param.AddVolatileData(new GH_Path(0), 0, goo);
        } else {
          param.AddVolatileDataList(new GH_Path(0), goo as IEnumerable);
        }
      }
    }

    public static void UpdateInputValues(this IFunction function, GH_Component component, IGH_DataAccess dataAccess) {
      foreach (var attribute in function.GetAllInputAttributes()) {
        int index = component.Params.IndexOfInputParam(attribute.Name);
        if (attribute.GetAccess() == GH_ParamAccess.item) {
          dynamic inputs = null;
          if (dataAccess.GetData(index, ref inputs)) {
            dynamic valueBasedParameter = attribute;
            dynamic newValue = GooToParam[attribute.GetType()](inputs.Value);
            valueBasedParameter.Value = newValue;
          }
        }
      }
    }

    public static void SetOutputValues(this IFunction function, GH_Component component, IGH_DataAccess dataAccess) {
      foreach (var attribute in function.GetAllOutputAttributes().Where(x => ToGoo.ContainsKey(x.GetType()))) {
        int index = component.Params.IndexOfOutputParam(attribute.Name);
        dynamic goo = ToGoo[attribute.GetType()](attribute);
        if (attribute.GetAccess() == GH_ParamAccess.item) {
          dataAccess.SetData(index, goo);
        } else {
          dataAccess.SetDataList(index, goo);
        }
      }
    }

    public static void PopulateInputParams(this IFunction function, GH_Component component) {
      RegisterParams(function.GetAllInputAttributes(), param => component.Params.RegisterInputParam(param));
    }

    private static void RegisterParams(Attribute[] attributesSelector, Action<IGH_Param> action) {
      foreach (var attribute in attributesSelector.Where(x => ToGhParam.ContainsKey(x.GetType()))) {
        var func = ToGhParam[attribute.GetType()];
        var param = func(attribute);
        action(param);
      }
    }

    public static void PopulateOutputParams(this IFunction function, GH_Component component) {
      RegisterParams(function.GetAllOutputAttributes(), param => component.Params.RegisterOutputParam(param));
    }
  }

}
