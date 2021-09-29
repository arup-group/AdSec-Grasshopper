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
    public class AdSecRebarGroupGoo : GH_Goo<IGroup>
    {
        public AdSecRebarGroupGoo(IGroup group)
        : base(group)
        {
        }
        internal ICover Cover
        {
            get { return m_cover; }
            set { m_cover = value; }
        }
        ICover m_cover;
        public override bool IsValid => true;
        public override string TypeName => "Rebar Group";
        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            AdSecRebarGroupGoo dup = new AdSecRebarGroupGoo(this.Value);
            if (m_cover != null)
                dup.m_cover = ICover.Create(m_cover.UniformCover);
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
                ILongitudinalGroup longitudinal = (ILongitudinalGroup)Value;
                
                // get any preload
                if (longitudinal.Preload != null)
                {
                    try
                    {
                        IPreForce force = (IPreForce)longitudinal.Preload;
                        if (force.Force.Value != 0)
                        {
                            IQuantity quantityForce = new UnitsNet.Force(0, DocumentUnits.ForceUnit);
                            string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
                            m_preLoad = ", " + Math.Round(force.Force.As(DocumentUnits.ForceUnit), 4) + unitforceAbbreviation + " prestress";
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            IPreStress stress = (IPreStress)longitudinal.Preload;
                            if (stress.Stress.Value != 0)
                            {
                                IQuantity quantityStress = new UnitsNet.Pressure(0, DocumentUnits.StressUnit);
                                string unitstressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
                                m_preLoad = ", " + Math.Round(stress.Stress.As(DocumentUnits.StressUnit), 4) + unitstressAbbreviation + " prestress";
                            }
                        }
                        catch (Exception)
                        {
                            IPreStrain strain = (IPreStrain)longitudinal.Preload;
                            if (strain.Strain.Value != 0)
                            {
                                string unitstrainAbbreviation = Oasys.Units.Strain.GetAbbreviation(DocumentUnits.StrainUnit);
                                m_preLoad = ", " + Math.Round(strain.Strain.As(DocumentUnits.StrainUnit), 4) + unitstrainAbbreviation + " prestress";
                            }
                        }
                    }
                    
                }

                try
                {
                    ITemplateGroup temp = (ITemplateGroup)Value;
                    m_ToString = "Template Group, " + m_cover.UniformCover.ToUnit(DocumentUnits.LengthUnit) + " cover";
                }
                catch (Exception)
                {
                    try
                    {
                        IPerimeterGroup perimeter = (IPerimeterGroup)Value;
                        m_ToString = "Perimeter Group, " + m_cover.UniformCover.ToUnit(DocumentUnits.LengthUnit) + " cover";
                    }
                    catch (Exception)
                    {
                        try
                        {
                            IArcGroup arc = (IArcGroup)Value;
                            m_ToString = "Arc Type Layout";
                        }
                        catch (Exception)
                        {
                            try
                            {
                                ICircleGroup cir = (ICircleGroup)Value;
                                m_ToString = "Circle Type Layout";
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    ILineGroup lin = (ILineGroup)Value;
                                    m_ToString = "Line Type Layout";
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        ISingleBars sin = (ISingleBars)Value;
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
                    ILinkGroup link = (ILinkGroup)Value;
                    m_ToString = "Link, " + m_cover.UniformCover.ToUnit(DocumentUnits.LengthUnit) + " cover";
                }
                catch (Exception)
                {

                }
                
            }

            return "AdSec " + TypeName + " {" + m_ToString + m_preLoad + "}";
        }
    }

    

    /// <summary>
    /// This class provides a Parameter interface for the Data_GsaBool6 type.
    /// </summary>
    public class AdSecRebarGroupParameter : GH_PersistentParam<AdSecRebarGroupGoo>
    {
        public AdSecRebarGroupParameter()
          : base(new GH_InstanceDescription("RebarGroup", "RbG", "AdSec RebarGroup Parameter", AdSecGH.Components.Ribbon.CategoryName.Name(), AdSecGH.Components.Ribbon.SubCategoryName.Cat9()))
        {
        }

        public override Guid ComponentGuid => new Guid("6d666276-61f6-47ce-81bc-9fabdd39edc2");

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        //protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.RebarLayout;

        protected override GH_GetterResult Prompt_Plural(ref List<AdSecRebarGroupGoo> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref AdSecRebarGroupGoo value)
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
