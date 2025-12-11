using System.Collections.Generic;

using Oasys.AdSec;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {

  public class SectionBuilder : IBuilder<ISection> {
    public readonly IConcrete defaultMaterial = Concrete.IS456.Edition_2000.M10;

    private readonly List<IGroup> ReinforcementGroups = new List<IGroup>();
    private ICover _cover;
    private IMaterial _material;
    private IProfile _profile;
    private List<ISubComponent> _subComponents = new List<ISubComponent>();
    private SectionType sectionType;
    private double Width { get; set; }
    private double Depth { get; set; }

    public ISection Build() {
      var profileBuilder = new ProfileBuilder();
      var profile = GetProfile(profileBuilder);
      var material = _material ?? defaultMaterial;

      var section = ISection.Create(profile, material);
      foreach (var subComponents in _subComponents) {
        section.SubComponents.Add(subComponents);
      }

      if (ReinforcementGroups.Count > 0) {
        foreach (var group in ReinforcementGroups) {
          section.ReinforcementGroups.Add(group);
        }
      }

      if (_cover != null) {
        section.Cover = _cover;
      }

      return section;
    }

    public SectionBuilder WithCover(ICover cover) {
      _cover = cover;
      return this;
    }

    public static ISection Get100Section() {
      var section = new SectionBuilder().CreateRectangularSection().WithHeight(100).WithWidth(100).Build();
      return section;
    }

    public SectionBuilder WithMaterial(IMaterial material) {
      _material = material;
      return this;
    }

    private IProfile GetProfile(ProfileBuilder profileBuilder) {
      if (_profile != null) {
        return _profile;
      }

      IProfile profile = null;
      switch (sectionType) {
        case SectionType.Square:
          profile = profileBuilder.WidthDepth(Width).WithWidth(Width).Build();
          break;
        case SectionType.Rectangular:
          profile = profileBuilder.WidthDepth(Depth).WithWidth(Width).Build();
          break;
        case SectionType.Perimeter:
          profile = SimplePerimeterProfile(Width, Depth);
          break;
      }

      return profile;
    }

    public static IProfile SimplePerimeterProfile(double width, double depth) {
      var perimeterBuilder = new PerimeterBuilder();
      // Create Around 0,0
      double halfWidth = width / 2;
      double halfDepth = depth / 2;
      perimeterBuilder = perimeterBuilder.WithPoint(IPointBuilder.InMillimeters(-halfWidth, -halfDepth))
       .WithPoint(IPointBuilder.InMillimeters(-halfWidth, halfDepth))
       .WithPoint(IPointBuilder.InMillimeters(halfWidth, halfDepth))
       .WithPoint(IPointBuilder.InMillimeters(halfWidth, -halfDepth));
      return perimeterBuilder.Build();
    }

    /// <summary>
    ///   Creates an invalid section for testing purposes
    ///   Given a Perimeter profile with a void polygon outside the solid polygon
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

    public SectionBuilder CreatePerimeterSection() {
      sectionType = SectionType.Perimeter;
      return this;
    }

    public SectionBuilder CreateSquareSection() {
      sectionType = SectionType.Square;
      return this;
    }

    public SectionBuilder WithWidth(double width) {
      Width = width;
      return this;
    }

    public SectionBuilder WithHeight(double height) {
      Depth = height;
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

    public SectionBuilder WithProfile(IProfile profile) {
      _profile = profile;
      return this;
    }

    internal enum SectionType {
      Square,
      Rectangular,
      Perimeter,
    }

    public SectionBuilder WithSubComponents(List<ISubComponent> subComponents) {
      _subComponents = subComponents;
      return this;
    }
  }
}
