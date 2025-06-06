﻿using System.IO;
using System.Reflection;

using Grasshopper.Kernel;

using Xunit;

namespace IntegrationTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class AdSecGH_Example2_CompositeColumnTests {
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
    [InlineData("CadTest", 14)]
    [InlineData("NmDiagramTest", 120215.428945)]
    [InlineData("ConcreteStressTest", new[] {
      -0.373978,
      -7.684297,
      -0.16744,
      -6.056018,
    })]
    [InlineData("RebarUlsStrainTest", new[] {
      0.241326,
      0.220247,
      0.102692,
      -0.056334,
      -0.182421,
      -0.216571,
      -0.142805,
      0.005074,
      0.156066,
    })]
    [InlineData("RebarUlsStressTest", new[] {
      48.265163,
      44.04935,
      20.538307,
      -11.26689,
      -36.484234,
      -43.31425,
      -28.561098,
      1.014877,
      31.213118,
    })]
    [InlineData("RebarSlsStrainTest", new[] {
      0.064542,
      0.055894,
      0.01106,
      -0.04898,
      -0.096133,
      -0.108336,
      -0.079879,
      -0.024077,
      0.032959,
    })]
    [InlineData("RebarSlsStressTest", new[] {
      12.908482,
      11.178755,
      2.212077,
      -9.795945,
      -19.226623,
      -21.667238,
      -15.9758,
      -4.815395,
      6.591898,
    })]
    [InlineData("LoadUtilisationTest", 0.128236)]
    [InlineData("CrackUtilisationTest", 1.315612)]
    public void Test(string groupIdentifier, object expected) {
      var param = Helper.FindParameter(Document, groupIdentifier);
      Helper.TestGHPrimitives(param, expected);
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
