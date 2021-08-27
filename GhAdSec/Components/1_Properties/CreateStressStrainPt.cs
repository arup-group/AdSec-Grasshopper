using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using GhAdSec.Parameters;
using System.Resources;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using UnitsNet.GH;

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create a new Stress Strain Point
    /// </summary>
    public class CreateStressStrainPoint : GH_Component
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

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.CreateMaterial;
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
                dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                selecteditems.Add(strainUnit.ToString());

                // pressure
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
                selecteditems.Add(stressUnit.ToString());

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
                    strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[i]);
                    break;
                case 1:
                    stressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
                    break;
            }

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

        private Oasys.Units.StrainUnit strainUnit = GhAdSec.DocumentUnits.StrainUnit;
        private UnitsNet.Units.PressureUnit stressUnit = GhAdSec.DocumentUnits.StressUnit;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Strain", "ε", "Value for strain (X-axis)", GH_ParamAccess.item);
            pManager.AddGenericParameter("Stress", "σ", "Value for stress (Y-axis)", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StressStrainPt", "SPt", "AdSec Stress Strain Point", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get inputs
            UnitsNet.Pressure stress = new UnitsNet.Pressure();
            Oasys.Units.Strain strain = new Oasys.Units.Strain();

            // 0 Strain input
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(0, ref gh_typ))
            {
                GH_UnitNumber inStrain;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inStrain = (GH_UnitNumber)gh_typ.Value;
                    strain = (Oasys.Units.Strain)inStrain.Value.ToUnit(strainUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inStrain = new GH_UnitNumber(new Oasys.Units.Strain(val, strainUnit));
                    strain = (Oasys.Units.Strain)inStrain.Value;
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert E input");
                    return;
                }
            }

            // 1 Stress input
            gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(1, ref gh_typ))
            {
                GH_UnitNumber inStress;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inStress = (GH_UnitNumber)gh_typ.Value;
                    stress = (UnitsNet.Pressure)inStress.Value.ToUnit(stressUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inStress = new GH_UnitNumber(new UnitsNet.Pressure(val, stressUnit));
                    stress = (UnitsNet.Pressure)inStress.Value;
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert E input");
                    return;
                }
            }

            // create new point
            AdSecStressStrainPointGoo pt = new AdSecStressStrainPointGoo(stress, strain);

            DA.SetData(0, pt);
        }

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            GhAdSec.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

            strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[0]);
            stressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[1]);

            first = false;
            return base.Read(reader);
        }
        #endregion
    }
}