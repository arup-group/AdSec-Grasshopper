using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Display;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec;
using UnitsNet;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Preloads;

namespace AdSecGH.Parameters
{
    public class AdSecRebarGroup
    {
        public IGroup Group
        {
            get { return m_group; }
            set { m_group = value; }
        }
        public ICover Cover
        {
            get { return m_cover; }
            set { m_cover = value; }
        }

        #region fields
        private ICover m_cover;
        private IGroup m_group;
        #endregion

        #region constructors
        public AdSecRebarGroup()
        {
        }
        
        public AdSecRebarGroup(IGroup group)
        {
            m_group = group;
        }
        
        public AdSecRebarGroup Duplicate()
        {
            if (this == null) { return null; }
            AdSecRebarGroup dup = (AdSecRebarGroup)this.MemberwiseClone();
            return dup;
        }
        #endregion

        #region properties
        public bool IsValid
        {
            get
            {
                if (this.Group == null) { return false; }
                return true;
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return Group.ToString();
        }

        #endregion
    }

    public class AdSecRebarGroupGoo : GH_Goo<AdSecRebarGroup>
    {
        public AdSecRebarGroupGoo()
        {
            this.Value = null;
        }
        public AdSecRebarGroupGoo(IGroup group)
        {
            this.Value = new AdSecRebarGroup(group);
        }
        public AdSecRebarGroupGoo(AdSecRebarGroup goo)
        {
            if (goo == null)
                goo = new AdSecRebarGroup();
            this.Value = goo; // goo.Duplicate(); 
        }
        internal ICover Cover
        {
            get { return Value.Cover; }
            set { Value.Cover = value; }
        }
        public override bool IsValid => true;
        public override string TypeName => "Rebar Group";
        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            AdSecRebarGroupGoo dup = new AdSecRebarGroupGoo(this.Value);
            if (Value.Cover != null)
                dup.Value.Cover = ICover.Create(Value.Cover.UniformCover);
            return dup;
        }
        //public AdSecRebarGroupGoo Duplicate()
        //{
        //    AdSecRebarGroupGoo dup;
        //    ICover cover;
        //    try
        //    {
        //        // try longitudinal group first
        //        ILongitudinalGroup longitudinal = (ILongitudinalGroup)Value;

        //        try
        //        {
        //            ITemplateGroup temp = (ITemplateGroup)Value;

        //            dup = new AdSecRebarGroupGoo(new ITemplateGroup.)
        //        }
        //        catch (Exception)
        //        {
        //            try
        //            {
        //                IPerimeterGroup perimeter = (IPerimeterGroup)Value;
        //                m_ToString = "Perimeter Group, " + m_cover.UniformCover.ToUnit(DocumentUnits.LengthUnit) + " cover";
        //            }
        //            catch (Exception)
        //            {
        //                try
        //                {
        //                    IArcGroup arc = (IArcGroup)Value;
        //                    m_ToString = "Arc Type Layout";
        //                }
        //                catch (Exception)
        //                {
        //                    try
        //                    {
        //                        ICircleGroup cir = (ICircleGroup)Value;
        //                        m_ToString = "Circle Type Layout";
        //                    }
        //                    catch (Exception)
        //                    {
        //                        try
        //                        {
        //                            ILineGroup lin = (ILineGroup)Value;
        //                            m_ToString = "Line Type Layout";
        //                        }
        //                        catch (Exception)
        //                        {
        //                            try
        //                            {
        //                                ISingleBars sin = (ISingleBars)Value;
        //                                m_ToString = "SingleBars Type Layout";
        //                            }
        //                            catch (Exception)
        //                            {

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        // get any preload
        //        if (longitudinal.Preload != null)
        //        {
        //            try
        //            {
        //                IPreForce force = (IPreForce)longitudinal.Preload;

        //            }
        //            catch (Exception)
        //            {
        //                try
        //                {
        //                    IPreStress stress = (IPreStress)longitudinal.Preload;

        //                }
        //                catch (Exception)
        //                {
        //                    IPreStrain strain = (IPreStrain)longitudinal.Preload;

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        try
        //        {
        //            ILinkGroup link = (ILinkGroup)Value;
        //            m_ToString = "Link, " + m_cover.UniformCover.ToUnit(DocumentUnits.LengthUnit) + " cover";
        //        }
        //        catch (Exception)
        //        {

        //        }

        //    }
        //}
        public override string ToString()
        {
            string m_ToString = "";
            string m_preLoad = "";
            try
            {
                // try longitudinal group first
                ILongitudinalGroup longitudinal = (ILongitudinalGroup)Value.Group;
                
                // get any preload
                if (longitudinal.Preload != null)
                {
                    try
                    {
                        IPreForce force = (IPreForce)longitudinal.Preload;
                        if (force.Force.Value != 0)
                        {
                            IQuantity quantityForce = new Force(0, Units.ForceUnit);
                            string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
                            m_preLoad = ", " + Math.Round(force.Force.As(Units.ForceUnit), 4) + unitforceAbbreviation + " prestress";
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            IPreStress stress = (IPreStress)longitudinal.Preload;
                            if (stress.Stress.Value != 0)
                            {
                                IQuantity quantityStress = new Pressure(0, Units.StressUnit);
                                string unitstressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
                                m_preLoad = ", " + Math.Round(stress.Stress.As(Units.StressUnit), 4) + unitstressAbbreviation + " prestress";
                            }
                        }
                        catch (Exception)
                        {
                            IPreStrain strain = (IPreStrain)longitudinal.Preload;
                            if (strain.Strain.Value != 0)
                            {
                                string unitstrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(Units.StrainUnit);
                                m_preLoad = ", " + Math.Round(strain.Strain.As(Units.StrainUnit), 4) + unitstrainAbbreviation + " prestress";
                            }
                        }
                    }
                }

                try
                {
                    ITemplateGroup temp = (ITemplateGroup)Value.Group;
                    m_ToString = "Template Group, " + Value.Cover.UniformCover.ToUnit(Units.LengthUnit) + " cover";
                }
                catch (Exception)
                {
                    try
                    {
                        IPerimeterGroup perimeter = (IPerimeterGroup)Value.Group;
                        m_ToString = "Perimeter Group, " + Value.Cover.UniformCover.ToUnit(Units.LengthUnit) + " cover";
                    }
                    catch (Exception)
                    {
                        try
                        {
                            IArcGroup arc = (IArcGroup)Value.Group;
                            m_ToString = "Arc Type Layout";
                        }
                        catch (Exception)
                        {
                            try
                            {
                                ICircleGroup cir = (ICircleGroup)Value.Group;
                                m_ToString = "Circle Type Layout";
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    ILineGroup lin = (ILineGroup)Value.Group;
                                    m_ToString = "Line Type Layout";
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        ISingleBars sin = (ISingleBars)Value.Group;
                                        m_ToString = "SingleBars Type Layout";
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    ILinkGroup link = (ILinkGroup)Value.Group;
                    m_ToString = "Link, " + Value.Cover.UniformCover.ToUnit(Units.LengthUnit) + " cover";
                }
                catch (Exception)
                {

                }
                
            }

            return "AdSec " + TypeName + " {" + m_ToString + m_preLoad + "}";
        }

        #region casting methods
        public override bool CastTo<Q>(ref Q target)
        {
            // This function is called when Grasshopper needs to convert this 
            // instance of GsaBool6 into some other type Q.            


            if (typeof(Q).IsAssignableFrom(typeof(AdSecRebarGroup)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value.Duplicate();
                return true;
            }

            target = default;
            return false;
        }
        public override bool CastFrom(object source)
        {
            // This function is called when Grasshopper needs to convert other data 
            // into this parameter.

            if (source == null) { return false; }

            //Cast from own type
            if (typeof(AdSecRebarGroup).IsAssignableFrom(source.GetType()))
            {
                Value = (AdSecRebarGroup)source;
                return true;
            }

            return false;
        }
        #endregion
    }


    /// <summary>
    /// This class provides a Parameter interface for the Data_GsaBool6 type.
    /// </summary>
    public class AdSecRebarGroupParameter : GH_PersistentParam<AdSecRebarGroupGoo>
    {
        public AdSecRebarGroupParameter()
          : base(new GH_InstanceDescription("RebarGroup", "RbG", "AdSec RebarGroup Parameter", Components.Ribbon.CategoryName.Name(), Components.Ribbon.SubCategoryName.Cat9()))
        {
        }

        public override Guid ComponentGuid => new Guid("6d666276-61f6-47ce-81bc-9fabdd39edc2");

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override Bitmap Icon => Properties.Resources.RebarGroupParam;

        protected override GH_GetterResult Prompt_Plural(ref List<AdSecRebarGroupGoo> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref AdSecRebarGroupGoo value)
        {
            return GH_GetterResult.cancel;
        }
        protected override ToolStripMenuItem Menu_CustomSingleValueItem()
        {
            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }
        protected override ToolStripMenuItem Menu_CustomMultiValueItem()
        {
            ToolStripMenuItem item = new ToolStripMenuItem
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
