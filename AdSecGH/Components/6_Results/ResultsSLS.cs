using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using Oasys.Units;
using OasysGH.Components;
using Rhino.Geometry;
using UnitsNet;

namespace AdSecGH.Components
{
    public class ResultsSLS : GH_OasysComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("27ba3ec5-b94c-43ad-8623-087540413628");
        public ResultsSLS()
          : base("Serviceability Result", "SLS", "Performs serviceability analysis (SLS), for a given Load or Deformation.",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat7())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.SLS;
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
            string strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(Units.StrainUnit);
            IQuantity curvature = new Curvature(0, Units.CurvatureUnit);
            string curvatureUnitAbbreviation = string.Concat(curvature.ToString().Where(char.IsLetter));
            IQuantity axial = new AxialStiffness(0, Units.AxialStiffnessUnit);
            string axialUnitAbbreviation = string.Concat(axial.ToString().Where(char.IsLetter));
            IQuantity bending = new BendingStiffness(0, Units.BendingStiffnessUnit);
            string bendingUnitAbbreviation = string.Concat(bending.ToString().Where(char.IsLetter));
            IQuantity moment = new Moment(0, Units.MomentUnit);
            string momentUnitAbbreviation = string.Concat(moment.ToString().Where(char.IsLetter));

            pManager.AddGenericParameter("Load", "Ld", "The section load under the applied action." +
                Environment.NewLine + "If the applied deformation is outside the capacity range of the section, the returned load will be zero.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Cracks", "Cks", "Crack results are calculated at bar positions or section surfaces depending on the Design Code specifications." +
                Environment.NewLine + "If the applied action is outside the capacity range of the section, the returned list will be empty. See MaximumCrack output " +
                "for the crack result corresponding to the maximum crack width.", GH_ParamAccess.item);

            pManager.AddGenericParameter("MaximumCrack", "Crk", "The crack result from Cracks that corresponds to the maximum crack width." +
                Environment.NewLine + "If the applied action is outside the capacity range of the section, the returned maximum width crack result will be maximum " +
                "double value.", GH_ParamAccess.item);

            pManager.AddNumberParameter("CrackUtil", "Uc", "The ratio of the applied load (moment and axial) to the load (moment and axial) in the same direction that would " +
                "cause the section to crack. Ratio > 1 means section is cracked." +
                Environment.NewLine + "The section is cracked when the cracking utilisation ratio is greater than 1. If the applied load is outside the capacity range" +
                " of the section, the cracking utilisation will be maximum double value.", GH_ParamAccess.item);

            pManager.AddVectorParameter("Deformation", "Def", "The section deformation under the applied action. The output is a vector representing:"
                + Environment.NewLine + "X: Strain [" + strainUnitAbbreviation + "],"
                + Environment.NewLine + "Y: Curvature around zz (so in local y-direction) [" + curvatureUnitAbbreviation + "],"
                + Environment.NewLine + "Z: Curvature around yy (so in local z-direction) [" + curvatureUnitAbbreviation + "]", GH_ParamAccess.item);

            pManager.AddVectorParameter("SecantStiffness", "Es", "The secant stiffness under the applied action. The output is a vector representing:"
                + Environment.NewLine + "X: Axial stiffness [" + axialUnitAbbreviation + "],"
                + Environment.NewLine + "Y: The bending stiffness about the y-axis in the local coordinate system [" + bendingUnitAbbreviation + "],"
                + Environment.NewLine + "Z: The bending stiffness about the z-axis in the local coordinate system [" + bendingUnitAbbreviation + "]", GH_ParamAccess.item);

            pManager.AddIntervalParameter("Uncracked Moment Ranges", "MRs", "The range of moments (in the direction of the applied moment, assuming constant axial force) " +
                "over which the section remains uncracked. Moment values are in [" + momentUnitAbbreviation + "]", GH_ParamAccess.list);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get solution input
            AdSecSolutionGoo solution = GetInput.Solution(this, DA, 0);

            IServiceabilityResult sls = null;

            // get load - can be either load or deformation
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(1, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecLoadGoo)
                {
                    AdSecLoadGoo load = (AdSecLoadGoo)gh_typ.Value;
                    sls = solution.Value.Serviceability.Check(load.Value);
                }
                else if (gh_typ.Value is AdSecDeformationGoo)
                {
                    AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typ.Value;
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

            DA.SetData(0, new AdSecLoadGoo(sls.Load, solution.LocalPlane));

            List<AdSecCrackGoo> cracks = new List<AdSecCrackGoo>();
            foreach (ICrack crack in sls.Cracks)
            {
                cracks.Add(new AdSecCrackGoo(crack, solution.LocalPlane));
            }
            DA.SetDataList(1, cracks);
            
            if (sls.MaximumWidthCrack != null && sls.MaximumWidthCrack.Width.Meters < 1)
                DA.SetData(2, new AdSecCrackGoo(sls.MaximumWidthCrack, solution.LocalPlane));

            double util = sls.CrackingUtilisation.As(UnitsNet.Units.RatioUnit.DecimalFraction);
            DA.SetData(3, util);
            if (util > 1)
            {
                if (cracks.Count == 0)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The section is failing and the cracks are so large we can't even compute them!");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "The section is cracked");
            }

            DA.SetData(4, new Vector3d(
                sls.Deformation.X.As(Units.StrainUnit),
                sls.Deformation.YY.As(Units.CurvatureUnit),
                sls.Deformation.ZZ.As(Units.CurvatureUnit)));
            
            DA.SetData(5, new Vector3d(
                sls.SecantStiffness.X.As(Units.AxialStiffnessUnit),
                sls.SecantStiffness.YY.As(Units.BendingStiffnessUnit),
                sls.SecantStiffness.ZZ.As(Units.BendingStiffnessUnit)));

            List<GH_Interval> momentRanges = new List<GH_Interval>();
            foreach (IMomentRange mrng in sls.UncrackedMomentRanges)
            {
                Interval interval = new Interval(
                    mrng.Min.As(Units.MomentUnit),
                    mrng.Max.As(Units.MomentUnit));
                momentRanges.Add(new GH_Interval(interval));
            }
            DA.SetDataList(6, momentRanges);
        }
    }
}
