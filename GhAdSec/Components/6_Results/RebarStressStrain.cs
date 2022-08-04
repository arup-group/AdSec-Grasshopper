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
using UnitsNet.GH;

namespace AdSecGH.Components
{
    public class RebarStressStrain : GH_OasysComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("bb9fe65b-76e1-466b-be50-3dd9c7a3283f");
        public RebarStressStrain()
          : base("Rebar Stress/Strain", "RSS", "Calculate the Rebar Stress/Strains in the Section for a given Load or Deformation.",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat7())
        { this.Hidden = true; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainRebar;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            IQuantity length = new Length(0, Units.LengthUnit);
            string lengthUnitAbbreviation = string.Concat(length.ToString().Where(char.IsLetter));
            string strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(Units.StrainUnit);
            IQuantity stress = new Pressure(0, Units.StressUnit);
            string stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));

            pManager.AddGenericParameter("Position [" + lengthUnitAbbreviation + "]", "Vx", "Rebar position as 2D vertex in the section's local yz-plane ", GH_ParamAccess.list);
            
            pManager.AddGenericParameter("ULS Strain [" + strainUnitAbbreviation + "]", "εd", "ULS strain for each rebar position", GH_ParamAccess.list);
            pManager.AddGenericParameter("ULS Stress [" + stressUnitAbbreviation + "]", "σd", "ULS stress for each rebar position", GH_ParamAccess.list);
            pManager.AddGenericParameter("SLS Strain [" + strainUnitAbbreviation + "]", "εk", "SLS strain for each rebar position", GH_ParamAccess.list);
            pManager.AddGenericParameter("SLS Stress [" + stressUnitAbbreviation + "]", "σk", "SLS stress for each rebar position", GH_ParamAccess.list);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get solution input
            AdSecSolutionGoo solution = GetInput.Solution(this, DA, 0);

            IStrengthResult uls = null;
            IServiceabilityResult sls = null;

            // get load - can be either load or deformation
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(1, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecLoadGoo)
                {
                    AdSecLoadGoo load = (AdSecLoadGoo)gh_typ.Value;
                    uls = solution.Value.Strength.Check(load.Value);
                    sls = solution.Value.Serviceability.Check(load.Value);
                }
                else if (gh_typ.Value is AdSecDeformationGoo)
                {
                    AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typ.Value;
                    uls = solution.Value.Strength.Check(def.Value);
                    sls = solution.Value.Serviceability.Check(def.Value);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
                    return;
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
                return;
            }
            

            // create flattened section to extract rebars
            ISection flat = null;
            if (solution.m_section.DesignCode != null) //{ code = Oasys.AdSec.DesignCode.EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex; }
            {
                IAdSec adSec = IAdSec.Create(solution.m_section.DesignCode);
                flat = adSec.Flatten(solution.m_section.Section);
            }
            else
            {
                IPerimeterProfile prof = IPerimeterProfile.Create(solution.m_section.Section.Profile);
                flat = ISection.Create(prof, solution.m_section.Section.Material);
            }

            List<AdSecPointGoo> pointGoos = new List<AdSecPointGoo>();
            List<GH_UnitNumber> outStrainULS = new List<GH_UnitNumber>();
            List<GH_UnitNumber> outStressULS = new List<GH_UnitNumber>();
            List<GH_UnitNumber> outStrainSLS = new List<GH_UnitNumber>();
            List<GH_UnitNumber> outStressSLS = new List<GH_UnitNumber>();

            // loop through rebar groups in flattened section
            foreach (IGroup rebargrp in flat.ReinforcementGroups)
            {
                try // first try if not a link group type
                {
                    ISingleBars snglBrs = (ISingleBars)rebargrp;
                    foreach (IPoint pos in snglBrs.Positions)
                    {
                        // position
                        pointGoos.Add(new AdSecPointGoo(pos));

                        // ULS strain
                        Strain strainULS = uls.Deformation.StrainAt(pos);
                        outStrainULS.Add(new GH_UnitNumber(new Strain(strainULS.As(Units.StrainUnit), Units.StrainUnit)));
                        
                        // ULS stress in bar material from strain
                        Pressure stressULS = snglBrs.BarBundle.Material.Strength.StressAt(strainULS);
                        outStressULS.Add(new GH_UnitNumber(new Pressure(stressULS.As(Units.StressUnit), Units.StressUnit)));

                        // SLS strain
                        Strain strainSLS = sls.Deformation.StrainAt(pos);
                        outStrainSLS.Add(new GH_UnitNumber(new Strain(strainSLS.As(Units.StrainUnit), Units.StrainUnit)));

                        // SLS stress in bar material from strain
                        Pressure stressSLS = snglBrs.BarBundle.Material.Serviceability.StressAt(strainSLS);
                        outStressSLS.Add(new GH_UnitNumber(new Pressure(stressSLS.As(Units.StressUnit), Units.StressUnit)));

                    }
                }
                catch (Exception)
                {
                    // do nothing if rebar is link
                }
            }

            DA.SetDataList(0, pointGoos);
            DA.SetDataList(1, outStrainULS);
            DA.SetDataList(2, outStressULS);
            DA.SetDataList(3, outStrainSLS);
            DA.SetDataList(4, outStressSLS);
        }
    }
}
