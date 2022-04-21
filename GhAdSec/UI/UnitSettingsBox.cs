using Grasshopper.Kernel;
using Oasys.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnitsNet.Units;

namespace AdSecGH.UI
{
    partial class UnitSettingsBox : Form
    {
        public UnitSettingsBox()
        {
            InitializeComponent();
            this.Text = "Default AdSec Units";
            this.labelDescription.Text = "Settings will apply to new components and display";

            this.labelLength.Text = "Length";
            lengthdropdown.Insert(0, "Use Rhino unit: " + Units.GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem).ToString());
            this.comboBoxLength.DataSource = lengthdropdown;
            this.comboBoxLength.DropDownStyle = ComboBoxStyle.DropDownList;
            if (!Units.useRhinoLengthUnit)
                this.comboBoxLength.SelectedIndex = lengthdropdown.IndexOf(Units.LengthUnit.ToString());

            this.labelForce.Text = "Force";
            this.comboBoxForce.DataSource = Units.FilteredForceUnits;
            this.comboBoxForce.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxForce.SelectedIndex = Units.FilteredForceUnits.IndexOf(Units.ForceUnit.ToString());

            this.labelMoment.Text = "Moment";
            this.comboBoxMoment.DataSource = Units.FilteredMomentUnits;
            this.comboBoxMoment.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxMoment.SelectedIndex = Units.FilteredMomentUnits.IndexOf(Units.MomentUnit.ToString());

            this.labelStress.Text = "Stress";
            this.comboBoxStress.DataSource = Units.FilteredStressUnits;
            this.comboBoxStress.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxStress.SelectedIndex = Units.FilteredStressUnits.IndexOf(Units.StressUnit.ToString());

            this.labelStrain.Text = "Strain";
            this.comboBoxStrain.DataSource = Units.FilteredStrainUnits;
            this.comboBoxStrain.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxStrain.SelectedIndex = Units.FilteredStrainUnits.IndexOf(Units.StrainUnit.ToString());

            this.labelAxialStiffness.Text = "Axial Stiffness";
            this.comboBoxAxialStiffness.DataSource = Units.FilteredAxialStiffnessUnits;
            this.comboBoxAxialStiffness.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxAxialStiffness.SelectedIndex = Units.FilteredAxialStiffnessUnits.IndexOf(Units.AxialStiffnessUnit.ToString());

            this.labelBendingStiffness.Text = "Bending Stiffness";
            this.comboBoxBendingStiffness.DataSource = Units.FilteredBendingStiffnessUnits;
            this.comboBoxBendingStiffness.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxBendingStiffness.SelectedIndex = Units.FilteredBendingStiffnessUnits.IndexOf(Units.BendingStiffnessUnit.ToString());
            
            this.labelCurvature.Text = "Curvature";
            this.comboBoxCurvature.DataSource = Units.FilteredCurvatureUnits;
            this.comboBoxCurvature.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxCurvature.SelectedIndex = Units.FilteredCurvatureUnits.IndexOf(Units.CurvatureUnit.ToString());
        }

        #region Temporary units
        List<string> lengthdropdown = Units.FilteredLengthUnits.ToList();
        string length = Units.LengthUnit.ToString();
        bool useRhinoUnits = Units.useRhinoLengthUnit;
        string force = Units.ForceUnit.ToString();
        string moment = Units.MomentUnit.ToString();
        string stress = Units.StressUnit.ToString();
        string strain = Units.StrainUnit.ToString();
        string axialstiffness = Units.AxialStiffnessUnit.ToString();
        string curvature = Units.CurvatureUnit.ToString();
        string bendingstiffness = Units.BendingStiffnessUnit.ToString();
        internal void SetUnits()
        {
            if (useRhinoUnits)
            {
                Units.useRhinoLengthUnit = true;
                Units.LengthUnit = Units.GetRhinoLengthUnit();
            }
            else
            {
                Units.useRhinoLengthUnit = false;
                Units.LengthUnit = (LengthUnit)Enum.Parse(typeof(LengthUnit), length);
            }

            Units.ForceUnit = (ForceUnit)Enum.Parse(typeof(ForceUnit), force);
            Units.MomentUnit = (MomentUnit)Enum.Parse(typeof(MomentUnit), moment);
            Units.StressUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), stress);
            Units.StrainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), strain);
            Units.AxialStiffnessUnit = (AxialStiffnessUnit)Enum.Parse(typeof(AxialStiffnessUnit), axialstiffness);
            Units.CurvatureUnit = (CurvatureUnit)Enum.Parse(typeof(CurvatureUnit), curvature);
            Units.BendingStiffnessUnit = (BendingStiffnessUnit)Enum.Parse(typeof(BendingStiffnessUnit), bendingstiffness);

            Units.SaveSettings();
        }
        #endregion


        private void UnitSettingsBox_Load(object sender, EventArgs e)
        {
        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SetUnits();
            this.Close();
            // force recalculation of all open grasshopper documents
            GH_DocumentServer documents = Grasshopper.Instances.DocumentServer;
            foreach (GH_Document doc in documents)
            {
                doc.NewSolution(true);
            }
        }

        private void comboBoxLength_SelectedIndexChanged(object sender, EventArgs e)
        {
            length = this.comboBoxLength.SelectedItem.ToString();
            if (this.comboBoxLength.SelectedIndex == 0)
                useRhinoUnits = true;
            else
                useRhinoUnits = false;
        }
        private void comboBoxForce_SelectedIndexChanged(object sender, EventArgs e)
        {
            force = this.comboBoxForce.SelectedItem.ToString();
        }

        private void comboBoxMoment_SelectedIndexChanged(object sender, EventArgs e)
        {
            moment = this.comboBoxMoment.SelectedItem.ToString();
        }

        private void comboBoxStress_SelectedIndexChanged(object sender, EventArgs e)
        {
            stress = this.comboBoxStress.SelectedItem.ToString();
        }

        private void comboBoxStrain_SelectedIndexChanged(object sender, EventArgs e)
        {
            strain = this.comboBoxStrain.SelectedItem.ToString();
        }

        private void comboBoxAxialStiffness_SelectedIndexChanged(object sender, EventArgs e)
        {
            axialstiffness = this.comboBoxAxialStiffness.SelectedItem.ToString();
        }

        private void comboBoxBendingStiffness_SelectedIndexChanged(object sender, EventArgs e)
        {
            bendingstiffness = this.comboBoxBendingStiffness.SelectedItem.ToString();
        }

        private void comboBoxCurvature_SelectedIndexChanged(object sender, EventArgs e)
        {
            curvature = this.comboBoxCurvature.SelectedItem.ToString();
        }

        private void updateSelections()
        {
            if (!useRhinoUnits)
                this.comboBoxLength.SelectedIndex = lengthdropdown.IndexOf(length);
            else
                this.comboBoxLength.SelectedIndex = 0;
            this.comboBoxForce.SelectedIndex = Units.FilteredForceUnits.IndexOf(force);
            this.comboBoxMoment.SelectedIndex = Units.FilteredMomentUnits.IndexOf(moment);
            this.comboBoxStress.SelectedIndex = Units.FilteredStressUnits.IndexOf(stress);
            this.comboBoxStrain.SelectedIndex = Units.FilteredStrainUnits.IndexOf(strain);
            this.comboBoxAxialStiffness.SelectedIndex = Units.FilteredAxialStiffnessUnits.IndexOf(axialstiffness);
            this.comboBoxBendingStiffness.SelectedIndex = Units.FilteredBendingStiffnessUnits.IndexOf(bendingstiffness);
            this.comboBoxCurvature.SelectedIndex = Units.FilteredCurvatureUnits.IndexOf(curvature);
        }
        private void buttonSI_Click(object sender, EventArgs e)
        {
            useRhinoUnits = false;
            length = LengthUnit.Meter.ToString();
            force = ForceUnit.Newton.ToString();
            moment = MomentUnit.NewtonMeter.ToString();
            stress = PressureUnit.Pascal.ToString();
            strain = StrainUnit.Ratio.ToString();
            axialstiffness = AxialStiffnessUnit.Newton.ToString();
            curvature = CurvatureUnit.PerMeter.ToString();
            bendingstiffness = BendingStiffnessUnit.NewtonSquareMeter.ToString();
            updateSelections();
        }

        private void buttonkNm_Click(object sender, EventArgs e)
        {
            useRhinoUnits = false;
            length = LengthUnit.Meter.ToString();
            force = ForceUnit.Kilonewton.ToString();
            moment = MomentUnit.KilonewtonMeter.ToString();
            stress = PressureUnit.NewtonPerSquareMillimeter.ToString();
            strain = StrainUnit.MilliStrain.ToString();
            axialstiffness = AxialStiffnessUnit.Kilonewton.ToString();
            curvature = CurvatureUnit.PerMeter.ToString();
            bendingstiffness = BendingStiffnessUnit.NewtonSquareMillimeter.ToString();
            updateSelections();
        }

        private void buttonkipFt_Click(object sender, EventArgs e)
        {
            useRhinoUnits = false;
            length = LengthUnit.Foot.ToString();
            force = ForceUnit.KilopoundForce.ToString();
            moment = MomentUnit.KilopoundForceFoot.ToString();
            stress = PressureUnit.KilopoundForcePerSquareInch.ToString();
            strain = StrainUnit.Percent.ToString();
            axialstiffness = AxialStiffnessUnit.KilopoundForce.ToString();
            curvature = CurvatureUnit.PerMeter.ToString();
            bendingstiffness = BendingStiffnessUnit.PoundForceSquareFoot.ToString();
            updateSelections();
        }

        private void buttonkipIn_Click(object sender, EventArgs e)
        {
            useRhinoUnits = false;
            length = LengthUnit.Inch.ToString();
            force = ForceUnit.KilopoundForce.ToString();
            moment = MomentUnit.KilopoundForceInch.ToString();
            stress = PressureUnit.KilopoundForcePerSquareInch.ToString();
            strain = StrainUnit.Percent.ToString();
            axialstiffness = AxialStiffnessUnit.KilopoundForce.ToString();
            curvature = CurvatureUnit.PerMeter.ToString();
            bendingstiffness = BendingStiffnessUnit.PoundForceSquareInch.ToString();
            updateSelections();
        }

        private void labelDescription_Click(object sender, EventArgs e)
        {

        }

        private void labelBendingStiffness_Click(object sender, EventArgs e)
        {

        }
    }
}
