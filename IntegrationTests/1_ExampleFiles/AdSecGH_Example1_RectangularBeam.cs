using System.IO;
using System.Reflection;

using AdSecCore.Functions;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using Xunit;

namespace IntegrationTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class AdSecGH_Example1_RectangularBeamTests {
    private static GH_Document _document;
    public static GH_Document Document {
      get {
        if (_document == null) {
          _document = OpenDocument();
        }

        return _document;
      }
    }

    [Fact]
    public void NoRuntimeErrorTest() {
      Assert.Empty(Helper.TestNoRuntimeMessagesInDocument(Document, GH_RuntimeMessageLevel.Error));
      Assert.Empty(Helper.TestNoRuntimeMessagesInDocument(Document, GH_RuntimeMessageLevel.Warning));
    }

    [Theory]
    [InlineData("CadTest", 15)]
    [InlineData("NmDiagramTest", 116003.838766)]
    [InlineData("ConcreteStressTest", new[] {
      -0.327292,
      -5.108709,
      -0.13875,
      -4.642858,
    })]
    [InlineData("RebarUlsStrainTest", new[] {
      0.262073,
      0.216727,
      0.166176,
      0.115644,
      0.145305,
      0.016448,
      -0.134224,
      -0.007447,
      -0.104581,
      -0.15289,
      -0.201211,
      -0.246279,
    })]
    [InlineData("RebarUlsStressTest", new[] {
      52.414556,
      43.34542,
      33.235123,
      23.128806,
      29.061078,
      3.289703,
      -26.844842,
      -1.4859736,
      -20.916174,
      -30.577941,
      -40.242196,
      -49.255725,
    })]
    [InlineData("RebarSlsStrainTest", new[] {
      0.058393,
      0.044786,
      0.029483,
      0.014066,
      0.017519,
      -0.027111,
      -0.073019,
      -0.028389,
      -0.069464,
      -0.084162,
      -0.09879,
      -0.112358,
    })]
    [InlineData("RebarSlsStressTest", new[] {
      11.678512,
      8.957159,
      5.896622,
      2.813283,
      3.503728,
      -5.422205,
      -14.603815,
      -5.677882,
      -13.892725,
      -16.83249,
      -19.758003,
      -22.471527,
    })]
    [InlineData("LoadUtilisationTest", 0.137663)]
    [InlineData("CrackUtilisationTest", 1.423388)]
    [InlineData("Neutral Offset", -0.004332)]
    [InlineData("Neutral Angle", 0.790348)]
    [InlineData("Neutral Failure Offset", 0.002438)]
    [InlineData("Neutral Failure Angle", 0.888143)]
    public void Test(string groupIdentifier, object expected) {
      var param = Helper.FindParameter(Document, groupIdentifier);
      Helper.TestGHPrimitives(param, expected);
    }

    [Fact]
    public void ShouldContainNeutralAxis() {
      var groupName = "Neutral Axis";
      var param = Helper.FindParameter(Document, groupName);
      AdSecNeutralAxisGoo line = param.VolatileData.get_Branch(0)[0] as AdSecNeutralAxisGoo;
      Assert.NotNull(line);
      Assert.Equal(new Point3d(-0.232384, -0.240854, 0), line.AxisLine.From);
      Assert.Equal(new Point3d(0.238541, 0.234757, 0), line.AxisLine.To);
    }

    [Fact]
    public void ShouldContainNeutralFailureAxis() {
      var groupName = "Failure Neutral Axis";
      var param = Helper.FindParameter(Document, groupName);
      AdSecNeutralAxisGoo line = param.VolatileData.get_Branch(0)[0] as AdSecNeutralAxisGoo;
      Assert.NotNull(line);
      Assert.Equal(new Point3d(-0.218759, -0.265192, 0), line.AxisLine.From);
      Assert.Equal(new Point3d(0.214976, 0.268269, 0), line.AxisLine.To);
    }

    private static GH_Document OpenDocument() {
      var thisClass = MethodBase.GetCurrentMethod().DeclaringType;
      string fileName = $"{thisClass.Name}.gh";
      fileName = fileName.Replace(thisClass.Namespace, string.Empty).Replace("Tests", string.Empty);

      string solutiondir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;
      string path = Path.Combine(new[] {
        solutiondir,
        "ExampleFiles",
      });

      return Helper.CreateDocument(Path.Combine(path, fileName));
    }
  }
}
