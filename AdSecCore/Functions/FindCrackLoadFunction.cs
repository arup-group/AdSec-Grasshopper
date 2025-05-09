﻿
using System;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCore.Functions {
  public class FindCrackLoadFunction : IFunction {


    public SectionSolutionParameter Solution { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = "AdSec Results to perform serviceability check on",
      Access = Access.Item,
      Optional = false,
    };

    public LoadParameter BaseLoad { get; set; } = new LoadParameter {
      Name = "BaseLoad",
      NickName = "Ld",
      Description = "AdSec Load to start the optimisation from",
      Access = Access.Item,
      Optional = false,
    };

    public StringParameter OptimisedLoad { get; set; } = new StringParameter {
      Name = "OptimisedLoad",
      NickName = "Opt",
      Description = "Text input to select which load component to optimise for, X, YY or ZZ (default 'YY')",
      Access = Access.Item,
      Default = "YY",
    };

    public IntegerParameter LoadIncrement { get; set; } = new IntegerParameter {
      Name = "Increment",
      NickName = "Inc",
      Description = "Increment the load in steps of this size [using Unit from Settings] (default 1)",
      Access = Access.Item,
    };


    public LengthParameter MaximumCrack { get; set; } = new LengthParameter {
      Name = "MaxCrack",
      NickName = "Crk",
      Description = "The maximum allowed crack width",
      Access = Access.Item,
    };

    public LoadParameter SectionLoad { get; set; } = new LoadParameter {
      Name = "Load",
      NickName = "Ld",
      Description = $"The section load under the applied action.{Environment.NewLine}If the applied deformation is outside the capacity range of the section, the returned load will be zero",
      Access = Access.Item,
    };

    public CrackParameter MaximumCracking { get; set; } = new CrackParameter {
      Name = "MaximumCrack",
      NickName = "Crk",
      Description = $"The crack result from Cracks that corresponds to the maximum crack width.{Environment.NewLine}If the applied action is outside the capacity range of the section, the returned maximum width crack result will be maximum double value",
      Access = Access.Item,
    };

    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Find Crack Load",
      NickName = "CrackLd",
      Description = "Increases the load until set crack width is reached",
    };

    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat7(),
    };


    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
       Solution,
       BaseLoad,
       OptimisedLoad,
       LoadIncrement,
       MaximumCrack,
      };

    }

    public virtual Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
       SectionLoad,
       MaximumCracking,
      };
    }

    private static bool IsFx(string loadComponent) {
      switch (loadComponent.ToLower().Trim()) {
        case "x":
        case "xx":
        case "fx":
        case "fxx":
          return true;
        default:
          return false;
      }
    }

    private static bool IsMyy(string loadComponent) {
      switch (loadComponent.ToLower().Trim()) {
        case "y":
        case "yy":
        case "my":
        case "myy":
          return true;
        default:
          return false;
      }
    }

    private static bool IsMzz(string loadComponent) {
      switch (loadComponent.ToLower().Trim()) {
        case "z":
        case "zz":
        case "mz":
        case "mzz":
          return true;
        default:
          return false;
      }
    }

    private static void UpdatedLoad(string loadComponent, ref ILoad baseLoad, int increment) {
      var forceUnit = ContextUnits.Instance.ForceUnit;
      var momentUnit = ContextUnits.Instance.MomentUnit;
      if (IsFx(loadComponent)) {
        baseLoad = ILoad.Create(new Force(baseLoad.X.As(forceUnit) + increment, forceUnit), baseLoad.YY,
          baseLoad.ZZ);
      } else if (IsMyy(loadComponent)) {
        baseLoad = ILoad.Create(baseLoad.X, new Moment(baseLoad.YY.As(momentUnit) + increment, momentUnit),
          baseLoad.ZZ);
      } else if (IsMzz(loadComponent)) {
        baseLoad = ILoad.Create(baseLoad.X, baseLoad.YY,
          new Moment(baseLoad.ZZ.As(momentUnit) + increment, momentUnit));
      } else {
        throw new ArgumentException($"Load component {loadComponent} is not supported.");
      }
    }

    public void Compute() {
      var solution = Solution.Value;
      var baseLoad = BaseLoad.Value;
      var loadComponent = OptimisedLoad.Value;
      var increment = LoadIncrement.Value;
      var sls = solution.Solution.Serviceability.Check(baseLoad);
      var maxCrack = MaximumCrack.Value.ToUnit(sls.MaximumWidthCrack.Width.Unit);
      while (sls.MaximumWidthCrack.Width <= maxCrack) {
        // update load
        UpdatedLoad(loadComponent, ref baseLoad, increment);
        sls = solution.Solution.Serviceability.Check(baseLoad);
      }

      // update load to one step back
      UpdatedLoad(loadComponent, ref baseLoad, -increment);

      sls = solution.Solution.Serviceability.Check(baseLoad);

      SectionLoad.Value = sls.Load;
      MaximumCracking.Value = new CrackLoad() { Load = sls.MaximumWidthCrack, Plane = solution.SectionDesign.LocalPlane };
    }

  }

}
