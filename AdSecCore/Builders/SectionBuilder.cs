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
    private readonly IConcrete defaultMaterial = Concrete.IS456.Edition_2000.M10;
    private ISection section;
    private SectionType sectionType;
    private IProfile _profile;
    private IMaterial _material;
    private double _width { get; set; }
    private double _depth { get; set; }

    public static ISection Get100Section() {
      var section = new SectionBuilder().CreateRectangularSection().WithHeight(100).WithWidth(100).Build();
      return section;
    }

    public SectionBuilder WithMaterial(IMaterial material) {
      _material = material;
      return this;
    }

    public ISection Build() {
      var profileBuilder = new ProfileBuilder();
      var profile = GetProfile(profileBuilder);
      var material = _material ?? defaultMaterial;

      section = ISection.Create(profile, material);
      if (ReinforcementGroups.Count > 0) {
        foreach (var group in ReinforcementGroups) {
          section.ReinforcementGroups.Add(group);
        }
      }

      return section;
    }

    private IProfile GetProfile(ProfileBuilder profileBuilder) {
      if (_profile != null) {
        return _profile;
      }

      IProfile profile = null;
      switch (sectionType) {
        case SectionType.Square:
          profile = profileBuilder.WidthDepth(_width).WithWidth(_width).Build();
          break;
        case SectionType.Rectangular:
          profile = profileBuilder.WidthDepth(_depth).WithWidth(_width).Build();
          break;
      }

      return profile;
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
      _depth = height;
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

    public void SetProfile(IProfile profile) { _profile = profile; }
  }
}
