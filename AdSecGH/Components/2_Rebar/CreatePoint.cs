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
    public class CreatePoint : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("1a0cdb3c-d66d-420e-a9d8-35d31587a122");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.VertexPoint;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;

    public CreatePoint() : base(
      "Create Vertex Point",
      "Vertex Point",
      "Create a 2D vertex in local yz-plane for AdSec Profile and Reinforcement",
      CategoryName.Name(),
      SubCategoryName.Cat3())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      pManager.AddGenericParameter("Y [" + unitAbbreviation + "]", "Y", "The local Y coordinate in yz-plane", GH_ParamAccess.item);
      pManager.AddGenericParameter("Z [" + unitAbbreviation + "]", "Z", "The local Z coordinate in yz-plane", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Vertex Point", "Vx", "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // get inputs
      Length y = (Length)Input.UnitNumber(this, DA, 0, this._lengthUnit);
      Length z = (Length)Input.UnitNumber(this, DA, 1, this._lengthUnit);

      // create IPoint
      IPoint pt = IPoint.Create(y, z);

      // Convert to AdSecPointGoo param
      AdSecPointGoo point = new AdSecPointGoo(pt);

      // set output
      DA.SetData(0, point);
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Measure"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      this.SelectedItems.Add(Length.GetAbbreviation(this._lengthUnit));

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

    public override void VariableParameterMaintenance()
    {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      Params.Input[0].Name = "Y [" + unitAbbreviation + "]";
      Params.Input[1].Name = "Z [" + unitAbbreviation + "]";
    }
    #endregion
  }
}
