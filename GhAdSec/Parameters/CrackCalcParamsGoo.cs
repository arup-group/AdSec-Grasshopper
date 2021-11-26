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

namespace AdSecGH.Parameters
{
    public class AdSecConcreteCrackCalculationParametersGoo : GH_Goo<IConcreteCrackCalculationParameters>
    {
        public AdSecConcreteCrackCalculationParametersGoo(IConcreteCrackCalculationParameters concreteCrackCalculationParameters)
        : base(concreteCrackCalculationParameters)
        {
        }
        public AdSecConcreteCrackCalculationParametersGoo(UnitsNet.Pressure elasticModulus, UnitsNet.Pressure characteristicCompressiveStrength, UnitsNet.Pressure characteristicTensionStrength)
        {
            this.Value = IConcreteCrackCalculationParameters.Create(
                elasticModulus, 
                characteristicCompressiveStrength, 
                characteristicTensionStrength);
        }
        public AdSecConcreteCrackCalculationParametersGoo(double elasticModulus, double characteristicCompressiveStrength, double characteristicTensionStrength)
        {
            this.Value = IConcreteCrackCalculationParameters.Create(
                new UnitsNet.Pressure(elasticModulus, Units.StressUnit),
                new UnitsNet.Pressure(characteristicCompressiveStrength, Units.StressUnit),
                new UnitsNet.Pressure(characteristicTensionStrength, Units.StressUnit));
        }

        public IConcreteCrackCalculationParameters ConcreteCrackCalculationParameters
        {
            get { return this.Value; }
        }

        public override bool IsValid => true;

        public override string TypeName => "ConcreteCrackCalculationParameters";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecConcreteCrackCalculationParametersGoo(this.Value.ElasticModulus, Value.CharacteristicCompressiveStrength, Value.CharacteristicTensileStrength);
        }

        public override string ToString()
        {
            // recreate pressure values with document units
            UnitsNet.Pressure e = new UnitsNet.Pressure(this.Value.ElasticModulus.As(Units.StressUnit), Units.StressUnit);
            UnitsNet.Pressure fck = new UnitsNet.Pressure(this.Value.CharacteristicCompressiveStrength.As(Units.StressUnit), Units.StressUnit);
            UnitsNet.Pressure ftk = new UnitsNet.Pressure(this.Value.CharacteristicTensileStrength.As(Units.StressUnit), Units.StressUnit);
            return "AdSec " + TypeName +
                " {E:" + e.ToString() +
                ", fc:" + fck.ToString() +
                ", ft: " + ftk.ToString() + "}";
        }
    }
}
