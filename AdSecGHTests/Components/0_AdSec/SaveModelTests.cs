using System;
using System.IO;
using System.Windows.Forms;

using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components._1_Properties {
  [Collection("GrasshopperFixture collection")]
  public class SaveModelTests {
    private static string tempPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".ads";
    private static SaveModel _component;
    public SaveModelTests() {
      _component = ComponentMother();
    }

    public static SaveModel ComponentMother() {
      var component = new SaveModel();
      component.CreateAttributes();
      ComponentTestHelper.SetInput(component, AdSecUtility.SectionObject(), 0);
      return component;
    }

    private static void ComputeData() {
      _component.ExpireSolution(true);
      _component.CollectData();
      _component.ComputeData();
    }

    private static void SetFilePath(string path) {
      ComponentTestHelper.SetInput(_component, path, 3);
      ComponentTestHelper.SetInput(_component, true, 2);
    }

    private static void SetLoad() {
      var tree = new DataTree<object>();
      tree.Add(new AdSecLoadGoo(ILoad.Create(Force.FromKilonewtons(-100), Moment.Zero, Moment.Zero)));
      ComponentTestHelper.SetInput(_component, tree, 1);
      SetFilePath(tempPath);
      ComputeData();
    }

    private static void SetWrongLoad() {
      var tree = new DataTree<object>();
      tree.Add(5);
      ComponentTestHelper.SetInput(_component, tree, 1);
      SetFilePath(tempPath);
      ComputeData();
    }

    private static void SetWrongFilePath() {
      SetFilePath("C:\\abcd\\");
      ComputeData();
    }

    private static void SetDifferentTypesOfLoad() {
      var tree = new GH_Structure<IGH_Goo>();
      tree.Append(new AdSecLoadGoo(ILoad.Create(Force.FromKilonewtons(-100), Moment.Zero, Moment.Zero)), new GH_Path(0));
      tree.Append(new AdSecDeformationGoo(IDeformation.Create(Strain.FromMilliStrain(-1), Curvature.Zero, Curvature.Zero)), new GH_Path(0));
      ComponentTestHelper.SetInput(_component, tree, 1);
      ComputeData();
    }

    [Fact]
    public void OpeningModelIsGivingCorrectSectionProfile() {
      SetLoad();
      var sections = AdSecFile.ReadSection(tempPath);
      Assert.Single(sections);
      Assert.Equal("STD R(m) 0.6 0.3", sections[0].Profile.Description());
    }

    [Fact]
    public void SettingTwoDifferentTypeOfLoadForSectionWillBeAnError() {
      SetDifferentTypesOfLoad();
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
    }

    [Fact]
    public void ThereWillWarningWhenInputLoadIsNotCorrect() {
      SetWrongLoad();
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeMessages);
    }


    [Fact]
    public void AdSecProcesscanBeLaunched() {
      SetLoad();
      var process = _component.RunAdSec();
      if (process != null) {
        try {
          Assert.Contains("AdSec", process.ProcessName);
        } finally {
          process.Kill();
        }
      }
    }

    [Fact]
    public void WrongFilePathWillNotLaunchAdSecProcesscan() {
      SetWrongFilePath();
      var process = _component.RunAdSec();
      Assert.Null(process);
    }
  }
}
