using Grasshopper.Kernel;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace IntegrationTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class AdSecGH_Example1_RectangularBeamTests {
    public static GH_Document Document {
      get {
        if (_document == null)
          _document = OpenDocument();
        return _document;
      }
    }
    private static GH_Document _document = null;

    [Fact]
    public void NoRuntimeErrorTest() {
      Helper.TestNoRuntimeMessagesInDocument(Document, GH_RuntimeMessageLevel.Error);
      Helper.TestNoRuntimeMessagesInDocument(Document, GH_RuntimeMessageLevel.Warning);
    }

    [Theory]
    [InlineData("CadTest", 15)]
    [InlineData("NmDiagramTest", 116003.838766)]
    [InlineData("ConcreteStressTest", new double[] { -0.327292, -5.108709, -0.13875, -4.642858 }, 5)]
    [InlineData("RebarUlsStrainTest", new double[] { 0.262073, 0.216727, 0.166176, 0.115644, 0.145305, 0.01743, -0.134224, -0.006349, -0.104581, -0.15289, -0.201211, -0.246279 })]
    [InlineData("RebarUlsStressTest", new double[] { 52.414556, 43.34542, 33.235123, 23.128806, 29.061078, 3.486051, -26.844842, -1.269816, -20.916174, -30.577941, -40.242196, -49.255725 })]
    [InlineData("RebarSlsStrainTest", new double[] { 0.058393, 0.044786, 0.029483, 0.014066, 0.017519, -0.027111, -0.073019, -0.028389, -0.069464, -0.084162, -0.09879, -0.112358 })]
    [InlineData("RebarSlsStressTest", new double[] { 11.678512, 8.957159, 5.896622, 2.813283, 3.503728, -5.422205, -14.603815, -5.677882, -13.892725, -16.83249, -19.758003, -22.471527 })]
    [InlineData("LoadUtilisationTest", 0.137663)]
    [InlineData("CrackUtilisationTest", 1.423388)]
    public void Test(string groupIdentifier, object expected, int tolerance = 6) {
      IGH_Param param = Helper.FindParameter(Document, groupIdentifier);
      Helper.TestGHPrimitives(param, expected, tolerance);
    }

    private static GH_Document OpenDocument() {
      Type thisClass = MethodBase.GetCurrentMethod().DeclaringType;
      string fileName = thisClass.Name + ".gh";
      fileName = fileName.Replace(thisClass.Namespace, string.Empty).Replace("Tests", string.Empty);

      string solutiondir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;
      string path = Path.Combine(new string[] { solutiondir, "ExampleFiles" });

      return Helper.CreateDocument(Path.Combine(path, fileName));
    }
  }
}
