using System;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Components {
  public class FindCrackLoad : GH_OasysComponent {

    public FindCrackLoad() : base("Find Crack Load", "CrackLd", "Increases the load until set crack width is reached",
      CategoryName.Name(), SubCategoryName.Cat7()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("f0b27be7-f367-4a2c-b90c-3ba0f66ae584");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CrackLoad;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("BaseLoad", "Ld", "AdSec Load to start the optimisation from.", GH_ParamAccess.item);
      pManager.AddTextParameter("OptimiseLd", "Opt",
        "Text input to select which load component to optimise for, X, YY or ZZ (default 'YY')", GH_ParamAccess.item,
        "YY");
      pManager.AddNumberParameter("Increment", "Inc",
        "Increment the load in steps of this size [using Unit from Settings] (default 1)", GH_ParamAccess.item, 1);
      pManager.AddGenericParameter("MaxCrack", "Crk", "The maximum allowed crack width", GH_ParamAccess.item);
      pManager[2].Optional = true;
      pManager[3].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Load", "Ld",
        "The section load under the applied action." + Environment.NewLine
        + "If the applied deformation is outside the capacity range of the section, the returned load will be zero.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("MaximumCrack", "Crk",
        "The crack result from Cracks that corresponds to the maximum crack width." + Environment.NewLine
        + "If the applied action is outside the capacity range of the section, the returned maximum width crack result will be maximum "
        + "double value.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get solution input
      var solution = AdSecInput.Solution(this, DA, 0);
      if (solution == null) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Results input is null");
        return;
      }

      // get load - can be either load or deformation
      var gh_typ = new GH_ObjectWrapper();
      AdSecLoadGoo load = null;
      if (DA.GetData(1, ref gh_typ)) {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecLoadGoo ld) {
          load = new AdSecLoadGoo(ILoad.Create(ld.Value.X, ld.Value.YY, ld.Value.ZZ));
        } else {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
            "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
          return;
        }
      } else {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
        return;
      }

      var sls = solution.Value.Serviceability.Check(load.Value);

      string loadComponent = "X";
      DA.GetData(2, ref loadComponent);

      double increment = 1;
      DA.GetData(3, ref increment);

      var maxCrack = (Length)Input.UnitNumber(this, DA, 4, DefaultUnits.LengthUnitGeometry);

      var forceUnit = DefaultUnits.ForceUnit;
      var momentUnit = DefaultUnits.MomentUnit;

      while (sls.MaximumWidthCrack.Width <= maxCrack) {
        // update load
        switch (loadComponent.ToLower().Trim()) {
          case "x":
          case "xx":
          case "fx":
          case "fxx":
            load.Value = ILoad.Create(new Force(load.Value.X.As(forceUnit) + increment, forceUnit), load.Value.YY,
              load.Value.ZZ);
            break;

          case "y":
          case "yy":
          case "my":
          case "myy":
            load.Value = ILoad.Create(load.Value.X, new Moment(load.Value.YY.As(momentUnit) + increment, momentUnit),
              load.Value.ZZ);
            break;

          case "z":
          case "zz":
          case "mz":
          case "mzz":
            load.Value = ILoad.Create(load.Value.X, load.Value.YY,
              new Moment(load.Value.ZZ.As(momentUnit) + increment, momentUnit));
            break;
        }

        sls = solution.Value.Serviceability.Check(load.Value);
      }

      // update load to one step back
      switch (loadComponent.ToLower().Trim()) {
        case "x":
        case "xx":
        case "fx":
        case "fxx":
          load.Value = ILoad.Create(new Force(load.Value.X.As(forceUnit) - increment, forceUnit), load.Value.YY,
            load.Value.ZZ);
          break;

        case "y":
        case "yy":
        case "my":
        case "myy":
          load.Value = ILoad.Create(load.Value.X, new Moment(load.Value.YY.As(momentUnit) - increment, momentUnit),
            load.Value.ZZ);
          break;

        case "z":
        case "zz":
        case "mz":
        case "mzz":
          load.Value = ILoad.Create(load.Value.X, load.Value.YY,
            new Moment(load.Value.ZZ.As(momentUnit) - increment, momentUnit));
          break;
      }

      sls = solution.Value.Serviceability.Check(load.Value);

      DA.SetData(0, new AdSecLoadGoo(sls.Load, solution.LocalPlane));

      if (sls.MaximumWidthCrack != null && sls.MaximumWidthCrack.Width.Meters < 1) {
        DA.SetData(1, new AdSecCrackGoo(sls.MaximumWidthCrack, solution.LocalPlane));
      }
    }
  }
}
