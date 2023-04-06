using System;
using System.Collections.Generic;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
    public class CreateLoad : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b74-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateLoad;
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private MomentUnit _momentUnit = DefaultUnits.MomentUnit;

    public CreateLoad() : base(
      "Create" + AdSecLoadGoo.Name.Replace(" ", string.Empty),
      AdSecLoadGoo.Name.Replace(" ", string.Empty),
      "Create an" + AdSecLoadGoo.Description + " from an axial force and biaxial moments",
      CategoryName.Name(),
      SubCategoryName.Cat5())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string forceUnitAbbreviation = Force.GetAbbreviation(this._forceUnit);
      string momentUnitAbbreviation = Moment.GetAbbreviation(this._momentUnit);
      pManager.AddGenericParameter("Fx [" + forceUnitAbbreviation + "]", "X", "The axial force. Positive x is tension.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Myy [" + momentUnitAbbreviation + "]", "YY", "The moment about local y-axis. Positive yy is anti - clockwise moment about local y-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("Mzz [" + momentUnitAbbreviation + "]", "ZZ", "The moment about local z-axis. Positive zz is anti - clockwise moment about local z-axis.", GH_ParamAccess.item);
      // make all but last input optional
      for (int i = 0; i < pManager.ParamCount - 1; i++)
        pManager[i].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter(AdSecLoadGoo.Name, AdSecLoadGoo.NickName, AdSecLoadGoo.Description, GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // Create new load
      ILoad load = ILoad.Create(
        (Force)Input.UnitNumber(this, DA, 0, this._forceUnit, true),
        (Moment)Input.UnitNumber(this, DA, 1, this._momentUnit, true),
        (Moment)Input.UnitNumber(this, DA, 2, this._momentUnit, true));

      // check for enough input parameters
      if (this.Params.Input[0].SourceCount == 0 && this.Params.Input[1].SourceCount == 0
          && this.Params.Input[2].SourceCount == 0)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameters " + this.Params.Input[0].NickName + ", " +
            this.Params.Input[1].NickName + ", and " + this.Params.Input[2].NickName + " failed to collect data!");
        return;
      }

      DA.SetData(0, new AdSecLoadGoo(load));
    }

    #region Custom UI
    protected override void InitialiseDropdowns()
    {
      this._spacerDescriptions = new List<string>(new string[] {
        "Force Unit",
        "Moment Unit"
      });

      this._dropDownItems = new List<List<string>>();
      this._selectedItems = new List<string>();

      // force
      this._dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      this._selectedItems.Add(Force.GetAbbreviation(this._forceUnit));

      // moment
      this._dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Moment));
      this._selectedItems.Add(Moment.GetAbbreviation(this._momentUnit));

      this._isInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      this._selectedItems[i] = this._dropDownItems[i][j];

      switch (i)
      {
        case 0:
          this._forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), this._selectedItems[i]);
          break;
        case 1:
          this._momentUnit = (MomentUnit)UnitsHelper.Parse(typeof(MomentUnit), this._selectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    protected override void UpdateUIFromSelectedItems()
    {
      this._forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), this._selectedItems[0]);
      this._momentUnit = (MomentUnit)UnitsHelper.Parse(typeof(MomentUnit), this._selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }

    public override void VariableParameterMaintenance()
    {
      string forceUnitAbbreviation = Force.GetAbbreviation(this._forceUnit);
      string momentUnitAbbreviation = Moment.GetAbbreviation(this._momentUnit);
      Params.Input[0].Name = "Fx [" + forceUnitAbbreviation + "]";
      Params.Input[1].Name = "Myy [" + momentUnitAbbreviation + "]";
      Params.Input[2].Name = "Mzz [" + momentUnitAbbreviation + "]";
    }
    #endregion
  }
}
