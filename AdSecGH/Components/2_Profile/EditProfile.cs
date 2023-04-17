using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdSecGH.Components {
  public class EditProfile : GH_OasysDropDownComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("78f26bee-c72c-4d88-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.EditProfile;
    private AngleUnit _angleUnit = AngleUnit.Radian;

    public EditProfile() : base(
      "Edit Profile",
      "ProfileEdit",
      "Modify an AdSec Profile",
      CategoryName.Name(),
      SubCategoryName.Cat2()) {
      this.Hidden = false; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      this._selectedItems[i] = this._dropDownItems[i][j];
      this._angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), this._selectedItems[i]);
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string angleAbbreviation = Angle.GetAbbreviation(this._angleUnit);
      IQuantity quantityAngle = new Angle(0, _angleUnit);
      angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));
      Params.Input[1].Name = "Rotation [" + angleAbbreviation + "]";
    }

    protected override void InitialiseDropdowns() {
      this._spacerDescriptions = new List<string>(new string[] {
        "Measure"
      });

      this._dropDownItems = new List<List<string>>();
      this._selectedItems = new List<string>();

      this._dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Angle));
      this._selectedItems.Add(_angleUnit.ToString());

      this._isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string angleAbbreviation = Angle.GetAbbreviation(this._angleUnit);

      pManager.AddGenericParameter("Profile", "Pf", "AdSet Profile to Edit or get information from", GH_ParamAccess.item);
      pManager.AddGenericParameter("Rotation [" + angleAbbreviation + "]", "R", "[Optional] The angle at which the profile is rotated. Positive rotation is anti-clockwise around the x-axis in the local coordinate system.", GH_ParamAccess.item);
      pManager.AddBooleanParameter("isReflectedY", "rY", "[Optional] Reflects the profile over the y-axis in the local coordinate system.", GH_ParamAccess.item);
      pManager.AddBooleanParameter("isReflectedZ", "rZ", "[Optional] Reflects the profile over the z-axis in the local coordinate system.", GH_ParamAccess.item);

      // make all but first input optional
      for (int i = 1; i < pManager.ParamCount; i++)
        pManager[i].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Profile", "Pf", "Modified AdSet Profile", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // #### get material input and duplicate it ####
      AdSecProfileGoo editPrf = AdSecInput.AdSecProfileGoo(this, DA, 0);

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
      this._angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), this._selectedItems[0]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
