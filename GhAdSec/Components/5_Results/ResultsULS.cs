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
    public class ResultsULS : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("1337cc01-0b76-4f58-b24e-81e32ae24f92");
        public ResultsULS()
          : base("Strength Result", "ULS", "Performs strength checks (ULS), for a given Load or Deformation.",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat5())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.Analyse;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Res", "AdSec Results to perform strenght check on.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load", "Ld", "AdSec Load (Load or Deformation) for which the strength results are to be calculated.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            IQuantity strain = new Oasys.Units.Strain(0, GhAdSec.DocumentUnits.StrainUnit);
            string strainUnitAbbreviation = string.Concat(strain.ToString().Where(char.IsLetter));
            IQuantity curvature = new Oasys.Units.Curvature(0, GhAdSec.DocumentUnits.CurvatureUnit);
            string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
            IQuantity moment = new Oasys.Units.Moment(0, GhAdSec.DocumentUnits.MomentUnit);
            string momentUnitAbbreviation = string.Concat(moment.ToString().Where(char.IsLetter));

            pManager.AddGenericParameter("Load", "Ld", "The section load under the applied action." + 
                System.Environment.NewLine + "If the applied deformation is outside the capacity range of the section, the returned load will be zero.", GH_ParamAccess.item);

            pManager.AddNumberParameter("LoadUtil", "Ul", "The strength load utilisation is the ratio of the applied load to the load in the same direction that would cause the " +
                "section to reach its capacity. Utilisation > 1 means the applied load exceeds the section capacity." +
                System.Environment.NewLine + "If the applied load is outside the capacity range of the section, the utilisation will be greater than 1. Whereas, if the applied " +
                "deformation exceeds the capacity, the load utilisation will be zero.", GH_ParamAccess.item);

            pManager.AddVectorParameter("Deformation", "Def", "The section deformation under the applied action. The output is a vector representing:"
                + System.Environment.NewLine + "X: Strain [" + strainUnitAbbreviation + "],"
                + System.Environment.NewLine + "Y: Curvature around zz (so in local y-direction) [" + curvatureUnitAbbreviation + "],"
                + System.Environment.NewLine + "Z: Curvature around yy (so in local z-direction) [" + curvatureUnitAbbreviation + "]", GH_ParamAccess.item);

            pManager.AddNumberParameter("DeformationUtil", "Ud", "The strength deformation utilisation is the ratio of the applied deformation to the deformation in the same direction" +
                " that would cause the section to reach its capacity. Utilisation > 1 means capacity has been exceeded." +
                System.Environment.NewLine + "Capacity has been exceeded when the utilisation is greater than 1. If the applied load is outside the capacity range of the section, the " +
                "deformation utilisation will be the maximum double value.", GH_ParamAccess.item);

            pManager.AddIntervalParameter("Moment Ranges", "MRs", "The range of moments (in the direction of the applied moment, assuming constant axial force) that are within the " +
                "section's capacity. Moment values are in [" + momentUnitAbbreviation + "]", GH_ParamAccess.list);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get solution input
            AdSecSolutionGoo solution = GetInput.Solution(this, DA, 0);

            IStrengthResult uls = null;

            // get load - can be either load or deformation
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(1, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecLoadGoo)
                {
                    AdSecLoadGoo load = (AdSecLoadGoo)gh_typ.Value;
                    uls = solution.Value.Strength.Check(load.Value);
                }
                else if (gh_typ.Value is AdSecDeformationGoo)
                {
                    AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typ.Value;
                    uls = solution.Value.Strength.Check(def.Value);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + Params.Input[1].Name + " input (index " + 1 + ") to an AdSec Load");
                    return;
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + Params.Input[1].Name + " input, index " + 1 + " - Input required");
                return;
            }

            DA.SetData(0, new AdSecLoadGoo(uls.Load, solution.LocalPlane));
            double util = uls.LoadUtilisation.As(UnitsNet.Units.RatioUnit.DecimalFraction);
            DA.SetData(1, util);
            if (util > 1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Utilisation is above 1!");

            DA.SetData(2, new Vector3d(
                uls.Deformation.X.As(GhAdSec.DocumentUnits.StrainUnit),
                uls.Deformation.YY.As(GhAdSec.DocumentUnits.CurvatureUnit),
                uls.Deformation.ZZ.As(GhAdSec.DocumentUnits.CurvatureUnit)));
            double defUtil = uls.DeformationUtilisation.As(UnitsNet.Units.RatioUnit.DecimalFraction);
            DA.SetData(3, defUtil);
            if (defUtil > 1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Deformation utilisation is above 1!");

            List<GH_Interval> momentRanges = new List<GH_Interval>();
            foreach (IMomentRange mrng in uls.MomentRanges)
            {
                Rhino.Geometry.Interval interval = new Interval(
                    mrng.Min.As(GhAdSec.DocumentUnits.MomentUnit),
                    mrng.Max.As(GhAdSec.DocumentUnits.MomentUnit));
                momentRanges.Add(new GH_Interval(interval));
            }
            DA.SetDataList(4, momentRanges);
        }
    }
}
