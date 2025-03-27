using System;
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

using Microsoft.CSharp.RuntimeBinder;

using Oasys.AdSec;

using OasysGH.Units;

using OasysUnits;

using Rhino.Geometry;

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
          typeof(SubComponentParameter), ParamGenericObject
        }, {
          typeof(SubComponentArrayParameter), ParamGenericObject
        }, {
          typeof(MaterialParameter), ParamGenericObject
        }, {
          typeof(SectionParameter), ParamGenericObject
        }, {
          typeof(AdSecMaterialParameter), ParamGenericObject
        }, {
          typeof(RebarGroupParameter), ParamGenericObject
        }, {
          typeof(ProfileParameter), ParamGenericObject
        }, {
          typeof(DoubleParameter), ParamNumber
        }, {
          typeof(DoubleArrayParameter), ParamNumber
        }, {
          typeof(AdSecSectionParameter), ConfigureParam<AdSecGH.Parameters.AdSecSectionParameter>
        }, {
          typeof(AdSecPointArrayParameter), ParamGenericObject
        }, {
          typeof(AdSecPointParameter), ParamGenericObject
        }, {
          typeof(PointParameter), ParamGenericObject
        }, {
          typeof(AdSecMaterialArrayParam), ConfigureParam<AdSecMaterialParameter>
        }, {
          typeof(IntegerArrayParameter), ParamInteger
        }, {
          typeof(StringParameter), a => {
            var value = a as StringParameter;
            var paramString = ParamString(a);

            if (value.Default != null) {
              paramString.SetPersistentData(value.Default);
            }

            return paramString;
          }
        }, {
          typeof(StringArrayParam), ParamString
        }, {
          typeof(LengthParameter), ParamGenericObject
        },{
          typeof(SectionSolutionParameter), ParamGenericObject
        }, {
          typeof(LoadSurfaceParameter), ParamGenericObject
        }, {
          typeof(LoadParameter), ParamGenericObject
        }, {
          typeof(CrackParameter), ParamGenericObject
        }, {
          typeof(IntegerParameter), ParamInteger
        }, {
          typeof(DeformationParameter), ParamGenericObject
        }, {
          typeof(GenericParameter), ParamGenericObject
        }, {
          typeof(CrackArrayParameter), ParamGenericObject
        }, {
          typeof(SecantStiffnessParameter), ParamGenericObject
        }, {
          typeof(IntervalArrayParameter), ParamGenericObject
        },{
          typeof(NeutralLineParameter), ParamGenericObject
        }
      };

    /// <summary>
    /// For the Outputs
    /// This is use to DA.SetData()
    /// i.e. we will get the value from the Attribute.Value and set it to a Goo object
    /// Since the value is all we need and this is often shared between the "Business" Parameter and the Gh (Goo) object, it can take the form:
    /// { typeof(<ParamType), a => (a as ParamType).Value }
    /// We need to do more work when the Goo, has a more complex constructor in which
    /// case it might be best to create a new constructor, so we can simplify this dictionary and even deprecated later on.
    /// </summary>
    private static readonly Dictionary<Type, Func<Attribute, object>> ToGoo
      = new Dictionary<Type, Func<Attribute, object>> {
        { typeof(SubComponentParameter), a => new AdSecSubComponentGoo((a as SubComponentParameter)?.Value) },
        // { typeof(RebarGroupParameter), a => (a as RebarGroupParameter).Value },
        { typeof(DoubleParameter), a => new GH_Number((a as DoubleParameter).Value) }, {
          typeof(LoadSurfaceParameter),
          a => new AdSecFailureSurfaceGoo((a as LoadSurfaceParameter).Value, Plane.WorldXY)
        },
        { typeof(DoubleArrayParameter), a => (a as DoubleArrayParameter).Value },
        // { typeof(SectionParameter), a => (a as SectionParameter).Value },
        { typeof(AdSecSectionParameter), a => (a as AdSecSectionParameter).Value }, {
          typeof(SectionSolutionParameter), a => {
            var sectionSolutionParameter = (a as SectionSolutionParameter).Value;
            return new AdSecSolutionGoo(sectionSolutionParameter);
          }
        }, {
          typeof(AdSecPointArrayParameter), a => {
            var points = (a as AdSecPointArrayParameter).Value;
            return points?.ToList();
          }
        },
        { typeof(AdSecPointParameter), a => (a as AdSecPointParameter).Value }, {
          typeof(AdSecMaterialArrayParam), a => {
            var materials = (a as AdSecMaterialArrayParam).Value;
            return materials?.ToList();
          }
        },
        { typeof(IntegerArrayParameter), a => (a as IntegerArrayParameter).Value },
        { typeof(StringArrayParam), a => (a as StringArrayParam).Value },
        // { typeof(IntegerParameter), a => (a as IntegerParameter).Value },
        {
          typeof(CrackParameter), a => {
            var crack = (a as CrackParameter).Value;
            return new AdSecCrackGoo(crack);
          }
        }, {
          typeof(LoadParameter), a => {
            var load = (a as LoadParameter).Value;
            return new AdSecLoadGoo(load);
          }
        }, {
          typeof(IntervalArrayParameter), a => {
            var intervals = (a as IntervalArrayParameter).Value;
            var ranges = new List<GH_Interval>();
            foreach (var interval in intervals) {
              ranges.Add(new GH_Interval(new Interval(interval.Item1, interval.Item2)));
            }

            return ranges;
          }
        }, {
          typeof(SecantStiffnessParameter), a => {
            var stiffness = (a as SecantStiffnessParameter).Value;
            return new Vector3d(stiffness.X.As(DefaultUnits.AxialStiffnessUnit),
              stiffness.YY.As(DefaultUnits.BendingStiffnessUnit), stiffness.ZZ.As(DefaultUnits.BendingStiffnessUnit));
          }
        }, {
          typeof(CrackArrayParameter), a => {
            var cracks = (a as CrackArrayParameter).Value;
            var cracksGoo = new List<AdSecCrackGoo>();
            foreach (var crack in cracks) {
              cracksGoo.Add(new AdSecCrackGoo(crack));
            }

            return cracksGoo;
          }
        }, {
          typeof(DeformationParameter), a => {
            var deformation = (a as DeformationParameter).Value;
            return new AdSecDeformationGoo(deformation);
          }
        },{
          typeof(NeutralLineParameter), a => {
            var value = (a as NeutralLineParameter).Value;
            return new AdSecNeutralAxisGoo(value);
          }
        },{
          typeof(LengthParameter), a => {
            return (a as LengthParameter).Value;
          }
        }
      };

    /// <summary>
    /// Setting the Inputs
    /// ***************************************************************************
    /// ******* if the base data is the same, you can skip this completely! *******
    /// ***************************************************************************
    /// This is for grabbing the Values from Grasshopper and feeding the to the Business Input Params
    /// So we often need to convert the Grasshopper object to the Business object
    /// Again we aim to have the data, so the conversion would be simple
    /// example: { typeof(ParamType), goo => new DataType(goo) }
    /// This is the place where we might need to call AdSecInput to account for different inputed types
    /// like in the case of SubComponent, that can also accept a Section Type
    /// </summary>
    private static readonly Dictionary<Type, Func<object, object>> GooToParam
      = new Dictionary<Type, Func<object, object>> {
        {
          typeof(LengthParameter),
          goo => { return UnitHelpers.ParseToQuantity<Length>(goo, DefaultUnits.LengthUnitGeometry); }
        }, {
          typeof(AdSecSectionParameter), goo => {
            dynamic gooDynamic = goo;
            return new AdSecSectionGoo(gooDynamic);
          }
        }, {
          typeof(RebarGroupParameter), goo => {
            var gooDynamic = goo as List<object>;
            return gooDynamic.Select(x => new AdSecRebarGroup((x as AdSecRebarGroupGoo).Value)).ToArray();
          }
        }, {
          typeof(SubComponentArrayParameter), goo => {
            var gooDynamic = goo as List<object>;
            return gooDynamic.Select(x => {
              if (x is AdSecSubComponentGoo subComponentGoo) {
                var component = subComponentGoo.Value;
                return new SubComponent() {
                  ISubComponent = component,
                  SectionDesign = new SectionDesign() {
                    Section = component.Section,
                  }
                };
              } else if (x is AdSecSectionGoo sectionGoo) {
                var section = sectionGoo.Value;
                return new SubComponent() {
                  ISubComponent = ISubComponent.Create(section.Section, AdSecCore.Builders.Geometry.Zero()),
                  SectionDesign = new SectionDesign() {
                    Section = section.Section,
                  }
                };
              }

              return null;
            }).ToArray();
          }
        }, {
          typeof(AdSecPointParameter), goo => {
            dynamic gooDynamic = goo;
            return new AdSecPointGoo(gooDynamic);
          }
        }, {
          typeof(DoubleParameter), goo => {
            if (goo is double value) {
              return value;
            }

            return null;
          }
        }, {
          typeof(DoubleArrayParameter), goo => {
            var list = goo as List<object>;
            return list.Select(x => {
              dynamic y = x;
              return (double)y.Value;
            }).ToArray();
          }
        },
      };

    private static T ConfigureParam<T>(Attribute a) where T : IGH_Param, new() {
      return new T {
        Name = a.Name,
        NickName = a.NickName,
        Description = a.Description,
        Access = GetAccess(a),
        Optional = a.Optional,
      };
    }

    private static Param_GenericObject ParamGenericObject(Attribute a) {
      return ConfigureParam<Param_GenericObject>(a);
    }

    private static Param_Number ParamNumber(Attribute a) {
      return ConfigureParam<Param_Number>(a);
    }

    private static Param_Integer ParamInteger(Attribute a) {
      return ConfigureParam<Param_Integer>(a);
    }

    private static Param_String ParamString(Attribute a) {
      return ConfigureParam<Param_String>(a);
    }

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
        default: throw new ArgumentException("Invalid parameter access type");
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
            if (GooToParam.ContainsKey(attribute.GetType())) {
              dynamic newValue = GooToParam[attribute.GetType()](inputs.Value);
              valueBasedParameter.Value = newValue;
            } else {
              try {
                valueBasedParameter.Value = inputs.Value;
              } catch (RuntimeBinderException) {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Input type mismatch for {attribute.Name}");
                return;
              }
            }
          }
        } else if (attribute.GetAccess() == GH_ParamAccess.list) {
          List<object> inputs = new List<object>();
          if (dataAccess.GetDataList(index, inputs)) {
            dynamic valueBasedParameter = attribute;
            if (GooToParam.ContainsKey(attribute.GetType())) {
              dynamic newValue = GooToParam[attribute.GetType()](inputs);
              valueBasedParameter.Value = newValue;
            } else {
              try {
                valueBasedParameter.Value = inputs.ToArray();
              } catch (RuntimeBinderException) {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Input type mismatch for {attribute.Name}");
                return;
              }
            }
          }
        }
      }
    }

    public static void SetOutputValues(this IFunction function, GH_Component component, IGH_DataAccess dataAccess) {
      foreach (var attribute in function.GetAllOutputAttributes().Where(x => ToGoo.ContainsKey(x.GetType()))) {
        int index = component.Params.IndexOfOutputParam(attribute.Name);
        var type = attribute.GetType();
        if (!ToGoo.ContainsKey(type)) {
          throw new InvalidOperationException($"No conversion function found for type {type}");
        }

        var func = ToGoo[type];
        dynamic goo = func(attribute);
        bool success = false;
        if (attribute.GetAccess() == GH_ParamAccess.item) {
          success = dataAccess.SetData(index, goo);
        } else {
          success = dataAccess.SetDataList(index, goo);
        }

        if (!success) {
          throw new InvalidOperationException(
            $"Failed to set data for {attribute.Name} of type {type} at index {index} into param of type {component.Params.Output[index].GetType()}");
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
