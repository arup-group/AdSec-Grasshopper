using System;
using System.Collections.Generic;
using System.Linq;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create a new Stress Strain Point
  /// </summary>
  public class CreateStressStrainPoint : GH_OasysComponent, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("69a789d4-c11b-4396-b237-a10efdd6d0c4");
        public CreateStressStrainPoint()
          : base("Create StressStrainPt", "StressStrainPt", "Create a Stress Strain Point for AdSec Stress Strain Curve",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = false; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainPt;
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    public override void CreateAttributes()
    {
      if (first)
      {
        dropdownitems = new List<List<string>>();
        selecteditems = new List<string>();

        // strain
        //dropdownitems.Add(Enum.GetNames(typeof(StrainUnit)).ToList());
        dropdownitems.Add(Units.FilteredStrainUnits);
        selecteditems.Add(strainUnit.ToString());

        // pressure
        //dropdownitems.Add(Enum.GetNames(typeof(Units.PressureUnit)).ToList());
        dropdownitems.Add(Units.FilteredStressUnits);
        selecteditems.Add(stressUnit.ToString());

        strainUnitAbbreviation = Strain.GetAbbreviation(strainUnit);
        IQuantity stress = new Pressure(0, stressUnit);
        stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));

        first = false;
      }

      m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
    }

    public void SetSelected(int i, int j)
    {
      // change selected item
      selecteditems[i] = dropdownitems[i][j];

      switch (i)
      {
        case 0:
          strainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), selecteditems[i]);
          break;
        case 1:
          stressUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), selecteditems[i]);
          break;
      }

        // update name of inputs (to display unit on sliders)
        (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      ExpireSolution(true);
      Params.OnParametersChanged();
      this.OnDisplayExpired(true);
    }
    private void UpdateUIFromSelectedItems()
    {
      CreateAttributes();
      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      ExpireSolution(true);
      Params.OnParametersChanged();
      this.OnDisplayExpired(true);
    }
    #endregion

    #region Input and output

    // list of lists with all dropdown lists conctent
    List<List<string>> dropdownitems;
    // list of selected items
    List<string> selecteditems;
    // list of descriptions 
    List<string> spacerDescriptions = new List<string>(new string[]
    {
            "Strain Unit",
            "Stress Unit"
    });
    private bool first = true;

    private StrainUnit strainUnit = Units.StrainUnit;
    private PressureUnit stressUnit = Units.StressUnit;
    string strainUnitAbbreviation;
    string stressUnitAbbreviation;
    #endregion

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Strain [" + strainUnitAbbreviation + "]", "ε", "Value for strain (X-axis)", GH_ParamAccess.item);
      pManager.AddGenericParameter("Stress [" + stressUnitAbbreviation + "]", "σ", "Value for stress (Y-axis)", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("StressStrainPt", "SPt", "AdSec Stress Strain Point", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {

      // create new point
      AdSecStressStrainPointGoo pt = new AdSecStressStrainPointGoo(
          GetInput.GetStress(this, DA, 1, stressUnit),
          GetInput.GetStrain(this, DA, 0, strainUnit));

      DA.SetData(0, pt);
    }

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

      return base.Write(writer);
    }
    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

      strainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), selecteditems[0]);
      stressUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), selecteditems[1]);
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
      strainUnitAbbreviation = Strain.GetAbbreviation(strainUnit);
      IQuantity stress = new Pressure(0, stressUnit);
      stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));
      Params.Input[0].Name = "Strain [" + strainUnitAbbreviation + "]";
      Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
    }
    #endregion
  }
}