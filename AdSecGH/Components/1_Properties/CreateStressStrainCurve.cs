using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
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
using Rhino.Geometry;

namespace AdSecGH.Components
{
  public class CreateStressStrainCurve : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("b2ddf545-2a4c-45ac-ba1c-cb0f3da5b37f");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainCrv;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;
    private AdSecStressStrainCurveGoo.StressStrainCurveType _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;

    public CreateStressStrainCurve() : base(
      "Create StressStrainCrv",
      "StressStrainCrv",
      "Create a Stress Strain Curve for AdSec Material",
      CategoryName.Name(),
      SubCategoryName.Cat1())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }

    protected override string HtmlHelp_Source()
    {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Materials.StressStrainCurves.html";
      return help;
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Failure", "SPu", "AdSec Stress Strain Point representing the Failure Point", GH_ParamAccess.item);
      this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("StressStrainCrv", "SCv", "AdSec Stress Strain Curve", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      IStressStrainCurve crv = null;
      try
      {
        switch (this._mode)
        {
          case AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear:
            crv = IBilinearStressStrainCurve.Create(AdSecInput.StressStrainPoint(this, DA, 0), AdSecInput.StressStrainPoint(this, DA, 1));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit:
            IExplicitStressStrainCurve exCrv = IExplicitStressStrainCurve.Create();
            exCrv.Points = AdSecInput.StressStrainPoints(this, DA, 0);
            crv = exCrv;
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode:
            crv = IFibModelCodeStressStrainCurve.Create(
              (Pressure)Input.UnitNumber(this, DA, 1, this._stressUnit),
              AdSecInput.StressStrainPoint(this, DA, 0),
              (Strain)Input.UnitNumber(this, DA, 2, this._strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Mander:
            crv = IManderStressStrainCurve.Create(
              (Pressure)Input.UnitNumber(this, DA, 1, this._stressUnit),
              AdSecInput.StressStrainPoint(this, DA, 0),
              (Strain)Input.UnitNumber(this, DA, 2, this._strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Linear:
            crv = ILinearStressStrainCurve.Create(
              AdSecInput.StressStrainPoint(this, DA, 0));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined:
            crv = IManderConfinedStressStrainCurve.Create(
              (Pressure)Input.UnitNumber(this, DA, 0, this._stressUnit),
              (Pressure)Input.UnitNumber(this, DA, 1, this._stressUnit),
              (Pressure)Input.UnitNumber(this, DA, 2, this._stressUnit),
              (Strain)Input.UnitNumber(this, DA, 3, this._strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle:
            crv = IParabolaRectangleStressStrainCurve.Create(
              AdSecInput.StressStrainPoint(this, DA, 0),
              (Strain)Input.UnitNumber(this, DA, 1, this._strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Park:
            crv = IParkStressStrainCurve.Create(
              AdSecInput.StressStrainPoint(this, DA, 0));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics:
            crv = IPopovicsStressStrainCurve.Create(
              AdSecInput.StressStrainPoint(this, DA, 0),
              (Strain)Input.UnitNumber(this, DA, 1, this._strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular:
            crv = IRectangularStressStrainCurve.Create(
              AdSecInput.StressStrainPoint(this, DA, 0),
              (Strain)Input.UnitNumber(this, DA, 1, this._strainUnit));
            break;
        }
      }
      catch (Exception e)
      {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
        return;
      }

      // create preview
      Tuple<Curve, List<Point3d>> tuple = AdSecStressStrainCurveGoo.Create(crv, this._mode, true);

      DA.SetData(0, new AdSecStressStrainCurveGoo(tuple.Item1, crv, _mode, tuple.Item2));
    }

    #region Custom UI
    protected override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Curve Type",
        "Strain Unit",
        "Stress Unit",
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      this.DropDownItems.Add(Enum.GetNames(typeof(AdSecStressStrainCurveGoo.StressStrainCurveType)).ToList());
      this.DropDownItems[0].RemoveAt(this.DropDownItems[0].Count - 1);
      this.SelectedItems.Add(this._mode.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      this.SelectedItems[i] = this.DropDownItems[i][j];

      // toggle case
      if (i == 0)
      {
        // remove dropdown lists beyond first level
        while (this.DropDownItems.Count > 1)
          this.DropDownItems.RemoveAt(1);

        switch (j)
        {
          case 0:
            this.Mode0Clicked();
            break;
          case 1:
            this.Mode1Clicked();
            break;
          case 2:
            // add strain dropdown
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            this.SelectedItems.Add(Strain.GetAbbreviation(this._strainUnit));

            // add stress dropdown
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
            this.SelectedItems.Add(Pressure.GetAbbreviation(this._stressUnit));

            this.Mode2Clicked();
            break;
          case 3:
            this.Mode3Clicked();
            break;
          case 4:
            // add strain dropdown
            //this.DropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            this.SelectedItems.Add(Strain.GetAbbreviation(this._strainUnit));

            // add pressure dropdown
            //this.DropDownItems.Add(Enum.GetNames(typeof(PressureUnit)).ToList());
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
            this.SelectedItems.Add(Pressure.GetAbbreviation(this._stressUnit));

            this.Mode4Clicked();
            break;
          case 5:
            this.Mode5Clicked();
            break;
          case 6:
            // add strain dropdown
            //this.DropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            this.SelectedItems.Add(Strain.GetAbbreviation(this._strainUnit));

            this.Mode6Clicked();
            break;
          case 7:
            this.Mode7Clicked();
            break;
          case 8:
            // add strain dropdown
            //this.DropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            this.SelectedItems.Add(Strain.GetAbbreviation(this._strainUnit));
            this.Mode8Clicked();
            break;
          case 9:
            // add strain dropdown
            //this.DropDownItems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
            this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
            this.SelectedItems.Add(Strain.GetAbbreviation(this._strainUnit));
            this.Mode9Clicked();
            break;
        }
      }
      else
      {
        switch (i)
        {
          case 1:
            this._strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), this.SelectedItems[i]);
            break;
          case 2:
            this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[i]);
            break;
        }
      }
      base.UpdateUI();
    }

    protected override void UpdateUIFromSelectedItems()
    {
      this._mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)Enum.Parse(typeof(AdSecStressStrainCurveGoo.StressStrainCurveType), this.SelectedItems[0]);
      this._strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), this.SelectedItems[1]);
      this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[2]);

      switch (this._mode)
      {
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear:
          this.Mode0Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit:
          this.Mode1Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode:
          this.Mode2Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Linear:
          this.Mode3Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined:
          this.Mode4Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Mander:
          this.Mode5Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle:
          this.Mode6Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Park:
          this.Mode7Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics:
          this.Mode8Clicked(true);
          break;
        case AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular:
          this.Mode9Clicked(true);
          break;
      }
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    #region menu override
    private void Mode0Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        while (this.Params.Input.Count != 2)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode1Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode2Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
        while (this.Params.Input.Count != 3)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode3Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Linear || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode4Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined || forceUpdate)
      {
        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined;

        //remove input parameters
        while (this.Params.Input.Count > 0)
          this.Params.UnregisterInputParameter(this.Params.Input[0], true);
        while (this.Params.Input.Count != 4)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode5Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Mander || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Mander;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
        while (this.Params.Input.Count != 3)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode6Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
        while (this.Params.Input.Count != 2)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode7Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Park || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Park;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode8Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
        while (this.Params.Input.Count != 2)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }

    private void Mode9Clicked(bool forceUpdate = false)
    {
      if (this._mode != AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular || forceUpdate)
      {
        bool cleanAll = false;
        if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
          cleanAll = true;

        this.RecordUndoEvent("Changed dropdown");
        this._mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular;

        //remove input parameters
        int i = cleanAll ? 0 : 1;
        while (this.Params.Input.Count > i)
          this.Params.UnregisterInputParameter(this.Params.Input[i], true);
        if (cleanAll)
          this.Params.RegisterInputParam(new Param_GenericObject());
        while (this.Params.Input.Count != 2)
          this.Params.RegisterInputParam(new Param_GenericObject());
      }
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      string unitStressAbbreviation = Pressure.GetAbbreviation(this._stressUnit);
      string unitStrainAbbreviation = Strain.GetAbbreviation(this._strainUnit);

      if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear)
      {
        this.Params.Input[0].Name = "Yield Point";
        this.Params.Input[0].NickName = "SPy";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Failure Point";
        this.Params.Input[1].NickName = "SPu";
        this.Params.Input[1].Description = "AdSec Stress Strain Point representing the Failure Point";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit)
      {
        this.Params.Input[0].Name = "StressStrainPts";
        this.Params.Input[0].NickName = "SPs";
        this.Params.Input[0].Description = "AdSec Stress Strain Points representing the StressStrainCurve as a Polyline";
        this.Params.Input[0].Access = GH_ParamAccess.list;
        this.Params.Input[0].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode)
      {
        this.Params.Input[0].Name = "Peak Point";
        this.Params.Input[0].NickName = "SPt";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the FIB model's Peak Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        this.Params.Input[1].NickName = "Ei";
        this.Params.Input[1].Description = "Initial Moduls from FIB model code";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;

        this.Params.Input[2].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        this.Params.Input[2].NickName = "εu";
        this.Params.Input[2].Description = "Failure strain from FIB model code";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Linear)
      {
        this.Params.Input[0].Name = "Failure Point";
        this.Params.Input[0].NickName = "SPu";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Failure Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined)
      {
        this.Params.Input[0].Name = "Unconfined Strength [" + unitStressAbbreviation + "]";
        this.Params.Input[0].NickName = "σU";
        this.Params.Input[0].Description = "Unconfined strength for Mander Confined Model";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Confined Strength [" + unitStressAbbreviation + "]";
        this.Params.Input[1].NickName = "σC";
        this.Params.Input[1].Description = "Confined strength for Mander Confined Model";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;

        this.Params.Input[2].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        this.Params.Input[2].NickName = "Ei";
        this.Params.Input[2].Description = "Initial Moduls for Mander Confined Model";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;

        this.Params.Input[3].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        this.Params.Input[3].NickName = "εu";
        this.Params.Input[3].Description = "Failure strain for Mander Confined Model";
        this.Params.Input[3].Access = GH_ParamAccess.item;
        this.Params.Input[3].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Mander)
      {
        this.Params.Input[0].Name = "Peak Point";
        this.Params.Input[0].NickName = "SPt";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Mander model's Peak Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        this.Params.Input[1].NickName = "Ei";
        this.Params.Input[1].Description = "Initial Moduls for Mander model";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;

        this.Params.Input[2].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        this.Params.Input[2].NickName = "εu";
        this.Params.Input[2].Description = "Failure strain for Mander model";
        this.Params.Input[2].Access = GH_ParamAccess.item;
        this.Params.Input[2].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle)
      {
        this.Params.Input[0].Name = "Yield Point";
        this.Params.Input[0].NickName = "SPy";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        this.Params.Input[1].NickName = "εu";
        this.Params.Input[1].Description = "Failure strain from FIB model code";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Park)
      {
        this.Params.Input[0].Name = "Yield Point";
        this.Params.Input[0].NickName = "SPy";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics)
      {
        this.Params.Input[0].Name = "Peak Point";
        this.Params.Input[0].NickName = "SPt";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Peak Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        this.Params.Input[1].NickName = "εu";
        this.Params.Input[1].Description = "Failure strain from Popovic model";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;
      }

      else if (this._mode == AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular)
      {
        this.Params.Input[0].Name = "Yield Point";
        this.Params.Input[0].NickName = "SPy";
        this.Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        this.Params.Input[0].Access = GH_ParamAccess.item;
        this.Params.Input[0].Optional = false;

        this.Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        this.Params.Input[1].NickName = "εu";
        this.Params.Input[1].Description = "Failure strain";
        this.Params.Input[1].Access = GH_ParamAccess.item;
        this.Params.Input[1].Optional = false;
      }
    }
  }
}
