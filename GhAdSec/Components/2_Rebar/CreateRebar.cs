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
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using UnitsNet;

namespace AdSecGH.Components
{
  public class CreateRebar : GH_Component, IGH_VariableParameterComponent
  {
    #region Name and Ribbon Layout
    public CreateRebar()
        : base("Create Rebar", "Rebar", "Create Rebar (single or bundle) for an AdSec Section",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat3())
    { this.Hidden = true; }
    public override Guid ComponentGuid => new Guid("024d241a-b6cc-4134-9f5c-ac9a6dcb2c4b");
    public override GH_Exposure Exposure => GH_Exposure.primary;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.Rebar;
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    public override void CreateAttributes()
    {
      if (first)
      {
        List<string> list = Enum.GetNames(typeof(FoldMode)).ToList();
        dropdownitems = new List<List<string>>();
        dropdownitems.Add(list);

        selecteditems = new List<string>();
        selecteditems.Add(dropdownitems[0][0]);

        // length
        //dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
        dropdownitems.Add(Units.FilteredLengthUnits);
        selecteditems.Add(lengthUnit.ToString());

        IQuantity quantity = new Length(0, lengthUnit);
        unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

        first = false;
      }

      m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
    }

    public void SetSelected(int i, int j)
    {
      // set selected item
      selecteditems[i] = dropdownitems[i][j];
      if (i == 0)
      {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), selecteditems[i]);
        ToggleInput();
      }
      else
      {
        lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[i]);
      }
    }
    private void UpdateUIFromSelectedItems()
    {
      lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[1]);
      CreateAttributes();
      ToggleInput();
    }
    #endregion

    #region Input and output
    List<List<string>> dropdownitems;
    List<string> selecteditems;
    List<string> spacerDescriptions = new List<string>(new string[]
    {
            "Rebar Type",
            "Measure"
    });
    private UnitsNet.Units.LengthUnit lengthUnit = Units.LengthUnit;
    string unitAbbreviation;
    #endregion

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Material", "Mat", "AdSec Reinforcement Material", GH_ParamAccess.item);
      pManager.AddGenericParameter("Diameter [" + unitAbbreviation + "]", "Ø", "Bar Diameter", GH_ParamAccess.item);
      _mode = FoldMode.Single;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Rebar", "Rb", "Rebar (single or bundle) for AdSec Reinforcement", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // 0 material input
      AdSecMaterial material = GetInput.AdSecMaterial(this, DA, 0);

      switch (_mode)
      {
        case FoldMode.Single:
          AdSecRebarBundleGoo rebar = new AdSecRebarBundleGoo(
              IBarBundle.Create(
                  (Oasys.AdSec.Materials.IReinforcement)material.Material,
                  GetInput.Length(this, DA, 1, lengthUnit)));

          DA.SetData(0, rebar);

          break;

        case FoldMode.Bundle:
          int count = 1;
          DA.GetData(2, ref count);

          AdSecRebarBundleGoo bundle = new AdSecRebarBundleGoo(
          IBarBundle.Create(
              (Oasys.AdSec.Materials.IReinforcement)material.Material,
              GetInput.Length(this, DA, 1, lengthUnit),
              count));

          DA.SetData(0, bundle);
          break;
      }
    }

    #region menu override

    private bool first = true;
    private enum FoldMode
    {
      Single,
      Bundle
    }

    private FoldMode _mode = FoldMode.Single;

    private void ToggleInput()
    {
      RecordUndoEvent("Changed dropdown");

      switch (_mode)
      {
        case FoldMode.Single:
          // remove any additional input parameters
          while (Params.Input.Count > 2)
            Params.UnregisterInputParameter(Params.Input[2], true);
          break;

        case FoldMode.Bundle:
          // add input parameter
          while (Params.Input.Count != 3)
            Params.RegisterInputParam(new Param_Integer());
          break;
      }
      ExpireSolution(true);
      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      this.OnDisplayExpired(true);
    }
    #endregion

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
      writer.SetString("mode", _mode.ToString());
      return base.Write(writer);
    }
    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString("mode"));
      UpdateUIFromSelectedItems();
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
      IQuantity quantity = new Length(0, lengthUnit);
      unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
      Params.Input[1].Name = "Diameter [" + unitAbbreviation + "]";

      if (_mode == FoldMode.Bundle)
      {
        Params.Input[2].Name = "Count";
        Params.Input[2].NickName = "N";
        Params.Input[2].Description = "Count per bundle (1, 2, 3 or 4)";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      }
    }
    #endregion
  }
}
