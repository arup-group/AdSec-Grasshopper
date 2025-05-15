using System.Reflection;

using AdSecCore.Functions;

using AdSecCoreTests.Functions;

using AdSecGHCore;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.IO.Serialization;
using Oasys.AdSec.StandardMaterials;

namespace AdSecCore {
  public class SaveLoadTests {
    [Fact]
    public void SimpleCompositeSection() {
      var designCode = IS456.Edition_2000;
      var section = SampleData.GetCompositeSectionDesign().Section;
      Assert.True(TrySaveAndLoad(designCode, section));
    }

    [Fact]
    public void EditSectionPassThroughForConsistentRebarAndConcreteGrade() {
      var designCode = IS456.Edition_2000;
      var sectionDesign = SampleData.GetCompositeSectionDesign();
      var function = new EditSectionFunction();
      function.Section.Value = sectionDesign;
      function.Compute();

      Assert.True(TrySaveAndLoad(designCode, function.SectionOut.Value.Section));
    }

    [Fact]
    public void EditSectionWillNotPassThroughForInConsistentRebarAndConcreteGrade() {
      var designCode = IS456.Edition_2000;
      var sectionDesign = SampleData.GetCompositeSectionDesign(Concrete.AS5100.Edition_2017.MPA100);
      var function = new EditSectionFunction();
      function.Section.Value = sectionDesign;
      function.Compute();

      var exception = Assert.Throws<TargetInvocationException>(() => TrySaveAndLoad(designCode, function.SectionOut.Value.Section));
      var message = exception.InnerException?.Message;
      Assert.Contains("The section is invalid", message);
    }

    private static bool TrySaveAndLoad(IDesignCode designCode, ISection section) {
      var jsonConverter = new JsonConverter(designCode);
      var json = jsonConverter.SectionToJson(section);
      string fileName = "test.ads";
      File.WriteAllText(fileName, json);

      string jsonRead = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(jsonRead);
      if (jsonParser.Sections.Count != 1) {
        return false;
      }

      var sectionOut = jsonParser.Sections.First();

      return Compare.Equal(section, sectionOut);
    }
  }
}
