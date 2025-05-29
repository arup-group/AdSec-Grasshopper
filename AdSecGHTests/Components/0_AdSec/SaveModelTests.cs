using System;
using System.Collections.Generic;
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
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components.AdSec {
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
      tempPath = GetValidPath();
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

    private static GH_Structure<IGH_Goo> CreateLoad(bool mix = false) {
      var tree = new GH_Structure<IGH_Goo>();
      tree.Append(new AdSecLoadGoo(ILoad.Create(Force.FromKilonewtons(-100), Moment.Zero, Moment.Zero)), new GH_Path(0));
      if (mix) {
        tree.Append(new AdSecDeformationGoo(IDeformation.Create(Strain.FromMilliStrain(-1), Curvature.Zero, Curvature.Zero)), new GH_Path(0));
      }
      return tree;
    }

    private static GH_Structure<IGH_Goo> CreateNullLoad() {
      var tree = new GH_Structure<IGH_Goo>();
      tree.Append(null, new GH_Path(0));
      return tree;
    }

    private static DataTree<object> CreateWrongLoad() {
      var tree = new DataTree<object>();
      tree.Add(5);
      return tree;
    }

    private static void SetLoadAndPath(bool mix = false) {
      SetFilePath();
      ComponentTestHelper.SetInput(_component, CreateLoad(mix), 1);
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetWrongLoad() {
      SetFilePath();
      ComponentTestHelper.SetInput(_component, CreateWrongLoad(), 1);
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetWrongFilePath() {
      tempPath = "C:\\abcd\\";
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetNullFilePath(bool save = true) {
      tempPath = null;
      SetFilePath(save);
      ComponentTestHelper.ComputeData(_component);
    }

    private static void SetEmptyFilePath() {
      tempPath = string.Empty;
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
    }

    private static string GetValidPath(bool extra = false) {
      string additional = extra ? Guid.NewGuid().ToString() : string.Empty;
      return $"{Path.GetTempPath()}{Guid.NewGuid()}{additional}.ads";
    }

    private static void ChangeFileNameTo(string path) {
      tempPath = path;
      ComponentTestHelper.SetInput(_component, tempPath, 3);
      ComponentTestHelper.ResetInput(_component, 2);
      ComponentTestHelper.ComputeData(_component);
    }

    private static void DisconnectSection() {
      ComponentTestHelper.ResetInput(_component);
      ComponentTestHelper.ComputeData(_component);
    }

    private static void DisconnectFileNamePanel() {
      ComponentTestHelper.ResetInput(_component, 3);
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
      SetLoadAndPath(true);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("either deformation or load can be specified", runtimeMessages[0]);
    }

    [Fact]
    public void ThereWillWarningWhenInputLoadIsNotCorrect() {
      SetWrongLoad();
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeMessages);
    }

    [Fact]
    public void ShouldLaunchProcessWhenEverythingIsSet() {
      SetLoadAndPath();
      ComponentTestHelper.ComputeData(_component);
      var process = _component.OpenAdSecExe();
      if (process == null) {
        return;
      }
      try {
        Assert.Contains("AdSec", process.ProcessName);
      } finally {
        process.Kill();
      }
    }

    [Fact]
    public void ShouldLaunchProcessWhenSectionAndPathIsSet() {
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
      var process = _component.OpenAdSecExe();
      if (process == null) {
        return;
      }

      try {
        Assert.Contains("AdSec", process.ProcessName);
      } finally {
        process.Kill();
      }
    }

    [Fact]
    public void ShouldNotOpenWhenSectionDisconnectsAfterSuccess() {
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
      DisconnectSection();
      var process = _component.OpenAdSecExe();
      Assert.Null(process);
    }

    [Fact]
    public void ShouldNotOpenWhenFileNameDisconnectsAfterSuccess() {
      SetFilePath();
      ComponentTestHelper.ComputeData(_component);
      DisconnectFileNamePanel();
      var process = _component.OpenAdSecExe();
      Assert.Null(process);
    }

    [Fact]
    public void ShouldNotOpenWhenFileNameChangesWithoutSavingAfterSuccess() {
      SetFilePath();
      ChangeFileNameTo(GetValidPath(true));
      ComponentTestHelper.ComputeData(_component);
      var process = _component.OpenAdSecExe();
      Assert.Null(process);
    }

    [Fact]
    public void WrongFilePathWillNotLaunchAdSecProcess() {
      SetWrongFilePath();
      var process = _component.OpenAdSecExe();
      Assert.Null(process);
    }

    [Fact]
    public void NullFilePathWillNotLaunchAdSecProcess() {
      SetNullFilePath();
      var process = _component.OpenAdSecExe();
      Assert.Null(process);
    }

    [Fact]
    public void EmptyFilePathWillNotLaunchAdSecProcess() {
      SetEmptyFilePath();
      var process = _component.OpenAdSecExe();
      Assert.Null(process);
    }

    [Fact]
    public void TryCastToLoadsReturnsTrueWhenInputLoadsAreCorrect() {
      var objectWrappers = CreateLoad();
      var adSecloads = new Dictionary<int, List<object>>();
      bool castSuccessful = AdSecInput.TryCastToLoads(objectWrappers, ref adSecloads, out int path, out int index);
      Assert.True(castSuccessful);
      Assert.Equal(1, path);
      Assert.Equal(1, index);

    }

    [Fact]
    public void TryCastToLoadsReturnsNullWhenInputLoadsAreNull() {
      var objectWrappers = CreateNullLoad();
      var adSecloads = new Dictionary<int, List<object>>();
      bool castSuccessful = AdSecInput.TryCastToLoads(objectWrappers, ref adSecloads, out int path, out int index);
      Assert.False(castSuccessful);
      Assert.Equal(0, path);
      Assert.Equal(0, index);
    }

    [Fact]
    public void OpeningValidModelWillParseJsonStringCorrectly() {
      SetLoadAndPath();
      var sections = AdSecFile.ReadSection(tempPath);
      Assert.Equal(2, sections.Count);
      Assert.Equal("STD R(m) 0.6 0.3", sections[0].Profile.Description());
    }

    [Fact]
    public void JsonStringWillBeEmptyWhenSectionIsNull() {
      string jsonString = AdSecFile.ModelJson(null, new Dictionary<int, List<object>>());
      Assert.Empty(jsonString);
    }

    [Fact]
    public void JsonStringWillBeEmptyWhenSectionIsEmpty() {
      string jsonString = AdSecFile.ModelJson(new List<AdSecSection>(), null);
      Assert.Empty(jsonString);
    }

    [Fact]
    public void CombineJSonStringsWillBeNullIfListIsEmpty() {
      string jsonString = AdSecFile.CombineJSonStrings(new List<string>());
      Assert.Null(jsonString);
    }

    [Fact]
    public void CombineJSonStringsWillBeNullIfListIsNull() {
      string jsonString = AdSecFile.CombineJSonStrings(null);
      Assert.Null(jsonString);
    }

    [Fact]
    public void ModelJsonThrowExceptionWhenRebarAndConcreteMaterialAreNotConsistent() {
      var concreteMaterial = Concrete.AS3600.Edition_2018.MPA40;
      var rebarMaterial = Reinforcement.Steel.IS456.Edition_2000.S415;
      var designCode = AS3600.Edition_2018;
      var loads = new Dictionary<int, List<object>>();
      var section = AdSecUtility.SectionObject(designCode, concreteMaterial, rebarMaterial);
      var exception = Assert.Throws<InvalidOperationException>(() => AdSecFile.ModelJson(new List<AdSecSection> { section }, loads));
      Assert.Contains("section material and rebar grade are not consistent", exception.Message);
    }

    [Fact]
    public void ModelJsonThrowExceptionIfDesignCodeIsNotassigned() {
      var concreteMaterial = Concrete.AS3600.Edition_2018.MPA40;
      var rebarMaterial = Reinforcement.Steel.IS456.Edition_2000.S415;
      var loads = new Dictionary<int, List<object>>();
      var section = AdSecUtility.SectionObject(null, concreteMaterial, rebarMaterial);
      var exception = Assert.Throws<ArgumentException>(() => AdSecFile.ModelJson(new List<AdSecSection> { section }, loads));
      Assert.Contains("design code is null", exception.Message);
    }
  }
}
