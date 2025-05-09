﻿using System.Collections.Generic;
using System.Reflection;

using AdSecCore;
using AdSecCore.Builders;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.GH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  public class AdSecUtility {
    private static readonly IDesignCode designCode = IS456.Edition_2000;

    private AdSecUtility() { }

    public static ISection CreateSTDRectangularSection(IConcrete concreteMaterial = null, IReinforcement rebarMaterial = null) {
      var topRight = new BuilderSingleBar().WithMaterial(rebarMaterial).WithSize(2).AtPosition(Geometry.Position(13, 28)).Build();
      var BottomRight = new BuilderSingleBar().WithMaterial(rebarMaterial).WithSize(2).AtPosition(Geometry.Position(13, -28))
       .Build();
      var topLeft = new BuilderSingleBar().WithMaterial(rebarMaterial).WithSize(2).AtPosition(Geometry.Position(-13, 28)).Build();
      var BottomLeft = new BuilderSingleBar().WithMaterial(rebarMaterial).WithSize(2).AtPosition(Geometry.Position(-13, -28))
       .Build();
      return new SectionBuilder().WithMaterial(concreteMaterial).WithWidth(30).WithHeight(60).CreateRectangularSection()
       .WithReinforcementGroups(new List<IGroup> { topRight, BottomRight, topLeft, BottomLeft, }).Build();
    }

    public static AdSecSection SectionObject() {
      return new AdSecSection(CreateSTDRectangularSection(), designCode, "", "", Plane.WorldXY);
    }

    public static AdSecSection SectionObject(IDesignCode code, IConcrete concreteMaterial, IReinforcement rebarMaterial) {
      return new AdSecSection(CreateSTDRectangularSection(), code, "", "", Plane.WorldXY);
    }

    public static GH_Component AnalyzeComponent() {
      var component = new Analyse();
      component.SetInputParamAt(0, new AdSecSectionGoo(SectionObject()));
      return component;
    }

    public static AdSecSolutionGoo GetResult() {
      return (AdSecSolutionGoo)ComponentTestHelper.GetOutput(AnalyzeComponent());
    }

    public static bool IsBoundingBoxEqual(BoundingBox actual, BoundingBox expected) {
      var comparer = new DoubleComparer(0.001);
      return comparer.Equals(expected.Min.X, actual.Min.X) && comparer.Equals(expected.Min.Y, actual.Min.Y)
        && comparer.Equals(expected.Max.X, actual.Max.X) && comparer.Equals(expected.Max.X, actual.Max.X);
    }

    public static void LoadAdSecAPI() {
      if (AddReferencePriority.AdSecAPI == null) {
        AddReferencePriority.AdSecAPI = Assembly.Load("AdSec_API.dll");
      }
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class AdSecUtilityTest {
    [Fact]
    public void RectangularSectionTest() {
      var section = AdSecUtility.CreateSTDRectangularSection();
      string expectedProfileDescription = "STD R(m) 0.6 0.3";
      string actualProfileDescription = section.Profile.Description();
      Assert.Equal(expectedProfileDescription, actualProfileDescription);
    }

    [Fact]
    public void LoadAdSecAPITest() {
      AdSecUtility.LoadAdSecAPI();
      Assert.NotNull(AddReferencePriority.AdSecAPI);
    }
  }
}
