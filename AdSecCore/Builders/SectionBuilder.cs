using System.Collections.Generic;

using Oasys.AdSec;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {
  public class SectionBuilder : IBuilder<ISection> {

    private readonly List<IGroup> ReinforcementGroups = new List<IGroup>();
    private readonly IConcrete material = Concrete.IS456.Edition_2000.M10;
    private ISection section;
    private SectionType sectionType;
    private double _width { get; set; }
    private double _height { get; set; }

    public ISection Build() {
      switch (sectionType) {
        case SectionType.Square:
          section = ISection.Create(
            IRectangleProfile.Create(Length.FromCentimeters(_width), Length.FromCentimeters(_width)), material);
          break;
        case SectionType.Rectangular:
          section = ISection.Create(
            IRectangleProfile.Create(Length.FromCentimeters(_height), Length.FromCentimeters(_width)), material);
          break;
      }

      if (ReinforcementGroups.Count > 0) {
        foreach (var group in ReinforcementGroups) {
          section.ReinforcementGroups.Add(group);
        }
      }

      return section;
    }

    /// <summary>
    /// Creates an invalid section for testing purposes
    /// Given a Perimeter profile with a void polygon outside the solid polygon
    /// </summary>
    /// <returns></returns>
    public static ISection InvalidSection(int size = 100) {
      // Important to pick a material that has the checks for warnings.
      IMaterial concreteMaterial = Concrete.IS456.Edition_2000.M10;

      var invalid = IPerimeterProfile.Create();
      invalid.SolidPolygon = CreateRectangle(0, size);
      invalid.VoidPolygons.Add(CreateRectangle(-1, 2));

      return ISection.Create(invalid, concreteMaterial);
    }

    public static IPolygon CreateRectangle(int @base, int size) {
      var voidOutside = IPolygon.Create();
      voidOutside.Points.Add(IPoint.Create(Length.FromMillimeters(@base), Length.FromMillimeters(@base)));
      voidOutside.Points.Add(IPoint.Create(Length.FromMillimeters(@base), Length.FromMillimeters(size)));
      voidOutside.Points.Add(IPoint.Create(Length.FromMillimeters(size), Length.FromMillimeters(size)));
      voidOutside.Points.Add(IPoint.Create(Length.FromMillimeters(size), Length.FromMillimeters(@base)));
      return voidOutside;
    }

    public SectionBuilder CreateRectangularSection() {
      sectionType = SectionType.Rectangular;
      return this;
    }

    public SectionBuilder CreateSquareSection() {
      sectionType = SectionType.Square;
      return this;
    }

    public SectionBuilder WithWidth(double width) {
      _width = width;
      return this;
    }

    public SectionBuilder WithHeight(double height) {
      _height = height;
      return this;
    }

    public SectionBuilder WithReinforcementGroup(IGroup group) {
      ReinforcementGroups.Add(group);
      return this;
    }

    public SectionBuilder WithReinforcementGroups(List<IGroup> groups) {
      foreach (var group in groups) {
        ReinforcementGroups.Add(group);
      }

      return this;
    }

    internal enum SectionType {
      Square,
      Rectangular,
    }
  }
}
