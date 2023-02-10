using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
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
    public class CreateProfileWeb : GH_OasysDropDownComponent
  {
    private enum FoldMode
    {
      Constant,
      Tapered
    }
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("0f9a9223-e745-44b9-add2-8b2e5950e86a");
    public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateWeb;
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Constant;

    public CreateProfileWeb() : base(
      "Create Web",
      "Web",
      "Create a Web for AdSec Profile",
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
      pManager.AddGenericParameter("Thickness [" + unitAbbreviation + "]", "t", "Web thickness", GH_ParamAccess.item);
      this._mode = FoldMode.Constant;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("WebProfile", "Web", "Web Profile for AdSec Profile", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      switch (_mode)
      {
        case FoldMode.Constant:

          AdSecProfileWebGoo webConst = new AdSecProfileWebGoo(
            IWebConstant.Create(
              (Length)Input.UnitNumber(this, DA, 0, this._lengthUnit)));

          DA.SetData(0, webConst);
          break;

        case FoldMode.Tapered:
          AdSecProfileWebGoo webTaper = new AdSecProfileWebGoo(
            IWebTapered.Create(
              (Length)Input.UnitNumber(this, DA, 0, this._lengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, this._lengthUnit)));

          DA.SetData(0, webTaper);
          break;
      }
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Web Type",
        "Measure"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      this.DropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      this.SelectedItems.Add(this.DropDownItems[0][0]);

      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      this.SelectedItems.Add(this._lengthUnit.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // set selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];
      if (i == 0)
        this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), this.SelectedItems[i]);
      else
        this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[i]);
      this.ToggleInput();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this._lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    #region menu override
    private void ToggleInput()
    {
      switch (this._mode)
      {
        case FoldMode.Constant:
          // remove any additional input parameters
          while (Params.Input.Count > 1)
            Params.UnregisterInputParameter(Params.Input[1], true);
          break;

        case FoldMode.Tapered:
          // add input parameter
          while (Params.Input.Count != 2)
            Params.RegisterInputParam(new Param_GenericObject());
          break;
      }

      base.UpdateUI();
    }
    #endregion

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetString("mode", this._mode.ToString());
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString("mode"));
      return base.Read(reader);
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      string unitAbbreviation = Length.GetAbbreviation(this._lengthUnit);
      if (this._mode == FoldMode.Constant)
      {
        Params.Input[0].Name = "Thickness [" + unitAbbreviation + "]";
        Params.Input[0].NickName = "t";
        Params.Input[0].Description = "Web thickness";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

      }
      if (this._mode == FoldMode.Tapered)
      {
        Params.Input[0].Name = "Top Thickness [" + unitAbbreviation + "]";
        Params.Input[0].NickName = "Tt";
        Params.Input[0].Description = "Web thickness at the top";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = false;

        Params.Input[1].Name = "Bottom Thickness [" + unitAbbreviation + "]";
        Params.Input[1].NickName = "Bt";
        Params.Input[1].Description = "Web thickness at the bottom";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }
    }
  }
}
