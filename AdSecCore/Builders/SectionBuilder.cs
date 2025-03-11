using System;
using System.Collections.Generic;

using AdSecGH.Parameters;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Builders {

  public class SectionBuilder : IBuilder<ISection> {
    public readonly IConcrete defaultMaterial = Concrete.IS456.Edition_2000.M10;

    private readonly List<IGroup> ReinforcementGroups = new List<IGroup>();
    private IMaterial _material;
    private IProfile _profile;
    private ISection section;
    private SectionType sectionType;
    private double _width { get; set; }
    private double _depth { get; set; }

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
          profile = profileBuilder.WidthDepth(_width).WithWidth(_width).Build();
          break;
        case SectionType.Rectangular:
          profile = profileBuilder.WidthDepth(_depth).WithWidth(_width).Build();
          break;
        case SectionType.Perimeter:
          var perimeterBuilder = new PerimeterBuilder();
          // Create Around 0,0
          double halfWidth = _width / 2;
          double halfDepth = _depth / 2;
          perimeterBuilder = perimeterBuilder.WithPoint(IPointBuilder.InMillimeters(-halfWidth, -halfDepth))
           .WithPoint(IPointBuilder.InMillimeters(-halfWidth, halfDepth))
           .WithPoint(IPointBuilder.InMillimeters(halfWidth, halfDepth))
           .WithPoint(IPointBuilder.InMillimeters(halfWidth, -halfDepth));
          profile = perimeterBuilder.Build();

          break;
      }

      return profile;
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

    public void SetProfile(IProfile profile) { _profile = profile; }

    // public static List<AdSecRebarGroup> CalibrateReinforcementGroupsForIPerimeterProfile(
    //   List<AdSecRebarGroup> reinforcements, IDesignCode designCodeDesignCode, IProfile profileProfile,
    //   IMaterial material) {
    // }

    public static List<AdSecRebarGroup> CalibrateReinforcementGroupsForSection(
      List<AdSecRebarGroup> reinforcements, IDesignCode designCode, ISection sectionSection) {
      var adSec = IAdSec.Create(designCode);
      var flattened = adSec.Flatten(sectionSection);

      string description = sectionSection.Profile.Description();
      string[] coordinates1 = description.Remove(0, 11).Split(new[] { ") L(", }, StringSplitOptions.None);
      double maxY1 = double.MinValue;
      double maxZ1 = double.MinValue;
      foreach (string c in coordinates1) {
        string[] value = c.Split('|');
        double y1 = double.Parse(value[0]);
        double z1 = double.Parse(value[1].Remove(value[1].Length - 2));

        if (y1 > maxY1) {
          maxY1 = y1;
        }

        if (z1 > maxZ1) {
          maxZ1 = z1;
        }
      }

      string[] coordinates2 = flattened.Profile.Description().Remove(0, 11).Split(new[] {
        ") L(",
      }, StringSplitOptions.None);
      double maxY2 = double.MinValue;
      double maxZ2 = double.MinValue;
      foreach (string c in coordinates2) {
        string[] value = c.Split('|');
        double y2 = double.Parse(value[0]);
        double z2 = double.Parse(value[1].Remove(value[1].Length - 2));

        if (y2 > maxY2) {
          maxY2 = y2;
        }

        if (z2 > maxZ2) {
          maxZ2 = z2;
        }
      }

      double deltaY = maxY2 - maxY1;
      double deltaZ = maxZ2 - maxZ1;

      var updatedReinforcement = new List<AdSecRebarGroup>();
      foreach (var group in reinforcements) {
        var duplicate = new AdSecRebarGroup();
        if (group.Cover != null) {
          duplicate.Cover = ICover.Create(group.Cover.UniformCover);
        }

        updatedReinforcement.Add(duplicate);

        switch (group.Group) {
          case ISingleBars bars:
            var bundle = IBarBundle.Create(bars.BarBundle.Material, bars.BarBundle.Diameter,
              bars.BarBundle.CountPerBundle);
            var singleBars = ISingleBars.Create(bundle);

            foreach (var point in bars.Positions) {
              var p = IPoint.Create(new Length(point.Y.As(LengthUnit.Meter) - deltaY, LengthUnit.Meter),
                new Length(point.Z.As(LengthUnit.Meter) - deltaZ, LengthUnit.Meter));
              singleBars.Positions.Add(p);
            }

            duplicate.Group = singleBars;
            break;

          default:
            duplicate.Group = group.Group;
            break;
        }
      }

      return updatedReinforcement;
    }

    internal enum SectionType {
      Square,
      Rectangular,
      Perimeter,
    }
  }
}
