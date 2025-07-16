using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Microsoft.CSharp.RuntimeBinder;

using Oasys.AdSec;
using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH.Parameters;
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
          typeof(PathParameter), ConfigureParam<Param_FilePath>
        }, {
          typeof(SectionArrayParameter), ParamGenericObject
        }, {
          typeof(PlaneParameter), ConfigureParam<Param_Plane>
        }, {
          typeof(BooleanParameter), ConfigureParam<Param_Boolean>
        }, {
          typeof(SubComponentArrayParameter), ParamGenericObject
        }, {
          typeof(StressStrainCurveParameter), ParamGenericObject
        }, {
          typeof(CrackCalcParameter), ParamGenericObject
        }, {
          typeof(DesignCodeParameter), ParamGenericObject
        }, {
          typeof(MaterialParameter), ParamGenericObject
        }, {
          typeof(SectionParameter), ParamGenericObject
        }, {
          typeof(GeometryParameter), ConfigureParam<Param_Geometry>
        }, {
          typeof(AdSecMaterialParameter), ParamGenericObject
        }, {
          typeof(RebarGroupArrayParameter), ParamGenericObject
        }, {
          typeof(RebarGroupParameter), ParamGenericObject
        },{
          typeof(RebarLayerParameter), ParamGenericObject
        }, {
          typeof(RebarLayerArrayParameter), ParamGenericObject
        }, {
          typeof(RebarBundleParameter), ParamGenericObject
        }, {
          typeof(ProfileParameter), ParamGenericObject
        }, {
          typeof(DoubleParameter), ParamNumber
        }, {
          typeof(NullableDoubleParameter), ParamNumber
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
        }, {
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
        }, {
          typeof(NeutralLineParameter), ParamGenericObject
        }, {
          typeof(StrainParameter), ParamGenericObject
        }, {
          typeof(PressureParameter), ParamGenericObject
        }, {
          typeof(StrainArrayParameter), ParamGenericObject
        }, {
          typeof(PressureArrayParameter), ParamGenericObject
        }, {
          typeof(PointArrayParameter), ParamGenericObject
        }, {
          typeof(CurvatureParameter), ParamGenericObject
        }, {
          typeof(ForceParameter), ParamGenericObject
        }, {
          typeof(MomentParameter), ParamGenericObject
        },{
          typeof(StressStrainPointParameter), ParamGenericObject
        },{
          typeof(StressStrainPointArrayParameter), ParamGenericObject
        },
      };

    /// <summary>
    ///   For the Outputs
    ///   This is use to DA.SetData()
    ///   i.e. we will get the value from the Attribute.Value and set it to a Goo object
    ///   Since the value is all we need and this is often shared between the "Business" Parameter and the Gh (Goo) object, it
    ///   can take the form:
    ///   { typeof(
    ///   <ParamType), a=>
    ///     (a as ParamType).Value }
    ///     We need to do more work when the Goo, has a more complex constructor in which
    ///     case it might be best to create a new constructor, so we can simplify this dictionary and even deprecated later on.
    /// </summary>
    private static readonly Dictionary<Type, Func<Attribute, object>> ToGoo
      = new Dictionary<Type, Func<Attribute, object>> {
        { typeof(SubComponentParameter), a => new AdSecSubComponentGoo((a as SubComponentParameter)?.Value) },
        { typeof(RebarGroupArrayParameter), a => {
            return (a as RebarGroupArrayParameter).Value.Select(x=> new AdSecRebarGroupGoo(x)).ToList();
          }
        },{
          typeof(RebarLayerParameter), a => {
            var group = (a as RebarLayerParameter).Value;
            return new AdSecRebarLayerGoo(group);
          }
        },{
          typeof(RebarGroupParameter), a => {
            var group = (a as RebarGroupParameter).Value;
            return new AdSecRebarGroupGoo(group);
          }
        }, {
          typeof(SectionArrayParameter), a => {
            var parameter = (a as SectionArrayParameter);
            return parameter.Value.Select(x => new AdSecSectionGoo(new AdSecSection(x))).ToList();
          }
        }, {
          typeof(RebarBundleParameter), a => new AdSecRebarBundleGoo((a as RebarBundleParameter).Value)
        },
        { typeof(DoubleParameter), a => new GH_Number((a as DoubleParameter).Value) }, {
          typeof(LoadSurfaceParameter),
          a => new AdSecFailureSurfaceGoo((a as LoadSurfaceParameter).Value)
        },
        { typeof(DoubleArrayParameter), a => (a as DoubleArrayParameter).Value }, {
          typeof(ProfileParameter), a => {
            var profileDesign = (a as ProfileParameter).Value;
            return new AdSecProfileGoo(profileDesign);
          }
        }, {
          typeof(MaterialParameter), a => {
            var materialDesign = (a as MaterialParameter).Value;
            return new AdSecMaterialGoo(materialDesign);
          }
        }, {
          typeof(DesignCodeParameter), a => {
            var designCode = (a as DesignCodeParameter).Value;
            return new AdSecDesignCodeGoo(designCode);
          }
        },
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
          typeof(PointArrayParameter), a => {
            var points = (a as PointArrayParameter).Value;
            return points.Select(point => new AdSecPointGoo(point)).ToList();
          }
        }, {
          typeof(AdSecMaterialArrayParam), a => {
            var materials = (a as AdSecMaterialArrayParam).Value;
            return materials?.ToList();
          }
        },
        { typeof(IntegerArrayParameter), a => (a as IntegerArrayParameter).Value }, {
          typeof(GeometryParameter), a => {
            var value = (a as GeometryParameter).Value;
            var sectionDesign = value as SectionDesign;
            var sectionGoo = new AdSecSectionGoo(new AdSecSection(sectionDesign));
            var curves = sectionGoo.DrawInstructionsList.Select(x => {
              GH_Curve curve = null;
              GH_Convert.ToGHCurve(x.Geometry, GH_Conversion.Both, ref curve);
              return curve;
            }).ToList();
            return curves;
          }
        },
        { typeof(StringArrayParam), a => (a as StringArrayParam).Value }, {
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
        }, {
          typeof(NeutralLineParameter), a => {
            var value = (a as NeutralLineParameter).Value;
            return new AdSecNeutralAxisGoo(value);
          }
        }, {
          typeof(LengthParameter), a => { return (a as LengthParameter).Value; }
        }, {
          typeof(StrainParameter), a => {
            var value = (a as StrainParameter).Value;
            return new GH_UnitNumber(value.ToUnit(DefaultUnits.StrainUnitResult));
          }
        }, {
          typeof(PressureParameter), a => {
            var value = (a as PressureParameter).Value;
            return new GH_UnitNumber(value.ToUnit(DefaultUnits.StressUnitResult));
          }
        }, {
          typeof(StrainArrayParameter), a => {
            var strains = (a as StrainArrayParameter).Value;
            var quantityInRelevantUnit = new List<GH_UnitNumber>();
            foreach (var strain in strains) {
              quantityInRelevantUnit.Add(new GH_UnitNumber(strain.ToUnit(DefaultUnits.StrainUnitResult)));
            }

            return quantityInRelevantUnit;
          }
        }, {
          typeof(PressureArrayParameter), a => {
            var strsses = (a as PressureArrayParameter).Value;
            var quantityInRelevantUnit = new List<GH_UnitNumber>();
            foreach (var stress in strsses) {
              quantityInRelevantUnit.Add(new GH_UnitNumber(stress.ToUnit(DefaultUnits.StressUnitResult)));
            }

            return quantityInRelevantUnit;
          }
        }, {
          typeof(StressStrainCurveParameter), a => {
            var stressStrainCurve = (a as StressStrainCurveParameter).Value;
            return AdSecStressStrainCurveGoo.Create(stressStrainCurve.IStressStrainCurve,stressStrainCurve.IsCompression);
          }
        }, {
          typeof(StressStrainPointParameter), a => {
            var stressStrainPoint = (a as StressStrainPointParameter).Value;
            return new AdSecStressStrainPointGoo(stressStrainPoint);
          }
        }, {
          typeof(CrackCalcParameter), a => {
            var crackParameter = (a as CrackCalcParameter).Value;
            return new AdSecConcreteCrackCalculationParametersGoo(crackParameter);
          }
        }
      };

    /// <summary>
    ///   Setting the Inputs
    ///   ***************************************************************************
    ///   ******* if the base data is the same, you can skip this completely! *******
    ///   ***************************************************************************
    ///   This is for grabbing the Values from Grasshopper and feeding the to the Business Input Params
    ///   So we often need to convert the Grasshopper object to the Business object
    ///   Again we aim to have the data, so the conversion would be simple
    ///   example: { typeof(ParamType), goo => new DataType(goo) }
    ///   This is the place where we might need to call AdSecInput to account for different inputed types
    ///   like in the case of SubComponent, that can also accept a Section Type
    /// </summary>
    private static readonly Dictionary<Type, Func<object, object>> GooToParam
      = new Dictionary<Type, Func<object, object>> {
        {
          typeof(PlaneParameter),
          goo => {
            var values = goo as List<object>;
            return values.Select(x => (x as GH_Plane).Value.ToOasys()).ToArray();
          }
        }, {
          typeof(LengthParameter),
          goo => { return UnitHelpers.ParseToQuantity<Length>(goo, DefaultUnits.LengthUnitGeometry); }
        }, {
          typeof(AdSecSectionParameter), goo => {
            dynamic gooDynamic = goo;
            return new AdSecSectionGoo(gooDynamic);
          }
        }, {
          typeof(RebarLayerArrayParameter), goo => {
            var gooDynamic = goo as List<object>;
            return gooDynamic.Select(x => (x as AdSecRebarLayerGoo).Value).ToArray();
          }
        }, {
          typeof(PointParameter), goo => {
            dynamic gooDynamic = goo;
            return new AdSecPointGoo(gooDynamic).AdSecPoint;
          }
        }, {
          typeof(PointArrayParameter), goo => {
            var gooDynamic = goo as List<object>;
            return gooDynamic.Select(x => new AdSecPointGoo((x as AdSecPointGoo).Value).AdSecPoint).ToArray();
          }
        }, {
          typeof(RebarGroupArrayParameter), goo => {
            var gooDynamic = goo as List<object>;
            return gooDynamic.Select(x => new AdSecRebarGroup((x as AdSecRebarGroupGoo).Value)).ToArray();
          }
        }, {
          typeof(RebarGroupParameter),
          goo => { return goo is AdSecRebarGroup value ? new AdSecRebarGroup(value) : null; }
        }, {
          typeof(SubComponentArrayParameter), goo => {
            var gooDynamic = goo as List<object>;
            return SubComponentCasting(gooDynamic);
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
          typeof(BooleanParameter), goo => {
            if (goo is bool value) {
              return value;
            }

            return null;
          }
        }, {
          typeof(NullableDoubleParameter), goo => {
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
        }, {
          typeof(CurvatureParameter),
          goo => { return UnitHelpers.ParseToQuantity<Curvature>(goo, DefaultUnits.CurvatureUnit); }
        }, {
          typeof(StrainParameter),
          goo => { return UnitHelpers.ParseToQuantity<Strain>(goo, DefaultUnits.StrainUnitResult); }
        }, {
          typeof(ForceParameter), goo => { return UnitHelpers.ParseToQuantity<Force>(goo, DefaultUnits.ForceUnit); }
        }, {
          typeof(MomentParameter), goo => { return UnitHelpers.ParseToQuantity<Moment>(goo, DefaultUnits.MomentUnit); }
        }, {
          typeof(DesignCodeParameter), goo => {
            return goo is AdSecDesignCode value ? new DesignCode {
              IDesignCode = value.DesignCode,
              DesignCodeName = value.DesignCodeName
            } : goo;
          }
        },{
          typeof(StressStrainPointParameter), goo =>
          {
            return ConvertToStressStrainPoint(goo);
          }
        },{
          typeof(StressStrainPointArrayParameter), goo => {
           var list = goo as List<object>;
            return list.Select(x => {
              dynamic y = x;
             return (IStressStrainPoint)ConvertToStressStrainPoint(y.Value);
            }).ToArray();
          }
        }, {
          typeof(ProfileParameter), goo => {
            var profileDesign = goo is Oasys.Taxonomy.Profiles.IProfile profile
              ? new ProfileDesign() { Profile = AdSecProfiles.CreateProfile(profile) }
              : goo as ProfileDesign;
            var profileGoo = new AdSecProfileGoo(profileDesign);
            return new ProfileDesign(){ Profile = profileGoo.Clone(), LocalPlane = profileGoo.LocalPlane.ToOasys() };
           }
        },
      };

    private static object ConvertToStressStrainPoint(dynamic goo) {
      IStressStrainPoint stressStrainPoint = null;
      var point3d = new Point3d();
      if (goo is IStressStrainPoint point) {
        stressStrainPoint = point;
      } else if (GH_Convert.ToPoint3d(goo, ref point3d, GH_Conversion.Both)) {
        stressStrainPoint = AdSecStressStrainPointGoo.CreateFromPoint3d(point3d);
      } else {
        throw new RuntimeBinderException("Input type mismatch");
      }
      return new AdSecStressStrainPointGoo(stressStrainPoint).StressStrainPoint;
    }

    private static SubComponent[] SubComponentCasting(List<object> gooDynamic) {
      return gooDynamic.Select(x => {
        if (x is AdSecSubComponentGoo subComponentGoo) {
          var component = subComponentGoo.Value;
          return new SubComponent {
            ISubComponent = component,
            SectionDesign = new SectionDesign {
              Section = component.Section,
            },
          };
        }

        if (x is AdSecSectionGoo sectionGoo) {
          var section = sectionGoo.Value;
          return new SubComponent {
            ISubComponent = ISubComponent.Create(section.Section, AdSecCore.Builders.Geometry.Zero()),
            SectionDesign = new SectionDesign {
              Section = section.Section,
            },
          };
        }

        return null;
      }).ToArray();
    }

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
      component.Hidden = BusinessComponent.Organisation.Hidden;
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
      var allInputAttributes = function.GetAllInputAttributes();
      foreach (var attribute in allInputAttributes.Where(x => ToGoo.ContainsKey(x.GetType()))) {
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
      if (function is Function coreFunction) {
        coreFunction.ClearMessages();
        coreFunction.ClearInputs();
        coreFunction.ClearOutputs();
      }

      foreach (var attribute in function.GetAllInputAttributes()) {
        int index = component.Params.IndexOfInputParam(attribute.Name);
        dynamic valueBasedParameter = attribute;
        if (attribute.GetAccess() == GH_ParamAccess.item) {
          dynamic inputs = null;
          if (dataAccess.GetData(index, ref inputs)) {
            try {
              if (GooToParam.ContainsKey(attribute.GetType())) {
                dynamic newValue = GooToParam[attribute.GetType()](inputs.Value);
                valueBasedParameter.Value = newValue;
              } else {
                valueBasedParameter.Value = inputs.Value;
              }
            } catch (RuntimeBinderException) {
              component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Input type mismatch for {attribute.Name}");
              return;
            }
          }
        } else if (attribute.GetAccess() == GH_ParamAccess.list) {
          var inputs = new List<object>();
          try {
            if (dataAccess.GetDataList(index, inputs)) {
              if (GooToParam.ContainsKey(attribute.GetType())) {
                dynamic newValue = GooToParam[attribute.GetType()](inputs);
                valueBasedParameter.Value = newValue;
              } else {
                valueBasedParameter.Value = inputs.Select(x => ((dynamic)x).Value).ToArray();
              }
            }
          } catch (RuntimeBinderException) {
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Input type mismatch for {attribute.Name}");
            return;
          }
        }
      }
    }

    public static void SetOutputValues(this IFunction function, GH_Component component, IGH_DataAccess dataAccess) {
      var allOutputAttributes = function.GetAllOutputAttributes();
      foreach (var attribute in allOutputAttributes.Where(x => ToGoo.ContainsKey(x.GetType()))) {
        int index = component.Params.IndexOfOutputParam(attribute.Name);
        var type = attribute.GetType();
        if (!ToGoo.ContainsKey(type)) {
          throw new MissingPrimaryKeyException($"No conversion function found for type {type}");
        }

        var func = ToGoo[type];
        dynamic goo = func(attribute);
        bool success;
        if (attribute.GetAccess() == GH_ParamAccess.item) {
          success = dataAccess.SetData(index, goo);
        } else {
          success = dataAccess.SetDataList(index, goo);
        }

        if (!success) {
          throw new InvalidCastException(
            $"Failed to set data for {attribute.Name} of type {type} at index {index} into param of type {component.Params.Output[index].GetType()}");
        }
      }
    }

    public static void PopulateInputParams(
      this IFunction function, GH_Component component, Dictionary<string, IGH_Param> previous = null) {
      RegisterParams(function.GetAllInputAttributes(), param => component.Params.RegisterInputParam(param), previous);
    }

    private static void RegisterParams(
      Attribute[] attributesSelector, Action<IGH_Param> action, Dictionary<string, IGH_Param> previous = null) {
      foreach (var attribute in attributesSelector.Where(x => ToGhParam.ContainsKey(x.GetType()))) {
        if (previous != null && previous.TryGetValue(attribute.Name, out var value)) {
          action(value);
          continue;
        }

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
