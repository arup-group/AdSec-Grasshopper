using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using OasysGH;
using OasysGH.Components;
using Oasys.Units;
using UnitsNet;
using UnitsNet.Units;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create a new Load
  /// </summary>
  public class CreateLoad : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b74-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateLoad;

    public CreateLoad() : base("Create" + AdSecLoadGoo.Name.Replace(" ", string.Empty),
      AdSecLoadGoo.Name.Replace(" ", string.Empty),
      "Create an" + AdSecLoadGoo.Description + " from an axial force and biaxial moments",
      Ribbon.CategoryName.Name(), Ribbon.SubCategoryName.Cat5())
    { this.Hidden = true; } // sets the initial state of the component to hidden
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string forceUnitAbbreviation = Force.GetAbbreviation(this.ForceUnit);
      string momentUnitAbbreviation = Moment.GetAbbreviation(this.MomentUnit);
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
        GetInput.Force(this, DA, 0, this.ForceUnit, true),
        GetInput.Moment(this, DA, 1, this.MomentUnit, true),
        GetInput.Moment(this, DA, 2, this.MomentUnit, true));

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
    private ForceUnit ForceUnit = Units.ForceUnit;
    private MomentUnit MomentUnit = Units.MomentUnit;

    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Force Unit",
        "Moment Unit"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // force
      this.DropDownItems.Add(Units.FilteredForceUnits);
      this.SelectedItems.Add(this.ForceUnit.ToString());

      // moment
      this.DropDownItems.Add(Units.FilteredMomentUnits);
      this.SelectedItems.Add(this.MomentUnit.ToString());


      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // change selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];

      switch (i)
      {
        case 0:
          this.ForceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), this.SelectedItems[i]);
          break;
        case 1:
          this.MomentUnit = (Oasys.Units.MomentUnit)Enum.Parse(typeof(Oasys.Units.MomentUnit), this.SelectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this.ForceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), this.SelectedItems[0]);
      this.MomentUnit = (Oasys.Units.MomentUnit)Enum.Parse(typeof(Oasys.Units.MomentUnit), this.SelectedItems[1]);

      base.UpdateUIFromSelectedItems();
    }

    public override void VariableParameterMaintenance()
    {
      //string forceUnitAbbreviation = string.Concat(new Force(0, this.ForceUnit).ToString().Where(char.IsLetter));
      string forceUnitAbbreviation = Force.GetAbbreviation(this.ForceUnit);
      string momentUnitAbbreviation = Moment.GetAbbreviation(this.MomentUnit);
      Params.Input[0].Name = "Fx [" + forceUnitAbbreviation + "]";
      Params.Input[1].Name = "Myy [" + momentUnitAbbreviation + "]";
      Params.Input[2].Name = "Mzz [" + momentUnitAbbreviation + "]";
    }
    #endregion
  }
}