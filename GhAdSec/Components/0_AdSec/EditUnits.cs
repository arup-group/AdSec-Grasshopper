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

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create a new Material
    /// </summary>
    public class EditUnits : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("f422b2ba-f171-4ba6-a4da-86a0f3fa9a16");
        public EditUnits()
          : base("Set AdSec GH Units", "Units", "Edit units for AdSec in Grasshopper instance",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat0())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.primary;

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

                // length
                UnitsNet.Units.LengthUnit rhUNit = GhAdSec.DocumentUnits.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
                string rhinounitstr = "Use Rhino unit: " + rhUNit.ToString();
                rhinounit = rhinounitstr;
                List<string> length = new List<string>();
                length.Add(rhinounitstr);
                length.AddRange(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                dropdownitems.Add(length);
                selecteditems.Add(length[0]);

                // strain
                dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                selecteditems.Add(GhAdSec.DocumentUnits.StrainUnit.ToString());

                // pressure
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
                selecteditems.Add(GhAdSec.DocumentUnits.PressureUnit.ToString());

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUICapsule(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }
        string rhinounit;
        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            bool redraw = false;
            // update rhino unit string
            UnitsNet.Units.LengthUnit rhUNit = GhAdSec.DocumentUnits.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
            string rhinounitstr = "Use Rhino unit: " + rhUNit.ToString();
            if (!rhinounit.Equals(rhinounitstr))
            {
                dropdownitems[0][0] = rhinounitstr;
                rhinounit = rhinounitstr;
                redraw = true;
            }

            switch (i)
            {
                case 0:
                    if (j == 0)
                        GhAdSec.DocumentUnits.LengthUnit = rhUNit;
                    else
                        GhAdSec.DocumentUnits.LengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[i]);
                    break;
                case 1:
                    GhAdSec.DocumentUnits.StrainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[i]);
                    break;
                case 2:
                    GhAdSec.DocumentUnits.PressureUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
                    break;
            }

            if (redraw)
            {
                this.ExpireSolution(true);
                this.ExpirePreview(true);
            }
                
            
        }
        #endregion

        #region Input and output
        // get list of material types defined in material parameter
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Length Unit",
            "Strain Unit",
            "Pressure Unit"
        });
        private bool first = true;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
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

            first = false;
            return base.Read(reader);
        }
        #endregion
    }
}