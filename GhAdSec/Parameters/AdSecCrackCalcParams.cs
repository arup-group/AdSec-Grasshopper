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

namespace GhAdSec.Parameters
{
    public class AdSecConcreteCrackCalculationParameters : GH_Goo<IConcreteCrackCalculationParameters>
    {
        public AdSecConcreteCrackCalculationParameters(IConcreteCrackCalculationParameters concreteCrackCalculationParameters)
        : base(concreteCrackCalculationParameters)
        {
        }
        public AdSecConcreteCrackCalculationParameters(UnitsNet.Pressure elasticModulus, UnitsNet.Pressure characteristicCompressiveStrength, UnitsNet.Pressure characteristicTensionStrength)
        {
            this.Value = IConcreteCrackCalculationParameters.Create(
                elasticModulus, 
                characteristicCompressiveStrength, 
                characteristicTensionStrength);
        }
        public AdSecConcreteCrackCalculationParameters(double elasticModulus, double characteristicCompressiveStrength, double characteristicTensionStrength)
        {
            this.Value = IConcreteCrackCalculationParameters.Create(
                new UnitsNet.Pressure(elasticModulus, GhAdSec.DocumentUnits.PressureUnit),
                new UnitsNet.Pressure(characteristicCompressiveStrength, GhAdSec.DocumentUnits.PressureUnit),
                new UnitsNet.Pressure(characteristicTensionStrength, GhAdSec.DocumentUnits.PressureUnit));
        }

        public IConcreteCrackCalculationParameters ConcreteCrackCalculationParameters
        {
            get { return this.Value; }
        }

        public override bool IsValid => true;

        public override string TypeName => "AdSec CCP";

        public override string TypeDescription => "AdSec Concrete Crack-Calculation Parameters";

        public override IGH_Goo Duplicate()
        {
            return new AdSecConcreteCrackCalculationParameters(this.Value.ElasticModulus, Value.CharacteristicCompressiveStrength, Value.CharacteristicTensileStrength);
        }

        public override string ToString()
        {
            return TypeName + 
                " E:" + this.Value.ElasticModulus.As(GhAdSec.DocumentUnits.PressureUnit) + 
                " fck:" + this.Value.CharacteristicCompressiveStrength.As(GhAdSec.DocumentUnits.PressureUnit) + 
                " ftk: " + this.Value.CharacteristicTensileStrength.As(GhAdSec.DocumentUnits.PressureUnit);
        }
    }
}
