using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateStressStrainCurve : GH_OasysDropDownComponent {
    private const string helpLink
      = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Materials.StressStrainCurves.html";

    public override Guid ComponentGuid => new Guid("b2ddf545-2a4c-45ac-ba1c-cb0f3da5b37f");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainCrv;
    private AdSecStressStrainCurveGoo.StressStrainCurveType _mode
      = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreateStressStrainCurve() : base("Create StressStrainCrv", "StressStrainCrv",
      "Create a Stress Strain Curve for AdSec Material", CategoryName.Name(), SubCategoryName.Cat1()) {
      Hidden = false;
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      if (i == 0) {
        while (_dropDownItems.Count > 1) {
          _dropDownItems.RemoveAt(1);
        }

        switch (j) {
          case 0:
            Mode0Clicked();
            break;

          case 1:
            Mode1Clicked();
            break;

          case 2:
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
            _selectedItems.Add(Pressure.GetAbbreviation(_stressUnit));

            Mode2Clicked();
            break;

          case 3:
            Mode3Clicked();
            break;

          case 4:
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
            _selectedItems.Add(Pressure.GetAbbreviation(_stressUnit));

            Mode4Clicked();
            break;

          case 5:
            Mode5Clicked();
            break;

          case 6:
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

            Mode6Clicked();
            break;

          case 7:
            Mode7Clicked();
            break;

          case 8:
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));
            Mode8Clicked();
            break;

          case 9:
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));
            Mode9Clicked();
            break;
        }
      } else {
        switch (i) {
          case 1:
            _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[i]);
            break;

          case 2:
            _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[i]);
            break;
        }
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string unitStressAbbreviation = Pressure.GetAbbreviation(_stressUnit);
      string unitStrainAbbreviation = Strain.GetAbbreviation(_strainUnit);

      if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear) {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Failure Point";
        Params.Input[1].NickName = "SPu";
        Params.Input[1].Description = "AdSec Stress Strain Point representing the Failure Point";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit) {
        Params.Input[0].Name = "StressStrainPts";
        Params.Input[0].NickName = "SPs";
        Params.Input[0].Description = "AdSec Stress Strain Points representing the StressStrainCurve as a Polyline";
        Params.Input[0].Access = GH_ParamAccess.list;
        Params.Input[0].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode) {
        Params.Input[0].Name = "Peak Point";
        Params.Input[0].NickName = "SPt";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the FIB model's Peak Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = $"Initial Modus [{unitStressAbbreviation}]";
        Params.Input[1].NickName = "Ei";
        Params.Input[1].Description = "Initial Moduls from FIB model code";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = $"Failure Strain [{unitStrainAbbreviation}]";
        Params.Input[2].NickName = "εu";
        Params.Input[2].Description = "Failure strain from FIB model code";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Linear) {
        Params.Input[0].Name = "Failure Point";
        Params.Input[0].NickName = "SPu";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Failure Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
        Params.Input[0].Name = $"Unconfined Strength [{unitStressAbbreviation}]";
        Params.Input[0].NickName = "σU";
        Params.Input[0].Description = "Unconfined strength for Mander Confined Model";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = $"Confined Strength [{unitStressAbbreviation}]";
        Params.Input[1].NickName = "σC";
        Params.Input[1].Description = "Confined strength for Mander Confined Model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = $"Initial Modus [{unitStressAbbreviation}]";
        Params.Input[2].NickName = "Ei";
        Params.Input[2].Description = "Initial Moduls for Mander Confined Model";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;

        Params.Input[3].Name = $"Failure Strain [{unitStrainAbbreviation}]";
        Params.Input[3].NickName = "εu";
        Params.Input[3].Description = "Failure strain for Mander Confined Model";
        Params.Input[3].Access = GH_ParamAccess.item;
        Params.Input[3].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Mander) {
        Params.Input[0].Name = "Peak Point";
        Params.Input[0].NickName = "SPt";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Mander model's Peak Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = $"Initial Modus [{unitStressAbbreviation}]";
        Params.Input[1].NickName = "Ei";
        Params.Input[1].Description = "Initial Moduls for Mander model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = $"Failure Strain [{unitStrainAbbreviation}]";
        Params.Input[2].NickName = "εu";
        Params.Input[2].Description = "Failure strain for Mander model";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle) {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = $"Failure Strain [{unitStrainAbbreviation}]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain from FIB model code";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Park) {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics) {
        Params.Input[0].Name = "Peak Point";
        Params.Input[0].NickName = "SPt";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Peak Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = $"Failure Strain [{unitStrainAbbreviation}]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain from Popovic model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      } else if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular) {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = $"Failure Strain [{unitStrainAbbreviation}]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }
    }

    protected override string HtmlHelp_Source() {
      return helpLink;
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Curve Type",
        "Strain Unit",
        "Stress Unit",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(AdSecStressStrainCurveGoo.StressStrainCurveType)).ToList());
      _dropDownItems[0].RemoveAt(_dropDownItems[0].Count - 1);
      _selectedItems.Add(_mode.ToString());

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Failure", "SPu", "AdSec Stress Strain Point representing the Failure Point",
        GH_ParamAccess.item);
      _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("StressStrainCrv", "SCv", "AdSec Stress Strain Curve", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      IStressStrainCurve curve = null;
      try {
        var stressStrainPoint = _mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit ?
          this.GetStressStrainPoint(DA, 0) : null;
        switch (_mode) {
          case AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear:
            curve = IBilinearStressStrainCurve.Create(stressStrainPoint, stressStrainPoint);
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit:
            var explicitStressStrainCurve = IExplicitStressStrainCurve.Create();
            explicitStressStrainCurve.Points = this.GetStressStrainPoints(DA, 0);
            curve = explicitStressStrainCurve;
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode:
            curve = IFibModelCodeStressStrainCurve.Create((Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
              stressStrainPoint, (Strain)Input.UnitNumber(this, DA, 2, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Mander:
            curve = IManderStressStrainCurve.Create((Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
              stressStrainPoint, (Strain)Input.UnitNumber(this, DA, 2, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Linear:
            curve = ILinearStressStrainCurve.Create(stressStrainPoint);
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined:
            curve = IManderConfinedStressStrainCurve.Create((Pressure)Input.UnitNumber(this, DA, 0, _stressUnit),
              (Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
              (Pressure)Input.UnitNumber(this, DA, 2, _stressUnit), (Strain)Input.UnitNumber(this, DA, 3, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle:
            curve = IParabolaRectangleStressStrainCurve.Create(stressStrainPoint,
              (Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Park:
            curve = IParkStressStrainCurve.Create(stressStrainPoint);
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics:
            curve = IPopovicsStressStrainCurve.Create(stressStrainPoint,
              (Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular:
            curve = IRectangularStressStrainCurve.Create(stressStrainPoint,
              (Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
            break;
        }
      } catch (Exception e) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
        return;
      }

      var tuple = AdSecStressStrainCurveGoo.Create(curve, _mode, true);

      DA.SetData(0, new AdSecStressStrainCurveGoo(tuple.Item1, curve, _mode, tuple.Item2));
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)Enum.Parse(
        typeof(AdSecStressStrainCurveGoo.StressStrainCurveType), _selectedItems[0]);
      if (_selectedItems.Count > 1) {
        _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[1]);
      }

      if (_selectedItems.Count > 2) {
        _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[2]);
      }

      switch (_mode) {
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear:
          Mode0Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit:
          Mode1Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode:
          Mode2Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.Linear:
          Mode3Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined:
          Mode4Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.Mander:
          Mode5Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle:
          Mode6Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.Park:
          Mode7Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics:
          Mode8Clicked(true);
          break;

        case AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular:
          Mode9Clicked(true);
          break;
      }

      base.UpdateUIFromSelectedItems();
    }

    private void Mode0Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear, forceUpdate, 2);
    }

    private void Mode1Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, forceUpdate);
    }

    private void Mode2Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode, forceUpdate, 3);
    }

    private void Mode3Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, forceUpdate);
    }

    private void Mode4Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined, forceUpdate, 4);
    }

    private void Mode5Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Mander, forceUpdate, 3);
    }

    private void Mode6Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics, forceUpdate, 2);
    }

    private void Mode7Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Park, forceUpdate);
    }

    private void Mode8Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics, forceUpdate, 2);
    }

    private void Mode9Clicked(bool forceUpdate = false) {
      ModeClicked(AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular, forceUpdate, 2);
    }

    /// <summary>
    ///   Changes the mode for the curve and updates the number of input parameters based on the new mode
    /// </summary>
    /// <param name="mode">New mode for the curve</param>
    /// <param name="forceUpdate">Whether to force the update even if the mode is already set</param>
    /// <param name="desiredIndex">The number of parameters to have after the update</param>
    private void ModeClicked(
      AdSecStressStrainCurveGoo.StressStrainCurveType mode, bool forceUpdate, int desiredIndex = 0) {
      if (_mode == mode && !forceUpdate) {
        return;
      }

      bool cleanAll = _mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined;
      RecordUndoEvent("Changed dropdown");
      _mode = mode;

      RemoveInputsAboveIndex(cleanAll ? 0 : 1);

      if (cleanAll) {
        Params.RegisterInputParam(new Param_GenericObject());
      }

      ExpandParamsToIndex(desiredIndex);
    }

    private void RemoveInputsAboveIndex(int i) {
      while (Params.Input.Count > i) {
        Params.UnregisterInputParameter(Params.Input[i], true);
      }
    }

    private void ExpandParamsToIndex(int index) {
      if (index <= Params.Input.Count) {
        return;
      }

      while (Params.Input.Count != index) {
        Params.RegisterInputParam(new Param_GenericObject());
      }
    }
  }
}
