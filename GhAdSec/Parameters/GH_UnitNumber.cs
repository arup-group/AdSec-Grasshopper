﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino;
using Grasshopper.Documentation;
using Rhino.Collections;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using UnitsNet;
using Oasys.Units;

namespace UnitsNet.GH
{
    /// <summary>
    /// Goo wrapper class, makes sure this can be used in Grasshopper.
    /// </summary>
    public class GH_UnitNumber : GH_Goo<UnitsNet.IQuantity>
    {
        #region constructors
        public GH_UnitNumber(UnitsNet.IQuantity quantity)
        : base(quantity)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_UnitNumber(this.Value);
        }
        #endregion

        #region properties
        public override bool IsValid
        {
            get
            {
                if (Value == null) { return false; }
                return true;
            }
        }
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) { return string.Empty; }
                return Value.ToString(); 
            }
        }
        public override string ToString()
        {
            if (Value == null)
                return "Null";
            else
            {
                return Value.ToString();
            }
        }
        public override string TypeName
        {
            get { return ("Unit Number"); }
        }
        public override string TypeDescription
        {
            get { return ("A value with a unit measure"); }
        }
        #endregion

        #region casting methods
        public override bool CastTo<Q>(ref Q target)
        {
            // This function is called when Grasshopper needs to convert this 
            // instance of UnitNumber into some other type Q.            

            if (typeof(Q).IsAssignableFrom(typeof(GH_UnitNumber)))
            {
                target = (Q)(object)new GH_UnitNumber(Value);
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(GH_Number)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)new GH_Number(Value.Value);
                return true;
            }

            target = default;
            return false;
        }
        public override bool CastFrom(object source)
        {
            // This function is called when Grasshopper needs to convert other data 
            // into this parameter.


            //if (source == null) { return false; }

            ////Cast from own type
            //if (typeof(GhUnitNumber).IsAssignableFrom(source.GetType()))
            //{
            //    Value = (GhUnitNumber)source;
            //    return true;
            //}
            
            return false;
        }
        #endregion
    }

    /// <summary>
    /// This class provides a Parameter interface for the Data_GsaBool6 type.
    /// </summary>
    public class UnitNumber : GH_PersistentParam<GH_UnitNumber>
    {
        public UnitNumber()
          : base(new GH_InstanceDescription("UnitNumber", "UNum", "Quantity = number + unit", GhAdSec.Components.Ribbon.CategoryName.Name(), GhAdSec.Components.Ribbon.SubCategoryName.Cat9()))
        {
        }

        public override Guid ComponentGuid => new Guid("7368cb74-1c8d-411f-9455-1134a6d9df44");

        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.UnitNumber;

        protected override GH_GetterResult Prompt_Plural(ref List<GH_UnitNumber> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref GH_UnitNumber value)
        {
            return GH_GetterResult.cancel;
        }
        protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomSingleValueItem()
        {
            System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }
        protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomMultiValueItem()
        {
            System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }

        #region preview methods

        public bool Hidden
        {
            get { return true; }
            //set { m_hidden = value; }
        }
        public bool IsPreviewCapable
        {
            get { return false; }
        }
        #endregion
    }
}