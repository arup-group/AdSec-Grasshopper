using AdSecCore.Constants;
using AdSecCore.Functions;

using Oasys.AdSec.Materials;

namespace AdSecCoreTests.Constants {
  public class GsaMaterialParserTests {

    [Fact]
    public void DictionaryShouldContainEC2Entry() {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey("EC2-1-1"));
      Assert.Equal("EC2_04", GsaMaterialParser.GsaCodeToAdSecKey["EC2-1-1"]);
    }

    [Fact]
    public void DictionaryShouldContainCsaA2314Entry() {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey("CSA A23.3-14"));
      Assert.Equal("CSA_A23_3_14", GsaMaterialParser.GsaCodeToAdSecKey["CSA A23.3-14"]);
    }

    [Fact]
    public void DictionaryShouldContainCsaA2304Entry() {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey("CSA A23.3-04"));
      Assert.Equal("CSA_A23_3_04", GsaMaterialParser.GsaCodeToAdSecKey["CSA A23.3-04"]);
    }

    [Fact]
    public void DictionaryShouldBeCaseInsensitive() {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey("ec2-1-1"));
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey("EC2-1-1"));
    }

    [Theory]
    [InlineData("ACI 318-02", "ACI318_02")]
    [InlineData("ACI 318M-14", "ACI318M_14")]
    [InlineData("AASHTO 17", "AASHTO_17")]
    [InlineData("AASHTO 17M", "AASHTO_17M")]
    [InlineData("AS3600-09", "AS3600_09")]
    [InlineData("AS3600-18", "AS3600_18")]
    [InlineData("BS8110-97", "BS8110_97")]
    [InlineData("CSA S6-14", "CSA_S6_14")]
    [InlineData("EC2-2", "EC2_2_05")]
    [InlineData("HK CoP (04)", "HKCP_04")]
    [InlineData("HK CoP (13)", "HKCP_13")]
    [InlineData("HK SDM (13)", "HKSDM_13")]
    [InlineData("IRS Bridge (97)", "IRS_BRIDGE_97")]
    [InlineData("IS 456", "IS456_2000")]
    [InlineData("IRC:112", "IRC112_2011")]
    public void DictionaryShouldMapAllSupportedCodes(string gsaCode, string expectedKey) {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey(gsaCode));
      Assert.Equal(expectedKey, GsaMaterialParser.GsaCodeToAdSecKey[gsaCode]);
    }

    [Theory]
    [InlineData("EC2-1-1 (CY)", "EC2_CY_04")]
    [InlineData("EC2-1-1 (DE)", "EC2_DE_04")]
    [InlineData("EC2-1-1 (DK)", "EC2_DK_04")]
    [InlineData("EC2-1-1 (ES)", "EC2_ES_04")]
    [InlineData("EC2 (FI)", "EC2_FI_04")]
    [InlineData("EC2 (FR)", "EC2_FR_04")]
    [InlineData("EC2 (GB)", "EC2_GB_04")]
    [InlineData("EC2 (IE)", "EC2_IE_04")]
    [InlineData("EC2 (IT)", "EC2_IT_04")]
    [InlineData("EC2 (NL)", "EC2_NL_04")]
    [InlineData("EC2 (NO)", "EC2_NO_04")]
    [InlineData("EC2 (PL)", "EC2_PL_04")]
    [InlineData("EC2-1-1 (SG)", "EC2_SG_04")]
    [InlineData("BS EC2/PD:06", "BS_EC2_PD_06")]
    [InlineData("BS EC2/PD:10", "BS_EC2_PD_10")]
    public void DictionaryShouldMapEc2Part1NationalAnnexCodes(string gsaCode, string expectedKey) {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey(gsaCode));
      Assert.Equal(expectedKey, GsaMaterialParser.GsaCodeToAdSecKey[gsaCode]);
    }

    [Theory]
    [InlineData("EC2-2 (DE)", "EC2_2_DE_05")]
    [InlineData("EC2-2 (DK)", "EC2_2_DK_05")]
    [InlineData("EC2-2 (ES)", "EC2_2_ES_05")]
    [InlineData("EC2-2 (FR)", "EC2_2_FR_05")]
    [InlineData("EC2-2 (GB)", "EC2_2_GB_05")]
    [InlineData("EC2-2 (IE)", "EC2_2_IE_05")]
    [InlineData("EC2-2 (IT)", "EC2_2_IT_05")]
    [InlineData("EC2-2 (NL)", "EC2_2_NL_05")]
    [InlineData("EC2-2 (SG)", "EC2_2_SG_05")]
    public void DictionaryShouldMapEc2Part2NationalAnnexCodes(string gsaCode, string expectedKey) {
      Assert.True(GsaMaterialParser.GsaCodeToAdSecKey.ContainsKey(gsaCode));
      Assert.Equal(expectedKey, GsaMaterialParser.GsaCodeToAdSecKey[gsaCode]);
    }

    [Fact]
    public void ShouldReturnFalseForNullInput() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(null, out var material);

      Assert.False(result);
      Assert.Null(material);
    }

    [Fact]
    public void ShouldReturnFalseForEmptyInput() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(string.Empty, out var material);

      Assert.False(result);
      Assert.Null(material);
    }

    [Fact]
    public void ShouldReturnFalseForStringWithoutParentheses() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial("EC2-1-1 Concrete C12/15", out var material);

      Assert.False(result);
      Assert.Null(material);
    }

    [Fact]
    public void ShouldReturnFalseForNonConcreteInput() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial("GSA Material (EC2-1-1 Steel S355)", out var material);

      Assert.False(result);
      Assert.Null(material);
    }

    [Fact]
    public void ShouldUseFallbackForUnsupportedDesignCode() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (UNKNOWN CODE Concrete C30)", out var material, out string warning);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.NotNull(warning);
    }

    [Fact]
    public void ShouldParseEc2ConcreteC12_15() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 Concrete C12/15)", out var material);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.IsAssignableFrom<IConcrete>(material.Material);
      Assert.Equal("C12_15", material.GradeName);
    }

    [Fact]
    public void ShouldParseEc2MaterialAndSetDesignCodeName() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 Concrete C12/15)", out var material);

      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+NoNationalAnnex", material.DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldParseEc2MaterialAndSetDesignCode() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 Concrete C12/15)", out var material);

      Assert.NotNull(material.DesignCode.IDesignCode);
    }

    [Fact]
    public void ShouldParseEc2WithDeNationalAnnex() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 (DE) Concrete C30/37)", out var material);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.IsAssignableFrom<IConcrete>(material.Material);
      Assert.Equal("C30_37", material.GradeName);
    }

    [Fact]
    public void ShouldSetDesignCodeNameForDeNationalAnnex() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 (DE) Concrete C30/37)", out var material);

      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+DE+Edition_2013", material.DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldParseEc2WithGbNationalAnnex() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2 (GB) Concrete C32/40)", out var material);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+GB+Edition_2014", material.DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldParseCsaA2314Concrete20MPa() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (CSA A23.3-14 Concrete 20 MPa)", out var material);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.IsAssignableFrom<IConcrete>(material.Material);
    }

    [Fact]
    public void ShouldParseCsaGradeByNumericFallback() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (CSA A23.3-14 Concrete 20 MPa)", out var material);

      Assert.NotNull(material);
      Assert.Equal("CSA+A23_3+Edition_2014", material.DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldProduceCorrectAdSecMaterialStringForEc2() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 Concrete C12/15)", out var material);

      Assert.Equal("C12_15", material.GradeName);
      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+NoNationalAnnex", material.DesignCode.DesignCodeName);
    }

    [Fact]
    public void FallbackShouldHandleConcreteFirstWithEmbeddedGrade() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (Concrete Grd:3 Test (C25/30))", out var material, out string warning);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.Equal("C25_30", material.GradeName);
      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+NoNationalAnnex", material.DesignCode.DesignCodeName);
      Assert.NotNull(warning);
    }

    [Fact]
    public void FallbackShouldReturnTrueForUnknownDesignCode() {
      bool result = GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (UNKNOWN CODE Concrete C25/30)", out var material, out string warning);

      Assert.True(result);
      Assert.NotNull(material);
      Assert.NotNull(warning);
    }

    [Fact]
    public void FallbackShouldUseEc2NoNationalAnnex() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (UNKNOWN CODE Concrete C25/30)", out var material, out _);

      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+NoNationalAnnex", material.DesignCode.DesignCodeName);
    }

    [Fact]
    public void FallbackShouldMatchGradeFromInputWhenAvailable() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (UNKNOWN CODE Concrete C25/30)", out var material, out string warning);

      Assert.Equal("C25_30", material.GradeName);
      Assert.DoesNotContain("not found", warning ?? string.Empty);
    }

    [Fact]
    public void FallbackShouldUseC30_37WhenGradeNotInCatalogue() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (UNKNOWN CODE Concrete ZZZUNKNOWN)", out var material, out string warning);

      Assert.NotNull(material);
      Assert.Equal("C30_37", material.GradeName);
      Assert.Contains("ZZZUNKNOWN", warning);
    }

    [Fact]
    public void FallbackWarningShouldDescribeSubstitution() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (UNKNOWN CODE Concrete C99/99)", out _, out string warning);

      Assert.NotNull(warning);
      Assert.Contains("UNKNOWN CODE", warning);
    }

    [Fact]
    public void NoWarningShouldBeSetWhenParseIsExact() {
      GsaMaterialParser.TryParseConcreteMaterial(
        "GSA Material (EC2-1-1 Concrete C12/15)", out _, out string warning);

      Assert.Null(warning);
    }
  }
}
