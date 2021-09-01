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
using GhAdSec.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GhAdSec.Components
{
    public class EditProfile : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("78f26bee-c72c-4d88-9b30-492190df2910");
        public EditProfile()
          : base("Edit Profile", "ProfileEdit", "Modify AdSec Profile",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = true; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.EditProfile;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                List<string> list = Enum.GetNames(typeof(UnitsNet.Units.AngleUnit)).ToList();
                dropdownitems = new List<List<string>>();
                dropdownitems.Add(list);

                IQuantity quantityAngle = new UnitsNet.Angle(0, angleUnit);
                angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));

                selecteditems = new List<string>();
                selecteditems.Add(angleUnit.ToString());

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }
        public void SetSelected(int i, int j)
        {
            // set selected item
            selecteditems[i] = dropdownitems[i][j];
            angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), selecteditems[i]);
        }
        #endregion


        #region Input and output
        List<List<string>> dropdownitems;
        List<string> selecteditems;
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Measure"
        });
        private UnitsNet.Units.AngleUnit angleUnit = UnitsNet.Units.AngleUnit.Radian;
        string angleAbbreviation;
        bool first = true;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Profile", "Pf", "AdSet Profile to Edit or get information from", GH_ParamAccess.item);
            pManager.AddGenericParameter("Rotation", "R", "[Optional] The angle at which the profile is rotated. Positive rotation is anti-clockwise around the x-axis in the local coordinate system.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("isReflectedY", "rY", "[Optional] Reflects the profile over the y-axis in the local coordinate system.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("isReflectedZ", "rZ", "[Optional] Reflects the profile over the z-axis in the local coordinate system.", GH_ParamAccess.item);
            
            // make all but first input optional
            for (int i = 1; i < pManager.ParamCount; i++)
                pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Profile", "Pf", "Modified AdSet Profile", GH_ParamAccess.item);
            pManager.AddGenericParameter("Rotation", "R", "The angle at which the profile is rotated. Positive rotation is anti-clockwise around the x-axis in the local coordinate system.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("isReflectedY", "rY", "Reflects the profile over the y-axis in the local coordinate system.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("isReflectedZ", "rZ", "Reflects the profile over the z-axis in the local coordinate system.", GH_ParamAccess.item);

        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // #### get material input and duplicate it ####
            IProfile editPrf = GetInput.Profile(this, DA, 0);

            if (editPrf != null)
            {
                // #### get the remaining inputs ####

                // 1 Rotation
                if (Params.Input[1].SourceCount > 0)
                {
                    editPrf.Rotation = GetInput.Angle(this, DA, 1, angleUnit);
                }

                // 2 ReflectionY
                bool refY = false;
                if (DA.GetData(2, ref refY))
                {
                    editPrf.IsReflectedY = refY;
                }

                // 3 Reflection3
                bool refZ = false;
                if (DA.GetData(3, ref refZ))
                {
                    editPrf.IsReflectedZ = refZ;
                }

                DA.SetData(0, new AdSecProfileGoo(editPrf));
                DA.SetData(1, editPrf.Rotation);
                DA.SetData(2, editPrf.IsReflectedY);
                DA.SetData(3, editPrf.IsReflectedZ);
            }
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
            angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), selecteditems[0]);
            first = false;

            return base.Read(reader);
        }
        #endregion
    }
}
