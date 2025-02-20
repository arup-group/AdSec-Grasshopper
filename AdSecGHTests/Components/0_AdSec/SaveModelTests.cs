using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysUnits;

using Rhino.NodeInCode;

using Xunit;

using static System.Collections.Specialized.BitVector32;

namespace AdSecGHTests.Components._1_Properties {
  [Collection("GrasshopperFixture collection")]
  public class SaveModelTests {
    private static string tempPath = string.Empty;
    private static SaveModel _component;
    public SaveModelTests() {
      _component = ComponentMother();
      SetSections(new List<object> { AdSecUtility.SectionObject(), AdSecUtility.SectionObject() });
    }

    public static SaveModel ComponentMother() {
      var component = new SaveModel();
      component.CreateAttributes();
      tempPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".ads";
      return component;
    }

    private static void SetSections(List<object> sections) {
      ComponentTestHelper.SetListInput(_component, sections, 0);
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetFilePath(bool isSave = true) {
      ComponentTestHelper.SetInput(_component, tempPath, 3);
      if (isSave) {
        ComponentTestHelper.SetInput(_component, true, 2);
      }
    }

    private static void SetLoad() {
      var tree = new DataTree<object>();
      tree.Add(new AdSecLoadGoo(ILoad.Create(Force.FromKilonewtons(-100), Moment.Zero, Moment.Zero)));
      ComponentTestHelper.SetInput(_component, tree, 1);
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetWrongLoad() {
      var tree = new DataTree<object>();
      tree.Add(5);
      ComponentTestHelper.SetInput(_component, tree, 1);
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetWrongFilePath() {
      tempPath = "C:\\abcd\\";
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetNullFilePath() {
      tempPath = null;
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetEmptyFilePath() {
      tempPath = "";
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetDifferentTypesOfLoad() {
      var tree = new GH_Structure<IGH_Goo>();
      tree.Append(new AdSecLoadGoo(ILoad.Create(Force.FromKilonewtons(-100), Moment.Zero, Moment.Zero)), new GH_Path(0));
      tree.Append(new AdSecDeformationGoo(IDeformation.Create(Strain.FromMilliStrain(-1), Curvature.Zero, Curvature.Zero)), new GH_Path(0));
      ComponentTestHelper.SetInput(_component, tree, 1);
      ComponentTestHelper.ComputeData(_component);
    }

    [Fact]
    public void SectionWillBeEmptyWhenWrongSectionHasBeenSet() {
      _component = ComponentMother();
      SetSections(new List<object> { 1 });
      var sections = AdSecFile.ReadSection(tempPath);
      Assert.Empty(sections);
    }

    [Fact]
    public void OpeningModelIsGivingCorrectSectionProfile() {
      SetLoad();
      var sections = AdSecFile.ReadSection(tempPath);
      Assert.Equal(2, sections.Count);
      Assert.Equal("STD R(m) 0.6 0.3", sections[0].Profile.Description());
    }

    [Fact]
    public void SaveFileIsOptionIsWorking() {
      SetFilePath(false);
      ComponentTestHelper.ComputeData(_component);
      _component.SaveFile();
      var sections = AdSecFile.ReadSection(tempPath);
      Assert.Equal(2, sections.Count);
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
      var process = _component.OpenAdSecexe();
      if (process != null) {
        try {
          Assert.Contains("AdSec", process.ProcessName);
        } finally {
          process.Kill();
        }
      }
    }

    [Fact]
    public void WrongFilePathWillNotLaunchAdSecProcess() {
      SetWrongFilePath();
      var process = _component.OpenAdSecexe();
      Assert.Null(process);
    }

    [Fact]
    public void NullFilePathWillNotLaunchAdSecProcess() {
      SetNullFilePath();
      var process = _component.OpenAdSecexe();
      Assert.Null(process);
    }

    [Fact]
    public void EmptyFilePathWillNotLaunchAdSecProcess() {
      SetEmptyFilePath();
      var process = _component.OpenAdSecexe();
      Assert.Null(process);
    }
  }
}
