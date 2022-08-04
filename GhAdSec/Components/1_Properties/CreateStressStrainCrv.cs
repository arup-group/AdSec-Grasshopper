using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Oasys.AdSec.Materials.StressStrainCurves;
using AdSecGH.Parameters;
using UnitsNet.GH;
using UnitsNet;

namespace AdSecGH.Components
{
    public class CreateStressStrainCurve : GH_OasysComponent, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateStressStrainCurve()
            : base("Create StressStrainCrv", "StressStrainCrv", "Create a Stress Strain Curve for AdSec Material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = false; }
        public override Guid ComponentGuid => new Guid("b2ddf545-2a4c-45ac-ba1c-cb0f3da5b37f");
        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

    protected override string HtmlHelp_Source()
    {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Materials.StressStrainCurves.html";
      return help;
    }
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainCrv;
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    public override void CreateAttributes()
    {
      if (first)
      {
        selecteditems = new List<string>();
        selecteditems.Add(_mode.ToString());

        dropdownitems = new List<List<string>>();
        dropdownitems.Add(Enum.GetNames(typeof(AdSecStressStrainCurveGoo.StressStrainCurveType)).ToList());
        dropdownitems[0].RemoveAt(dropdownitems[0].Count - 1);
        first = false;
      }

      m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
    }

    public void SetSelected(int i, int j)
    {
      // set selected item
      selecteditems[i] = dropdownitems[i][j];

      // toggle case
      if (i == 0)
      {
        // remove dropdown lists beyond first level
        while (dropdownitems.Count > 1)
          dropdownitems.RemoveAt(1);

        switch (j)
        {
          case 0:
            Mode0Clicked();
            break;
          case 1:
            Mode1Clicked();
            break;
          case 2:
            // add strain dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
            dropdownitems.Add(Units.FilteredStrainUnits);
            selecteditems.Add(strainUnit.ToString());

            // add pressure dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
            dropdownitems.Add(Units.FilteredStressUnits);
            selecteditems.Add(stressUnit.ToString());

            Mode2Clicked();
            break;
          case 3:
            Mode3Clicked();
            break;
          case 4:
            // add strain dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
            dropdownitems.Add(Units.FilteredStrainUnits);
            selecteditems.Add(strainUnit.ToString());

            // add pressure dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
            dropdownitems.Add(Units.FilteredStressUnits);
            selecteditems.Add(stressUnit.ToString());

            Mode4Clicked();
            break;
          case 5:
            Mode5Clicked();
            break;
          case 6:
            // add strain dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
            dropdownitems.Add(Units.FilteredStrainUnits);
            selecteditems.Add(strainUnit.ToString());

            Mode6Clicked();
            break;
          case 7:
            Mode7Clicked();
            break;
          case 8:
            // add strain dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
            dropdownitems.Add(Units.FilteredStrainUnits);
            selecteditems.Add(strainUnit.ToString());
            Mode8Clicked();
            break;
          case 9:
            // add strain dropdown
            //dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
            dropdownitems.Add(Units.FilteredStrainUnits);
            selecteditems.Add(strainUnit.ToString());
            Mode9Clicked();
            break;

        }
      }
      else
      {
        switch (i)
        {
          case 1:
            strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[i]);
            break;
          case 2:
            stressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
            break;
        }
      }
        (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      ExpireSolution(true);
      Params.OnParametersChanged();
      this.OnDisplayExpired(true);
    }
    private void UpdateUIFromSelectedItems()
    {
      switch (_mode)
      {
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)0:
          Mode0Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)1:
          Mode1Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)2:
          Mode2Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)3:
          Mode3Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)4:
          Mode4Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)5:
          Mode5Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)6:
          Mode6Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)7:
          Mode7Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)8:
          Mode8Clicked();
          break;
        case (AdSecStressStrainCurveGoo.StressStrainCurveType)9:
          Mode9Clicked();
          break;
      }
      CreateAttributes();
      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      ExpireSolution(true);
      Params.OnParametersChanged();
      this.OnDisplayExpired(true);
    }
    #endregion

    #region Input and output
    List<List<string>> dropdownitems;
    List<string> selecteditems;
    List<string> spacerDescriptions = new List<string>(new string[]
    {
            "Curve Type",
            "Strain Unit",
            "Stress Unit",
    });
    private Oasys.Units.StrainUnit strainUnit = Units.StrainUnit;
    private UnitsNet.Units.PressureUnit stressUnit = Units.StressUnit;

    string unitStressAbbreviation;
    string unitStrainAbbreviation;
    #endregion

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Failure", "SPu", "AdSec Stress Strain Point representing the Failure Point", GH_ParamAccess.item);
      _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("StressStrainCrv", "SCv", "AdSec Stress Strain Curve", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      IStressStrainCurve crv = null;

      try
      {
        switch (_mode)
        {
          case AdSecStressStrainCurveGoo.StressStrainCurveType.Bilinear:

            crv = IBilinearStressStrainCurve.Create(GetInput.StressStrainPoint(this, DA, 0), GetInput.StressStrainPoint(this, DA, 1));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit:

            IExplicitStressStrainCurve exCrv = IExplicitStressStrainCurve.Create();
            exCrv.Points = GetInput.StressStrainPoints(this, DA, 0);
            crv = exCrv;
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.FibModelCode:

            crv = IFibModelCodeStressStrainCurve.Create(
                GetInput.Stress(this, DA, 1, stressUnit),
                GetInput.StressStrainPoint(this, DA, 0),
                GetInput.Strain(this, DA, 2, strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Mander:

            crv = IManderStressStrainCurve.Create(
                GetInput.Stress(this, DA, 1, stressUnit),
                GetInput.StressStrainPoint(this, DA, 0),
                GetInput.Strain(this, DA, 2, strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Linear:

            crv = ILinearStressStrainCurve.Create(
                GetInput.StressStrainPoint(this, DA, 0));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ManderConfined:

            crv = IManderConfinedStressStrainCurve.Create(
                GetInput.Stress(this, DA, 0, stressUnit),
                GetInput.Stress(this, DA, 1, stressUnit),
                GetInput.Stress(this, DA, 2, stressUnit),
                GetInput.Strain(this, DA, 3, strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.ParabolaRectangle:

            crv = IParabolaRectangleStressStrainCurve.Create(
                GetInput.StressStrainPoint(this, DA, 0),
                GetInput.Strain(this, DA, 1, strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Park:

            crv = IParkStressStrainCurve.Create(
                GetInput.StressStrainPoint(this, DA, 0));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Popovics:

            crv = IPopovicsStressStrainCurve.Create(
                GetInput.StressStrainPoint(this, DA, 0),
                GetInput.Strain(this, DA, 1, strainUnit));
            break;

          case AdSecStressStrainCurveGoo.StressStrainCurveType.Rectangular:


            crv = IRectangularStressStrainCurve.Create(
                GetInput.StressStrainPoint(this, DA, 0),
                GetInput.Strain(this, DA, 1, strainUnit));
            break;
        }
      }
      catch (Exception e)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
        return;
      }


      // create preview
      Tuple<Curve, List<Point3d>> tuple = AdSecStressStrainCurveGoo.Create(crv, _mode, true);

      DA.SetData(0, new AdSecStressStrainCurveGoo(tuple.Item1, crv, _mode, tuple.Item2));
    }

    #region menu override
    //internal enum GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType
    //{
    //    Bilinear,
    //    Explicit,
    //    FibModelCode,
    //    Linear,
    //    ManderConfined,
    //    Mander,
    //    ParabolaRectangle,
    //    Park,
    //    Popovics,
    //    Rectangular
    //}
    private bool first = true;
    private AdSecStressStrainCurveGoo.StressStrainCurveType _mode = AdSecStressStrainCurveGoo.StressStrainCurveType.Linear;
    private bool comingFromSave = false;
    private void Mode0Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)0)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)0;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      while (Params.Input.Count != 2)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode1Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)1)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)1;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode2Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)2)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)2;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
      while (Params.Input.Count != 3)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode3Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)3)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)3;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode4Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        if (!comingFromSave) { return; }

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)4;

      //remove input parameters
      while (Params.Input.Count > 0)
        Params.UnregisterInputParameter(Params.Input[0], true);
      while (Params.Input.Count != 4)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode5Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)5)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)5;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
      while (Params.Input.Count != 3)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode6Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)6)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)6;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
      while (Params.Input.Count != 2)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode7Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)7)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)7;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode8Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)8)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)8;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
      while (Params.Input.Count != 2)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    private void Mode9Clicked()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)9)
        if (!comingFromSave) { return; }

      bool cleanAll = false;
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
        cleanAll = true;

      RecordUndoEvent("Changed dropdown");
      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)9;

      //remove input parameters
      int i = cleanAll ? 0 : 1;
      while (Params.Input.Count > i)
        Params.UnregisterInputParameter(Params.Input[i], true);
      if (cleanAll)
        Params.RegisterInputParam(new Param_GenericObject());
      while (Params.Input.Count != 2)
        Params.RegisterInputParam(new Param_GenericObject());
    }
    #endregion

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
      writer.SetString("mode", _mode.ToString());
      writer.SetString("strain_mode", strainUnit.ToString());
      writer.SetString("stress_mode", stressUnit.ToString());
      return base.Write(writer);
    }
    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

      strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), reader.GetString("strain_mode"));
      stressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), reader.GetString("stress_mode"));

      _mode = (AdSecStressStrainCurveGoo.StressStrainCurveType)Enum.Parse(
          typeof(AdSecStressStrainCurveGoo.StressStrainCurveType), reader.GetString("mode"));
      comingFromSave = true;
      UpdateUIFromSelectedItems();
      comingFromSave = false;
      first = false;
      return base.Read(reader);
    }

    bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
    {
      return false;
    }
    bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
    {
      return false;
    }
    IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
    {
      return null;
    }
    bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
    {
      return false;
    }
    #endregion
    #region IGH_VariableParameterComponent null implementation
    void IGH_VariableParameterComponent.VariableParameterMaintenance()
    {
      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)0)
      {
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
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)1)
      {
        Params.Input[0].Name = "StressStrainPts";
        Params.Input[0].NickName = "SPs";
        Params.Input[0].Description = "AdSec Stress Strain Points representing the StressStrainCurve as a Polyline";
        Params.Input[0].Access = GH_ParamAccess.list;
        Params.Input[0].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)2)
      {
        Params.Input[0].Name = "Peak Point";
        Params.Input[0].NickName = "SPt";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the FIB model's Peak Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        IQuantity quantityStress = new Pressure(0, stressUnit);
        unitStressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));

        Params.Input[1].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        Params.Input[1].NickName = "Ei";
        Params.Input[1].Description = "Initial Moduls from FIB model code";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        unitStrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);

        Params.Input[2].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[2].NickName = "εu";
        Params.Input[2].Description = "Failure strain from FIB model code";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)3)
      {
        Params.Input[0].Name = "Failure Point";
        Params.Input[0].NickName = "SPu";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Failure Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)4)
      {
        IQuantity quantityStress = new Pressure(0, stressUnit);
        unitStressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));

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

        unitStrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);

        Params.Input[3].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[3].NickName = "εu";
        Params.Input[3].Description = "Failure strain for Mander Confined Model";
        Params.Input[3].Access = GH_ParamAccess.item;
        Params.Input[3].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)5)
      {
        Params.Input[0].Name = "Peak Point";
        Params.Input[0].NickName = "SPt";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Mander model's Peak Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        IQuantity quantityStress = new Pressure(0, stressUnit);
        unitStressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));

        Params.Input[1].Name = "Initial Modus [" + unitStressAbbreviation + "]";
        Params.Input[1].NickName = "Ei";
        Params.Input[1].Description = "Initial Moduls for Mander model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;

        unitStrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);

        Params.Input[2].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[2].NickName = "εu";
        Params.Input[2].Description = "Failure strain for Mander model";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)6)
      {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        unitStrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);

        Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain from FIB model code";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)7)
      {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)8)
      {
        Params.Input[0].Name = "Peak Point";
        Params.Input[0].NickName = "SPt";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Peak Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        unitStrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);

        Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain from Popovic model";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }

      if (_mode == (AdSecStressStrainCurveGoo.StressStrainCurveType)9)
      {
        Params.Input[0].Name = "Yield Point";
        Params.Input[0].NickName = "SPy";
        Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        unitStrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);

        Params.Input[1].Name = "Failure Strain [" + unitStrainAbbreviation + "]";
        Params.Input[1].NickName = "εu";
        Params.Input[1].Description = "Failure strain";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }
    }
    #endregion
  }
}
