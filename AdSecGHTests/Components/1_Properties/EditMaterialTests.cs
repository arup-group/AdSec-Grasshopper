using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components._01_Properties {
  [Collection("GrasshopperFixture collection")]
  public class EditMaterialTests {
    private readonly EditMaterial _component;

    public EditMaterialTests() {
      _component = new EditMaterial();
      SetEditMaterialInputs();
    }

    private MaterialDesign CreateMaterialAndCode() {
      return new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = new DesignCode() { IDesignCode = IS456.Edition_2000 },
      };
    }

    private StressStrainCurve CreateExplicitCompressionCurve(bool uls = true) {
      var explicitCurve = IExplicitStressStrainCurve.Create();
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.Zero, Strain.Zero));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(10), Strain.FromRatio(uls ? 0.0018 : 0.0021)));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(10), Strain.FromRatio(uls ? 0.0036 : 0.0042)));
      return new StressStrainCurve() { IStressStrainCurve = explicitCurve, IsCompression = true };
    }

    private StressStrainCurve CreateExplicitTensionCurve(bool uls = true) {
      var explicitCurve = IExplicitStressStrainCurve.Create();
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.Zero, Strain.Zero));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(12), Strain.FromRatio(uls ? 0.002 : 0.0022)));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(24), Strain.FromRatio(uls ? 0.004 : 0.0044)));
      return new StressStrainCurve() { IStressStrainCurve = explicitCurve, IsCompression = false };
    }

    private void SetEditMaterialInputs() {
      var material = CreateMaterialAndCode();
      ComponentTestHelper.SetInput(_component, new AdSecMaterialGoo(material), 0);
      ComponentTestHelper.SetInput(_component, new AdSecDesignCodeGoo(new DesignCode() { IDesignCode = IS456.Edition_2000, DesignCodeName = "IS456.Edition_2000" }), 1);
      ComponentTestHelper.SetInput(_component, AdSecStressStrainCurveGoo.Create(CreateExplicitCompressionCurve().IStressStrainCurve, true), 2);
      ComponentTestHelper.SetInput(_component, AdSecStressStrainCurveGoo.Create(CreateExplicitTensionCurve().IStressStrainCurve, false), 3);
      ComponentTestHelper.SetInput(_component, AdSecStressStrainCurveGoo.Create(CreateExplicitCompressionCurve(false).IStressStrainCurve, true), 4);
      ComponentTestHelper.SetInput(_component, AdSecStressStrainCurveGoo.Create(CreateExplicitTensionCurve(false).IStressStrainCurve, false), 5);
      var crackCalcParamsGoo = new AdSecConcreteCrackCalculationParametersGoo(
        IConcreteCrackCalculationParameters.Create(Pressure.FromMegapascals(10), Pressure.FromPascals(-10), Pressure.FromPascals(4)));
      ComponentTestHelper.SetInput(_component, crackCalcParamsGoo, 6);

    }


    [Fact]
    public void ShouldEditMaterialUlsCompression() {
      var expectedStrain = CreateExplicitCompressionCurve().IStressStrainCurve.FailureStrain;
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component, 2);
      AssertFailureStrainEqual(expectedStrain, result);
    }

    [Fact]
    public void ShouldEditMaterialUlstension() {
      var expectedStrain = CreateExplicitTensionCurve().IStressStrainCurve.FailureStrain;
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component, 3);
      AssertFailureStrainEqual(expectedStrain, result);
    }

    [Fact]
    public void ShouldEditMaterialSlsCompression() {
      var expectedStrain = CreateExplicitCompressionCurve(false).IStressStrainCurve.FailureStrain;
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component, 4);
      AssertFailureStrainEqual(expectedStrain, result);
    }

    [Fact]
    public void ShouldEditMaterialSlstension() {
      var expectedStrain = CreateExplicitTensionCurve(false).IStressStrainCurve.FailureStrain;
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component, 5);
      AssertFailureStrainEqual(expectedStrain, result);
    }

    private static void AssertFailureStrainEqual(Strain expectedStrain, AdSecStressStrainCurveGoo result) {
      Assert.Equal(expectedStrain.ToUnit(StrainUnit.Ratio), result.Value.IStressStrainCurve.FailureStrain.ToUnit(StrainUnit.Ratio));
    }

    [Fact]
    public void ShouldEditCrackParameter() {
      var result = (AdSecConcreteCrackCalculationParametersGoo)ComponentTestHelper.GetOutput(_component, 6);
      Assert.Equal(Pressure.FromMegapascals(10), result.Value.ElasticModulus.ToUnit(PressureUnit.Megapascal));
      Assert.Equal(Pressure.FromPascals(-10), result.Value.CharacteristicCompressiveStrength.ToUnit(PressureUnit.Pascal));
      Assert.Equal(Pressure.FromPascals(4), result.Value.CharacteristicTensileStrength.ToUnit(PressureUnit.Pascal));
    }

    [Fact]
    public void ShouldEditDesignCode() {
      var result = (AdSecDesignCodeGoo)ComponentTestHelper.GetOutput(_component, 1);
      Assert.Equal("IS456.Edition_2000", result.Value.DesignCodeName);
    }

    [Fact]
    public void MaterialOutputShouldNotNull() {
      SetEditMaterialInputs();
      var result = (AdSecMaterialGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(result);
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.EditMaterial));
    }
  }
}
