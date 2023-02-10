using System;
using System.Collections.Generic;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
  public class CreateProfileFlange : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("c182921f-0ace-49ca-8fb7-5722dbf2ba30");
    public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateFlange;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;

    public CreateProfileFlange() : base(
      "Create Flange",
      "Flange",
      "Create a Flange for AdSec Profile",
      CategoryName.Name(),
      SubCategoryName.Cat2())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      pManager.AddGenericParameter("Width [" + unitAbbreviation + "]", "B", "Flange width", GH_ParamAccess.item);
      pManager.AddGenericParameter("Thickness [" + unitAbbreviation + "]", "t", "Flange thickness", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("FlangeProfile", "Fla", "Flange Profile for AdSec Profile", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      AdSecProfileFlangeGoo flange = new AdSecProfileFlangeGoo(
        IFlange.Create(
          (Length)Input.UnitNumber(this, DA, 0, this._lengthUnit),
          (Length)Input.UnitNumber(this, DA, 1, this._lengthUnit)));

      DA.SetData(0, flange);
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Measure"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // length
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      this.SelectedItems.Add(Length.GetAbbreviation(this._lengthUnit));

      IQuantity quantity = new Length(0, this._lengthUnit);

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      this.SelectedItems[i] = this.DropDownItems[i][j];
      this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[i]);
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      Params.Input[0].Name = "Width [" + unitAbbreviation + "]";
      Params.Input[1].Name = "Thickness [" + unitAbbreviation + "]";
    }
  }
}
