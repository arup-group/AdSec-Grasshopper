using System;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.Units;
using UnitsNet;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using AdSecGH.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;

namespace AdSecGH.Components
{
  public class CreateSubcomponent : GH_Component
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("38747c89-01a4-4388-921b-8c8d8cbca410");
    public CreateSubcomponent()
      : base("SubComponent", "SubComponent", "Create an AdSec Subcomponent from a Section",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat4())
    { this.Hidden = false; } // sets the initial state of the component to hidden

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.SubComponent;
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    #endregion

    #region Input and output

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to create Subcomponent from", GH_ParamAccess.item);
      pManager.AddGenericParameter("Offset", "Off", "[Optional] Section offset (Vertex Point)." + Environment.NewLine +
          "Offset is applied between origins of containing section and sub-component. The offset of the profile is in " +
          "the containing section's Profile Coordinate System. Any rotation applied to the containing section's profile " +
          "will be applied to its sub-components. Sub-components can also have an additional rotation for their profiles.", GH_ParamAccess.item);

      // make all but first input optional
      for (int i = 1; i < pManager.ParamCount; i++)
        pManager[i].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("SubComponent", "Sub", "AdSet Subcomponent", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      AdSecSection section = GetInput.AdSecSection(this, DA, 0);
      if (section == null) { return; }
      IPoint offset = GetInput.IPoint(this, DA, 1, true);
      if (offset == null)
      {
        offset = IPoint.Create(Length.Zero, Length.Zero);
      }
      ISubComponent subComponent = ISubComponent.Create(section.Section, offset);
      AdSecSubComponentGoo subGoo = new AdSecSubComponentGoo(subComponent, section.LocalPlane, section.DesignCode, section.codeName, section.materialName);
      DA.SetData(0, subGoo);
    }
  }
}
