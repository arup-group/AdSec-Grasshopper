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

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create a new Material
    /// </summary>
    public class StressStrainPoint : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("69a789d4-c11b-4396-b237-a10efdd6d0c4");
        public StressStrainPoint()
          : base("Create StressStrainPt", "StressStrainPt", "Create a Stress Strain Point for AdSec Stress Strain Curve",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

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
                selecteditems.Add(pressureUnit.ToString());

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
                    pressureUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
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
            "Pressure Unit"
        });
        private bool first = true;

        private Oasys.Units.StrainUnit strainUnit = GhAdSec.DocumentUnits.StrainUnit;
        private UnitsNet.Units.PressureUnit pressureUnit = GhAdSec.DocumentUnits.PressureUnit;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Strain", "ε", "Value for strain (X-axis)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pressure", "σ", "Value for pressure (Y-axis)", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StressStrainPt", "SPt", "AdSec Stress Strain Point", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get inputs
            double x = 0; double y = 0;
            DA.GetData(0, ref x);
            DA.GetData(1, ref y);

            // create new point
            IStressStrainPoint pt = IStressStrainPoint.Create(new UnitsNet.Pressure(x, pressureUnit), new Oasys.Units.Strain(y, strainUnit));

            DA.SetData(0, pt);
        }
        
        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // to save the dropdownlist content, spacer list and selection list 
            // loop through the lists and save number of lists as well
            writer.SetInt32("dropdownCount", dropdownitems.Count);
            for (int i = 0; i < dropdownitems.Count; i++)
            {
                writer.SetInt32("dropdowncontentsCount" + i, dropdownitems[i].Count);
                for (int j = 0; j < dropdownitems[i].Count; j++)
                    writer.SetString("dropdowncontents" + i + j, dropdownitems[i][j]);
            }
            // spacer list
            writer.SetInt32("spacerCount", spacerDescriptions.Count);
            for (int i = 0; i < spacerDescriptions.Count; i++)
                writer.SetString("spacercontents" + i, spacerDescriptions[i]);
            // selection list
            writer.SetInt32("selectionCount", selecteditems.Count);
            for (int i = 0; i < selecteditems.Count; i++)
                writer.SetString("selectioncontents" + i, selecteditems[i]);
            
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // dropdown content list
            int dropdownCount = reader.GetInt32("dropdownCount");
            dropdownitems = new List<List<string>>();
            for (int i = 0; i < dropdownCount; i++)
            {
                int dropdowncontentsCount = reader.GetInt32("dropdowncontentsCount" + i);
                List<string> tempcontent = new List<string>();
                for (int j = 0; j < dropdowncontentsCount; j++)
                    tempcontent.Add(reader.GetString("dropdowncontents" + i + j));
                dropdownitems.Add(tempcontent);
            }
            // spacer list
            int dropdownspacerCount = reader.GetInt32("spacerCount");
            spacerDescriptions = new List<string>();
            for (int i = 0; i < dropdownspacerCount; i++)
                spacerDescriptions.Add(reader.GetString("spacercontents" + i));
            // selection list
            int selectionsCount = reader.GetInt32("selectionCount");
            selecteditems = new List<string>();
            for (int i = 0; i < selectionsCount; i++)
                selecteditems.Add(reader.GetString("selectioncontents" + i));

            strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[0]);
            pressureUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[1]);

            first = false;
            return base.Read(reader);
        }
        #endregion
    }
}