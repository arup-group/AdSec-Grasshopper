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
    public class ResultRebarStressStrain : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("bb9fe65b-76e1-466b-be50-3dd9c7a3283f");
        public ResultRebarStressStrain()
          : base("Rebar Result", "Str", "Calculate the stress / strain for all Rebars for a given Load or Deformation.",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat5())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.SLS;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform serviceability check on.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            IQuantity strain = new Oasys.Units.Strain(0, Units.StrainUnit);
            string strainUnitAbbreviation = string.Concat(strain.ToString().Where(char.IsLetter));
            IQuantity stress = new UnitsNet.Pressure(0, Units.StressUnit);
            string stressUnitAbbreviation = string.Concat(strain.ToString().Where(char.IsLetter));

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
            //solution.m_section.Section.ReinforcementGroups
            //IStrengthResult uls = null;

            //// get load - can be either load or deformation
            //GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            //if (DA.GetData(1, ref gh_typ))
            //{
            //    // try cast directly to quantity type
            //    if (gh_typ.Value is AdSecLoadGoo)
            //    {
            //        AdSecLoadGoo load = (AdSecLoadGoo)gh_typ.Value;
            //        uls = solution.Value.Strength.Check(load.Value);
            //    }
            //    else if (gh_typ.Value is AdSecDeformationGoo)
            //    {
            //        AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typ.Value;
            //        uls = solution.Value.Strength.Check(def.Value);
            //    }
            //    else
            //    {
            //        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[1].NickName + " to AdSec Load");
            //        return;
            //    }
            //}
            //else
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + Params.Input[1].NickName + " failed to collect data!");
            //    return;
            //}

            //Strain strain = uls.Deformation.StrainAt(GetInput.IPoint(this, DA, 2, false));
            //GH_UnitNumber outStrain = new GH_UnitNumber(new Oasys.Units.Strain(strain.As(Units.StrainUnit), Units.StrainUnit));
            //DA.SetData(0, outStrain);
        }
    }
}
