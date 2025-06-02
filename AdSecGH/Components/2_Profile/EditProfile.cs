using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Helpers;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class EditProfile : DropdownAdapter<EditProfileFunction> {
    private AngleUnit _angleUnit = AngleUnit.Radian;

    public override Guid ComponentGuid => new Guid("78f26bee-c72c-4d88-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditProfile;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[i]);
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      IQuantity quantityAngle = new Angle(0, _angleUnit);
      string angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));
      Params.Input[1].Name = $"Rotation [{angleAbbreviation}]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle));
      _selectedItems.Add(_angleUnit.ToString());

      _isInitialised = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Profile", "Pf", "Modified AdSet Profile", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // #### get material input and duplicate it ####
      var editPrf = this.GetAdSecProfileGoo(DA, 0);

      if (editPrf != null) {
        // #### get the remaining inputs ####

        // 1 Rotation
        if (Params.Input[1].SourceCount > 0) {
          editPrf.Rotation = (Angle)Input.UnitNumber(this, DA, 1, _angleUnit);
        }

        // 2 ReflectionY
        bool refY = false;
        if (DA.GetData(2, ref refY)) {
          editPrf.IsReflectedY = refY;
        }

        // 3 Reflection3
        bool refZ = false;
        if (DA.GetData(3, ref refZ)) {
          editPrf.IsReflectedZ = refZ;
        }

        DA.SetData(0, new AdSecProfileGoo(editPrf.Profile, editPrf.LocalPlane));
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
