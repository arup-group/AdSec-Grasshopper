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
    private IConcrete material = Concrete.IS456.Edition_2000.M10;
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

    public SectionBuilder WithMaterial(IConcrete material) {
      this.material = material;
      return this;
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
