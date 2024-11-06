using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class EditProfile : GH_OasysDropDownComponent {
    private AngleUnit _angleUnit = AngleUnit.Radian;

    public EditProfile() : base("Edit Profile", "ProfileEdit", "Modify an AdSec Profile", CategoryName.Name(),
      SubCategoryName.Cat2()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
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
      string angleAbbreviation = Angle.GetAbbreviation(_angleUnit);
      IQuantity quantityAngle = new Angle(0, _angleUnit);
      angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));
      Params.Input[1].Name = "Rotation [" + angleAbbreviation + "]";
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

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string angleAbbreviation = Angle.GetAbbreviation(_angleUnit);

      pManager.AddGenericParameter("Profile", "Pf", "AdSet Profile to Edit or get information from",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("Rotation [" + angleAbbreviation + "]", "R",
        "[Optional] The angle at which the profile is rotated. Positive rotation is anti-clockwise around the x-axis in the local coordinate system.",
        GH_ParamAccess.item);
      pManager.AddBooleanParameter("isReflectedY", "rY",
        "[Optional] Reflects the profile over the y-axis in the local coordinate system.", GH_ParamAccess.item);
      pManager.AddBooleanParameter("isReflectedZ", "rZ",
        "[Optional] Reflects the profile over the z-axis in the local coordinate system.", GH_ParamAccess.item);

      // make all but first input optional
      for (int i = 1; i < pManager.ParamCount; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Profile", "Pf", "Modified AdSet Profile", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // #### get material input and duplicate it ####
      var editPrf = AdSecInput.AdSecProfileGoo(this, DA, 0);

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
