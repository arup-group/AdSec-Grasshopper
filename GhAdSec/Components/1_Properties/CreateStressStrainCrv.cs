using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Oasys.AdSec.Materials.StressStrainCurves;
using GhAdSec.Parameters;

namespace GhAdSec.Components
{
    public class CreateStressStrainCurve : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateStressStrainCurve()
            : base("Create StressStrainCrv", "StressStrainCrv", "Create a Stress Strain Curve for AdSec Material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = false; }
        public override Guid ComponentGuid => new Guid("b2ddf545-2a4c-45ac-ba1c-cb0f3da5b37f");
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.BeamLoad;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            
            if (first)
            {
                selecteditems = new List<string>();
                selecteditems.Add(_mode.ToString());
                
                dropdownitems = new List<List<string>>();
                dropdownitems.Add(Enum.GetNames(typeof(GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)).ToList());
                dropdownitems[0].RemoveAt(dropdownitems[0].Count - 1);
                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // set selected item
            selecteditems[i] = dropdownitems[i][j];
            // remove dropdown lists beyond first level
            while (dropdownitems.Count > 1)
                dropdownitems.RemoveAt(1);
            // toggle case
            if (i == 0)
            {
                switch (j)
                {
                    case 0:
                        Mode0Clicked();
                        break;
                    case 1:
                        Mode1Clicked();
                        break;
                    case 2:
                        // add strain dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                        selecteditems.Add(strainUnit.ToString());

                        // add pressure dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
                        selecteditems.Add(pressureUnit.ToString());

                        Mode2Clicked();
                        break;
                    case 3:
                        Mode3Clicked();
                        break;
                    case 4:
                        // add strain dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                        selecteditems.Add(strainUnit.ToString());

                        // add pressure dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
                        selecteditems.Add(pressureUnit.ToString());

                        Mode4Clicked();
                        break;
                    case 5:
                        Mode5Clicked();
                        break;
                    case 6:
                        // add strain dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                        selecteditems.Add(strainUnit.ToString());

                        Mode6Clicked();
                        break;
                    case 7:
                        Mode7Clicked();
                        break;
                    case 8:
                        // add strain dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                        selecteditems.Add(strainUnit.ToString());
                        Mode8Clicked();
                        break;
                    case 9:
                        // add strain dropdown
                        dropdownitems.Add(Enum.GetNames(typeof(Oasys.Units.StrainUnit)).ToList());
                        selecteditems.Add(strainUnit.ToString());
                        Mode9Clicked();
                        break;

                }
            }
            else
            {
                switch (i)
                {
                    case 1:
                        strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[i]);
                        break;
                    case 2:
                        pressureUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
                        break;
                }
            }
            
        }
        #endregion

        #region Input and output
        List<List<string>> dropdownitems;
        List<string> selecteditems;
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Curve Type",
            "Strain Unit",
            "Pressure Unit",
        });
        private Oasys.Units.StrainUnit strainUnit = GhAdSec.DocumentUnits.StrainUnit;
        private UnitsNet.Units.PressureUnit pressureUnit = GhAdSec.DocumentUnits.PressureUnit;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Failure", "SPu", "AdSec Stress Strain Point representing the Failure Point", GH_ParamAccess.item);
            _mode = GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Linear;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StressStrainCrv", "SCv", "AdSec Stress Strain Curve", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IStressStrainCurve crv = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            GH_ObjectWrapper gh_typ1 = new GH_ObjectWrapper();
            GH_ObjectWrapper gh_typ2 = new GH_ObjectWrapper();
            IStressStrainPoint pt = null;
            IStressStrainPoint pt1 = null;
            IStressStrainPoint pt2 = null;
            double val1 = 0;
            double val2 = 0;
            double val3 = 0;
            double val4 = 0;

            switch (_mode)
            {
                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Bilinear:
                    if (DA.GetData(0, ref gh_typ1))
                    {
                        if (gh_typ1.Value is IStressStrainPoint)
                        {
                            pt1 = (IStressStrainPoint)gh_typ1.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input 1 to a Stress Strain Point");
                            return;
                        }
                    }
                    if (DA.GetData(1, ref gh_typ2))
                    {
                        if (gh_typ2.Value is IStressStrainPoint)
                        {
                            pt2 = (IStressStrainPoint)gh_typ2.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input 2 to a Stress Strain Point");
                            return;
                        }
                    }
                    crv = IBilinearStressStrainCurve.Create(pt1, pt2);
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Explicit:
                    Oasys.Collections.IList<IStressStrainPoint> pts = Oasys.Collections.IList<IStressStrainPoint>.Create();
                    List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
                    if (DA.GetDataList(0, gh_typs))
                    {
                        for (int i = 0; i < gh_typs.Count; i++)
                        {
                            Curve polycurve = null;
                            if (gh_typs[i].Value is IStressStrainPoint)
                            {
                                pts.Add((IStressStrainPoint)gh_typs[i].Value);
                            }
                            else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both))
                            {
                                PolylineCurve curve = (PolylineCurve)polycurve;
                                pts = GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainPtsFromPolyline(curve);
                            }
                            else
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input index " + i + " to a Stress Strain Point");
                                return;
                            }
                        }
                    }
                    IExplicitStressStrainCurve exCrv = IExplicitStressStrainCurve.Create();
                    exCrv.Points = pts;
                    crv = exCrv;
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.FibModelCode:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }
                    DA.GetData(1, ref val1);
                    DA.GetData(2, ref val2);
                    crv = IFibModelCodeStressStrainCurve.Create(
                        new UnitsNet.Pressure(val1, pressureUnit),
                        pt,
                        new Oasys.Units.Strain(val2, strainUnit));
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Mander:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }
                    DA.GetData(1, ref val1);
                    DA.GetData(2, ref val2);

                    crv = IManderStressStrainCurve.Create(
                        new UnitsNet.Pressure(val1, pressureUnit),
                        pt,
                        new Oasys.Units.Strain(val2, strainUnit));
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Linear:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }
                    
                    if (_mode == GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Linear)
                        crv = ILinearStressStrainCurve.Create(pt);
                    break;
                
                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.ManderConfined:
                    DA.GetData(0, ref val1);
                    DA.GetData(1, ref val2);
                    DA.GetData(2, ref val3);
                    DA.GetData(3, ref val4);

                    crv = IManderConfinedStressStrainCurve.Create(
                        new UnitsNet.Pressure(val1, pressureUnit),
                        new UnitsNet.Pressure(val2, pressureUnit),
                        new UnitsNet.Pressure(val3, pressureUnit),
                        new Oasys.Units.Strain(val4, strainUnit));
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.ParabolaRectangle:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }
                    DA.GetData(1, ref val1);
                    crv = IParabolaRectangleStressStrainCurve.Create(pt, new Oasys.Units.Strain(val1, strainUnit));
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Park:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }

                    if (_mode == GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Linear)
                        crv = IParkStressStrainCurve.Create(pt);
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Popovics:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }
                    
                    DA.GetData(1, ref val1);
                    crv = IParabolaRectangleStressStrainCurve.Create(pt, new Oasys.Units.Strain(val1, strainUnit));
                    break;

                case GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Rectangular:
                    if (DA.GetData(0, ref gh_typ))
                    {
                        if (gh_typ.Value is IStressStrainPoint)
                        {
                            pt = (IStressStrainPoint)gh_typ.Value;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert SPt input to a Stress Strain Point");
                            return;
                        }
                    }

                    DA.GetData(1, ref val1);
                    crv = IRectangularStressStrainCurve.Create(pt, new Oasys.Units.Strain(val1, strainUnit));
                    break;
            }

            // create preview
            Tuple<Curve, List<Point3d>> tuple = GhAdSec.Parameters.AdSecStressStrainCurve.Create(crv, _mode, true);

            DA.SetData(0, new GhAdSec.Parameters.AdSecStressStrainCurve(tuple.Item1, crv, _mode, tuple.Item2));
        }

        #region menu override
        //internal enum GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType
        //{
        //    Bilinear,
        //    Explicit,
        //    FibModelCode,
        //    Linear,
        //    ManderConfined,
        //    Mander,
        //    ParabolaRectangle,
        //    Park,
        //    Popovics,
        //    Rectangular
        //}
        private bool first = true;
        private GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType _mode = GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType.Linear;

        private void Mode0Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)0)
                return;
            
            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)0;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            while (Params.Input.Count != 2)
                Params.RegisterInputParam(new Param_GenericObject());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode1Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)1)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)1;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode2Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)2)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)2;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());
            while (Params.Input.Count != 3)
                Params.RegisterInputParam(new Param_Number());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode3Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)3)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)3;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode4Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                return;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4;

            //remove input parameters
            while (Params.Input.Count > 0)
                Params.UnregisterInputParameter(Params.Input[0], true);
            while (Params.Input.Count != 4)
                Params.RegisterInputParam(new Param_Number());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode5Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)5)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)5;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());
            while (Params.Input.Count != 3)
                Params.RegisterInputParam(new Param_Number());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode6Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)6)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)6;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());
            while (Params.Input.Count != 2)
                Params.RegisterInputParam(new Param_Number());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode7Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)7)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)7;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode8Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)8)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)8;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());
            while (Params.Input.Count != 2)
                Params.RegisterInputParam(new Param_Number());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void Mode9Clicked()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)9)
                return;

            bool cleanAll = false;
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
                cleanAll = true;

            RecordUndoEvent("Changed dropdown");
            _mode = (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)9;

            //remove input parameters
            int i = cleanAll ? 0 : 1;
            while (Params.Input.Count > i)
                Params.UnregisterInputParameter(Params.Input[i], true);
            if (cleanAll)
                Params.RegisterInputParam(new Param_GenericObject());
            while (Params.Input.Count != 2)
                Params.RegisterInputParam(new Param_Number());

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

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

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return null;
        }
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        #endregion
        #region IGH_VariableParameterComponent null implementation
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)0)
            {
                Params.Input[0].Name = "Yield Point";
                Params.Input[0].NickName = "SPy";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Failure Point";
                Params.Input[1].NickName = "SPu";
                Params.Input[1].Description = "AdSec Stress Strain Point representing the Failure Point";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)1)
            {
                Params.Input[0].Name = "StressStrainPts";
                Params.Input[0].NickName = "SPs";
                Params.Input[0].Description = "AdSec Stress Strain Points representing the StressStrainCurve as a Polyline";
                Params.Input[0].Access = GH_ParamAccess.list;
                Params.Input[0].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)2)
            {
                Params.Input[0].Name = "Peak Point";
                Params.Input[0].NickName = "SPt";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the FIB model's Peak Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Initial Modus";
                Params.Input[1].NickName = "Ei";
                Params.Input[1].Description = "Initial Moduls from FIB model code";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;

                Params.Input[2].Name = "Failure Strain";
                Params.Input[2].NickName = "εu";
                Params.Input[2].Description = "Failure strain from FIB model code";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)3)
            {
                Params.Input[0].Name = "Failure Point";
                Params.Input[0].NickName = "SPu";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Failure Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)4)
            {
                Params.Input[0].Name = "Unconfined Strength";
                Params.Input[0].NickName = "σU";
                Params.Input[0].Description = "Unconfined strength for Mander Confined Model";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Confined Strength";
                Params.Input[1].NickName = "σC";
                Params.Input[1].Description = "Confined strength for Mander Confined Model";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;

                Params.Input[2].Name = "Initial Modus";
                Params.Input[2].NickName = "Ei";
                Params.Input[2].Description = "Initial Moduls for Mander Confined Model";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;

                Params.Input[3].Name = "Failure Strain";
                Params.Input[3].NickName = "εu";
                Params.Input[3].Description = "Failure strain for Mander Confined Model";
                Params.Input[3].Access = GH_ParamAccess.item;
                Params.Input[3].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)5)
            {
                Params.Input[0].Name = "Peak Point";
                Params.Input[0].NickName = "SPt";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Mander model's Peak Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Initial Modus";
                Params.Input[1].NickName = "Ei";
                Params.Input[1].Description = "Initial Moduls for Mander model";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;

                Params.Input[2].Name = "Failure Strain";
                Params.Input[2].NickName = "εu";
                Params.Input[2].Description = "Failure strain for Mander model";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)6)
            {
                Params.Input[0].Name = "Yield Point";
                Params.Input[0].NickName = "SPy";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Failure Strain";
                Params.Input[1].NickName = "εu";
                Params.Input[1].Description = "Failure strain from FIB model code";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)7)
            {
                Params.Input[0].Name = "Yield Point";
                Params.Input[0].NickName = "SPy";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)8)
            {
                Params.Input[0].Name = "Peak Point";
                Params.Input[0].NickName = "SPt";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Peak Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Failure Strain";
                Params.Input[1].NickName = "εu";
                Params.Input[1].Description = "Failure strain from Popovic model";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }

            if (_mode == (GhAdSec.Parameters.AdSecStressStrainCurve.StressStrainCurveType)9)
            {
                Params.Input[0].Name = "Yield Point";
                Params.Input[0].NickName = "SPy";
                Params.Input[0].Description = "AdSec Stress Strain Point representing the Yield Point";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Failure Strain";
                Params.Input[1].NickName = "εu";
                Params.Input[1].Description = "Failure strain";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }
        }
        #endregion
    }
}
