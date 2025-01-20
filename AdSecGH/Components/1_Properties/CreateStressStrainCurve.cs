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
    private AdSecStressStrainCurveGoo.StressStrainCurveType _mode
      = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreateStressStrainCurve() : base("Create StressStrainCrv", "StressStrainCrv",
      "Create a Stress Strain Curve for AdSec Material", CategoryName.Name(), SubCategoryName.Cat1()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("b2ddf545-2a4c-45ac-ba1c-cb0f3da5b37f");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainCrv;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      // toggle case
      if (i == 0) {
        // remove dropdown lists beyond first level
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
            // add strain dropdown
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

            // add stress dropdown
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
            _selectedItems.Add(Pressure.GetAbbreviation(_stressUnit));

            Mode2Clicked();
            break;

          case 3:
            Mode3Clicked();
            break;

          case 4:
            // add strain dropdown
            //_dropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

            // add pressure dropdown
            //_dropDownItems.Add(Enum.GetNames(typeof(PressureUnit)).ToList());
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
            _selectedItems.Add(Pressure.GetAbbreviation(_stressUnit));

            Mode4Clicked();
            break;

          case 5:
            Mode5Clicked();
            break;

          case 6:
            // add strain dropdown
            //_dropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

            Mode6Clicked();
            break;

          case 7:
            Mode7Clicked();
            break;

          case 8:
            // add strain dropdown
            //_dropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));
            Mode8Clicked();
            break;

          case 9:
            // add strain dropdown
            //_dropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
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

        Params.Input[1].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        Params.Input[1].NickName = "Ei";
        Params.Input[1].Description = "Initial Moduls from FIB model code";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
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
        Params.Input[0].Name = "Unconfined Strength [" + unitStressAbbreviation + "]";
        Params.Input[0].NickName = "σU";
        Params.Input[0].Description = "Unconfined strength for Mander Confined Model";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Confined Strength [" + unitStressAbbreviation + "]";
        Params.Input[1].NickName = "σC";
        Params.Input[1].Description = "Confined strength for Mander Confined Model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        Params.Input[2].NickName = "Ei";
        Params.Input[2].Description = "Initial Moduls for Mander Confined Model";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;

        Params.Input[3].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
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

        Params.Input[1].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        Params.Input[1].NickName = "Ei";
        Params.Input[1].Description = "Initial Moduls for Mander model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        Params.Input[2].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
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

        Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
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

        Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
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

        Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }
    }

    protected override string HtmlHelp_Source() {
      string help
        = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Materials.StressStrainCurves.html";
      return help;
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
      IStressStrainCurve crv = null;
      try {
        var stressStrainPoint = this.GetStressStrainPoint(DA, 0);
        switch (_mode) {
          case AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear:
            crv = IBilinearStressStrainCurve.Create(stressStrainPoint, stressStrainPoint);
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit:
            var exCrv = IExplicitStressStrainCurve.Create();
            exCrv.Points = AdSecInput.StressStrainPoints(this, DA, 0);
            crv = exCrv;
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode:
            crv = IFibModelCodeStressStrainCurve.Create((Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
              stressStrainPoint, (Strain)Input.UnitNumber(this, DA, 2, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Mander:
            crv = IManderStressStrainCurve.Create((Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
              stressStrainPoint, (Strain)Input.UnitNumber(this, DA, 2, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Linear:
            crv = ILinearStressStrainCurve.Create(stressStrainPoint);
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined:
            crv = IManderConfinedStressStrainCurve.Create((Pressure)Input.UnitNumber(this, DA, 0, _stressUnit),
              (Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
              (Pressure)Input.UnitNumber(this, DA, 2, _stressUnit), (Strain)Input.UnitNumber(this, DA, 3, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle:
            crv = IParabolaRectangleStressStrainCurve.Create(stressStrainPoint,
              (Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Park:
            crv = IParkStressStrainCurve.Create(stressStrainPoint);
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics:
            crv = IPopovicsStressStrainCurve.Create(stressStrainPoint,
              (Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular:
            crv = IRectangularStressStrainCurve.Create(stressStrainPoint,
              (Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
            break;
        }
      } catch (Exception e) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
        return;
      }

      // create preview
      var tuple = AdSecStressStrainCurveGoo.Create(crv, _mode, true);

      DA.SetData(0, new AdSecStressStrainCurveGoo(tuple.Item1, crv, _mode, tuple.Item2));
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
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        while (Params.Input.Count != 2) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode1Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode2Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }

        while (Params.Input.Count != 3) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode3Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Linear || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode4Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined || forceUpdate) {
        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined;

        //remove input parameters
        while (Params.Input.Count > 0) {
          Params.UnregisterInputParameter(Params.Input[0], true);
        }

        while (Params.Input.Count != 4) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode5Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Mander || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Mander;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }

        while (Params.Input.Count != 3) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode6Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }

        while (Params.Input.Count != 2) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode7Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Park || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Park;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode8Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }

        while (Params.Input.Count != 2) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }

    private void Mode9Clicked(bool forceUpdate = false) {
      if (_mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular || forceUpdate) {
        bool cleanAll = false;
        if (_mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined) {
          cleanAll = true;
        }

        RecordUndoEvent("Changed dropdown");
        _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (Params.Input.Count > i) {
          Params.UnregisterInputParameter(Params.Input[i], true);
        }

        if (cleanAll) {
          Params.RegisterInputParam(new Param_GenericObject());
        }

        while (Params.Input.Count != 2) {
          Params.RegisterInputParam(new Param_GenericObject());
        }
      }
    }
  }
}
