using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.UI;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
  public class CreatePreLoad : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Prestress;
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;

    public CreatePreLoad() : base(
      "Create Prestress",
      "Prestress",
      "Create an AdSec Prestress Load for Reinforcement Layout as either Preforce, Prestrain or Prestress",
      Ribbon.CategoryName.Name(),
      Ribbon.SubCategoryName.Cat5())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string forceUnitAbbreviation = Force.GetAbbreviation(this._forceUnit);
      pManager.AddGenericParameter("RebarGroup", "RbG", "AdSec Reinforcement Group to apply Preload to", GH_ParamAccess.item);
      pManager.AddGenericParameter("Force [" + forceUnitAbbreviation + "]", "P", "The pre-load per reinforcement bar. Positive value is tension.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Prestressed RebarGroup", "RbG", "Preloaded Rebar Group for AdSec Section", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // get rebargroup
      AdSecRebarGroupGoo in_rebar = GetInput.ReinforcementGroup(this, DA, 0);

      IPreload load = null;
      // Create new load
      switch (this.SelectedItems[0])
      {
        case ("Force"):
          load = IPreForce.Create((Force)Input.UnitNumber(this, DA, 1, _forceUnit));
          break;
        case ("Strain"):
          load = IPreStrain.Create((Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
          break;
        case ("Stress"):
          load = IPreStress.Create((Pressure)Input.UnitNumber(this, DA, 1, _stressUnit));
          break;
      }
      ILongitudinalGroup longitudinal = (ILongitudinalGroup)in_rebar.Value;
      longitudinal.Preload = load;
      AdSecRebarGroupGoo out_rebar = new AdSecRebarGroupGoo(longitudinal);
      if (in_rebar.Cover != null)
        out_rebar.Cover = ICover.Create(in_rebar.Cover.UniformCover);

      DA.SetData(0, out_rebar);

      AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Applying prestress will change the up-stream (backwards) rebar object as well " +
          "- please make a copy of the input if you want to have both a rebar with and without prestress. " +
          "This will change in future releases, apologies for the inconvenience...");
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[]{
        "Type",
        "Measure"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // type
      this.DropDownItems.Add(new List<string>() { "Force", "Strain", "Stress" });
      this.SelectedItems.Add(this.DropDownItems[0][0]);

      // force
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      this.SelectedItems.Add(_forceUnit.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // change selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];

      if (i == 0)
      {
        switch (this.SelectedItems[0])
        {
          case ("Force"):
            this.DropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
            this.SelectedItems[0] = _forceUnit.ToString();
            break;
          case ("Strain"):
            this.DropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain);
            this.SelectedItems[0] = _strainUnit.ToString();
            break;
          case ("Stress"):
            this.DropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress);
            this.SelectedItems[0] = _stressUnit.ToString();
            break;
        }
      }
      else
      {
        switch (this.SelectedItems[0])
        {
          case ("Force"):
            _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), this.SelectedItems[i]);
            break;
          case ("Strain"):
            _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), this.SelectedItems[i]);
            break;
          case ("Stress"):
            _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[i]);
            break;
        }
      }
      base.UpdateUI();
    }
    #endregion

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetString("force", this._forceUnit.ToString());
      writer.SetString("strain", this._strainUnit.ToString());
      writer.SetString("stress", this._stressUnit.ToString());
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      this._forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), reader.GetString("force"));
      this._strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), reader.GetString("strain"));
      this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), reader.GetString("stress"));
      return base.Read(reader);
    }
    #endregion

    public override void UpdateUIFromSelectedItems()
    {
      string forceUnitAbbreviation = Force.GetAbbreviation(this._forceUnit);
      string strainUnitAbbreviation = Strain.GetAbbreviation(this._strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(this._stressUnit);
      switch (this.SelectedItems[0])
      {
        case ("Force"):
          Params.Input[1].Name = "Force [" + forceUnitAbbreviation + "]";
          Params.Input[1].NickName = "P";
          break;
        case ("Strain"):
          Params.Input[1].Name = "Strain [" + strainUnitAbbreviation + "]";
          Params.Input[1].NickName = "ε";
          break;
        case ("Stress"):
          Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
          Params.Input[1].NickName = "σ";
          break;
      }
    }
  }
}