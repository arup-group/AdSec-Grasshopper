using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using Rhino.Geometry;
using UnitsNet;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create a new Stress Strain Point
  /// </summary>
  public class NMDiagram : GH_OasysComponent, IGH_VariableParameterComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("21cd9e4c-6c85-4077-b575-1e04127f2998");
    public NMDiagram()
      : base("N-M Diagram", "N-M", "Calculates a force-moment (N-M) or moment-moment (M-M) interaction curve.",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat7())
    { this.Hidden = false; } // sets the initial state of the component to hidden
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.N_M;

    public override OasysPluginInfo PluginInfo => AdSecGHPluginInfo.Instance;
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    public override void CreateAttributes()
    {
      if (first)
      {
        dropdownitems = new List<List<string>>();
        selecteditems = new List<string>();

        // type
        List<string> types = new List<string>() { "N-M", "M-M" };
        dropdownitems.Add(types);
        selecteditems.Add(dropdownitems[0][0]);

        // force
        dropdownitems.Add(Units.FilteredAngleUnits);
        selecteditems.Add(angleUnit.ToString());

        IQuantity force = new Force(0, forceUnit);
        forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
        IQuantity angle = new Angle(0, angleUnit);
        angleUnitAbbreviation = string.Concat(angle.ToString().Where(char.IsLetter));

        first = false;
      }

      m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
    }

    public void SetSelected(int i, int j)
    {
      // change selected item
      selecteditems[i] = dropdownitems[i][j];

      if (i == 0)
      {
        switch (selecteditems[0])
        {
          case ("N-M"):
            _mode = FoldMode.NM;
            dropdownitems[1] = Units.FilteredAngleUnits;
            selecteditems[1] = angleUnit.ToString();
            break;

          case ("M-M"):
            _mode = FoldMode.MM;
            dropdownitems[1] = Units.FilteredForceUnits;
            selecteditems[1] = forceUnit.ToString();
            break;
        }
      }
      else
      {
        switch (selecteditems[0])
        {
          case ("N-M"):
            angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), selecteditems[i]);
            break;
          case ("M-M"):
            forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), selecteditems[i]);
            break;
        }
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
            "Interaction",
            "Measure"
    });
    private bool first = true;

    private UnitsNet.Units.ForceUnit forceUnit = Units.ForceUnit;
    private UnitsNet.Units.AngleUnit angleUnit = UnitsNet.Units.AngleUnit.Radian;
    string forceUnitAbbreviation;
    string angleUnitAbbreviation;
    #endregion

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Results", "Res", "AdSec Results to calculate interaction diagram from", GH_ParamAccess.item);
      pManager.AddGenericParameter("Moment Angle [" + angleUnitAbbreviation + "]", "A", "[Default 0] The moment angle, which must be in the range -180 degrees to +180 degrees. Angle of zero equals Nx-Myy diagram.", GH_ParamAccess.item);
      pManager[1].Optional = true;
      // create default rectangle as 1/2 meter square
      Length sz = Length.FromMeters(0.5);
      Rectangle3d rect = new Rectangle3d(Plane.WorldXY, sz.As(Units.LengthUnit), sz.As(Units.LengthUnit));
      pManager.AddRectangleParameter("Plot", "R", "Rectangle for plot boundary", GH_ParamAccess.item, rect);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("N-M Curve", "NM", "AdSec Force-Moment (N-M) interaction diagram", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      //AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "This component is WIP and currently does not place the NM diagram on an XY plane");

      // get solution input
      AdSecSolutionGoo solution = GetInput.Solution(this, DA, 0);

      // Get boundary input
      Rectangle3d rect = new Rectangle3d();
      if (!DA.GetData(2, ref rect))
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[2].NickName + " to Rectangle");
        return;
      }

      if (_mode == FoldMode.NM)
      {
        // get angle input
        Angle angle = GetInput.Angle(this, DA, 1, angleUnit, true);

        // get loadcurve
        Oasys.Collections.IList<Oasys.AdSec.Mesh.ILoadCurve> loadCurve = solution.Value.Strength.GetForceMomentInteractionCurve(angle);

        // create output
        DA.SetData(0, new AdSecNMMCurveGoo(loadCurve[0], angle, rect));
      }
      else
      {
        // get force input
        Force force = GetInput.Force(this, DA, 1, forceUnit, true);

        // get loadcurve
        Oasys.Collections.IList<Oasys.AdSec.Mesh.ILoadCurve> loadCurve = solution.Value.Strength.GetMomentMomentInteractionCurve(force);

        // check if curve is valid
        if (loadCurve.Count == 0)
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input axial force is outside the capacity range of the section");
          return;
        }

        // create output
        DA.SetData(0, new AdSecNMMCurveGoo(loadCurve[0], rect));
      }
    }

    private enum FoldMode
    {
      NM,
      MM
    }
    private FoldMode _mode = FoldMode.NM;

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
      writer.SetString("force", forceUnit.ToString());
      writer.SetString("angle", angleUnit.ToString());
      return base.Write(writer);
    }
    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

      forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), reader.GetString("force"));
      angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), reader.GetString("angle"));

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
      if (selecteditems == null) return;

      switch (selecteditems[0])
      {
        case ("N-M"):
          IQuantity angle = new Angle(0, angleUnit);
          angleUnitAbbreviation = string.Concat(angle.ToString().Where(char.IsLetter));
          Params.Input[1].Name = "Moment Angle [" + angleUnitAbbreviation + "]";
          Params.Input[1].NickName = "A";
          Params.Input[1].Description = "[Default 0] The moment angle, which must be in the range -180 degrees to +180 degrees. Angle of zero equals Nx-Myy diagram.";
          Params.Output[0].Name = "N-M Curve";
          Params.Output[0].NickName = "NM";
          Params.Output[0].Description = "AdSec Force-Moment (N-M) interaction diagram";
          break;
        case ("M-M"):
          IQuantity force = new Force(0, forceUnit);
          forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
          Params.Input[1].Name = "Axial Force [" + forceUnitAbbreviation + "]";
          Params.Input[1].NickName = "F";
          Params.Input[1].Description = "[Default 0] The axial force to calculate the moment-moment diagram for.";
          Params.Output[0].Name = "M-M Curve";
          Params.Output[0].NickName = "MM";
          Params.Output[0].Description = "AdSec Moment-Moment (M-M) interaction diagram";
          break;
      }
    }
    #endregion

  }
}