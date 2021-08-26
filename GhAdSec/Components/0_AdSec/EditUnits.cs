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
using Rhino;

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
                // add hook to get updates to rhino units
                Rhino.RhinoDoc.DocumentPropertiesChanged += DocumentPropertiesChanged;

                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // length
                UnitsNet.Units.LengthUnit rhUnit = GhAdSec.DocumentUnits.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
                
                List<string> length = Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList();
                for (int i = 0; i < length.Count; i++)
                {
                    if (length[i].Equals(rhUnit.ToString()))
                    {
                        string rh = "Use Rhino unit: " + length[i];
                        length.RemoveAt(i);
                        length.Insert(0, rh);
                        break;
                    }
                }

                dropdownitems.Add(length);
                if (GhAdSec.DocumentUnits.LengthUnit.Equals(rhUnit))
                {
                    selecteditems.Add(length[0]);
                }
                else
                {
                    selecteditems.Add(GhAdSec.DocumentUnits.LengthUnit.ToString());
                }

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
        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            UnitsNet.Units.LengthUnit rhUnit = GhAdSec.DocumentUnits.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);

            switch (i)
            {
                case 0:
                    if (j == 0)
                        GhAdSec.DocumentUnits.LengthUnit = rhUnit;
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

            List<string> length = Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList();
            for (int k = 0; k < length.Count; k++)
            {
                if (length[k].Equals(rhUnit.ToString()))
                {
                    string rh = "Use Rhino unit: " + length[k];
                    length.RemoveAt(k);
                    length.Insert(0, rh);
                    break;
                }
            }
            dropdownitems[0] = length;
        }

        UnitSystem cachedUnitSystem = Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem;
        
        private void DocumentPropertiesChanged(object s, Rhino.DocumentEventArgs e)
        {
            if (e.Document.ModelUnitSystem != cachedUnitSystem)
            {
                cachedUnitSystem = e.Document.ModelUnitSystem;
                if (selecteditems[0].StartsWith("Use Rhino unit: "))
                    GhAdSec.DocumentUnits.LengthUnit = GhAdSec.DocumentUnits.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
                first = true;
                this.ExpireSolution(true);
                ReDrawComponent();
            }
        }

        private void ReDrawComponent()
        {
            System.Drawing.PointF pivot = new System.Drawing.PointF(this.Attributes.Pivot.X, this.Attributes.Pivot.Y);
            System.Drawing.RectangleF bound = new System.Drawing.RectangleF(this.Attributes.Bounds.X, this.Attributes.Bounds.Y, this.Attributes.Bounds.Width, this.Attributes.Bounds.Height);
            this.CreateAttributes();
            this.Attributes.Pivot = pivot;
            this.Attributes.Bounds = bound;
            this.Attributes.ExpireLayout();
            this.Attributes.PerformLayout();
            this.OnDisplayExpired(true);
        }
        #endregion

        #region Input and output
        // get list of material types defined in material parameter
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        // list of selected items
        static List<string> selecteditems;
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

            if (selecteditems[0].StartsWith("Use Rhino unit: "))
                GhAdSec.DocumentUnits.LengthUnit = GhAdSec.DocumentUnits.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
            else
                GhAdSec.DocumentUnits.LengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[0]);
            GhAdSec.DocumentUnits.StrainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[1]);
            GhAdSec.DocumentUnits.PressureUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[2]);
            this.Attributes.ExpireLayout();
            this.Attributes.PerformLayout();
            this.OnDisplayExpired(true);
            first = false;
            return base.Read(reader);
        }
        #endregion
    }
}