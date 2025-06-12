using System;
using System.Collections.Generic;

using AdSecGH.Parameters;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public enum RebarLayoutOption {
    Line,
    SingleBars,
    Circle,
    Arc,
  }
  public class RebarLayoutFunction : Function, IVariableInput, IDropdownOptions, ILocalUnits {
    private const string PositiveAngleIsConsideredAntiClockwise = "Positive angle is considered anti-clockwise.";
    public LengthUnit LocalLengthUnit { get; set; } = LengthUnit.Meter;
    public AngleUnit LocalAngleUnit { get; set; } = AngleUnit.Radian;
    private RebarLayoutOption _rebarLayoutOption = RebarLayoutOption.Line;

    public event Action OnVariableInputChanged;

    public RebarLayoutOption RebarLayoutOption {
      get { return _rebarLayoutOption; }
      set {
        if (_rebarLayoutOption == value) {
          return;
        }
        _rebarLayoutOption = value;
        OnVariableInputChanged?.Invoke();
      }
    }

    public RebarLayerParameter SpacedRebars { get; set; } = new RebarLayerParameter {
      Name = "Spaced Rebars",
      NickName = "RbS",
      Description = "AdSec Rebars Spaced in a Layer",
      Optional = false,
    };

    public RebarBundleParameter RebarBundle { get; set; } = new RebarBundleParameter {
      Name = "Rebar",
      NickName = "Rb",
      Description = "AdSec Rebar (single or bundle)",
      Optional = false,
    };

    public PointParameter CentreOfCircle { get; set; } = new PointParameter {
      Name = "Centre",
      NickName = "CVx",
      Description = "Vertex Point representing the centre of the circle",
      Optional = true,
    };

    public DoubleParameter RadiusOfCircle { get; set; } = new DoubleParameter {
      Name = "Radius",
      NickName = "r",
      Description = "Distance representing the radius of the circle",
      Optional = false,
    };

    public DoubleParameter StartAngle { get; set; } = new DoubleParameter {
      Name = "StartAngle",
      NickName = "s°",
      Description = "The starting angle of the circle",
      Optional = true,
    };

    public DoubleParameter SweepAngle { get; set; } = new DoubleParameter {
      Name = "SweepAngle",
      NickName = "e°",
      Description = "The angle sweeped by the arc from its start angle",
      Optional = true,
    };

    public PointArrayParameter Positions { get; set; } = new PointArrayParameter {
      Name = "Position(s)",
      NickName = "Vxs",
      Description = "List of bar positions",
      Optional = false,
    };

    public PointParameter Position1 { get; set; } = new PointParameter {
      Name = "Position 1",
      NickName = "Vx1",
      Description = "First bar position",
      Optional = false,
    };

    public PointParameter Position2 { get; set; } = new PointParameter {
      Name = "Position 2",
      NickName = "Vx2",
      Description = "Last bar position",
      Optional = false,
    };

    public RebarGroupParameter RebarGroup { get; set; } = new RebarGroupParameter {
      Name = "Layout",
      NickName = "RbG",
      Description = "Rebar Group for AdSec Section",
      Optional = false,
    };

    public override Attribute[] GetAllInputAttributes() {
      Attribute[] allInputAttributes = { };
      switch (RebarLayoutOption) {
        case RebarLayoutOption.Line:
          allInputAttributes = new Attribute[] {
            SpacedRebars,
            Position1,
            Position2
          };
          break;
        case RebarLayoutOption.SingleBars:
          allInputAttributes = new Attribute[] {
            RebarBundle,
            Positions
          };
          break;
        case RebarLayoutOption.Circle:
          allInputAttributes = new Attribute[] {
            SpacedRebars,
            CentreOfCircle,
            RadiusOfCircle,
            StartAngle,
          };
          break;
        case RebarLayoutOption.Arc:
          allInputAttributes = new Attribute[] {
            SpacedRebars,
            CentreOfCircle,
            RadiusOfCircle,
            StartAngle,
            SweepAngle
          };
          break;
      }
      return allInputAttributes;
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        RebarGroup
      };
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Reinforcement Layout",
      NickName = "Reinforcement Layout",
      Description = "Create a Reinforcement Layout for an AdSec Section"
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3()
    };

    public override void Compute() {
      IGroup group = null;
      try {
        switch (RebarLayoutOption) {
          case RebarLayoutOption.Line:
            group = CreateLineTypeGroup();
            break;
          case RebarLayoutOption.SingleBars:
            group = CreateSingleBarsTypeGroup();
            break;
          case RebarLayoutOption.Circle:
            group = CreateCircleTypeGroup();
            break;
          case RebarLayoutOption.Arc:
            group = CreateArcTypeGroup();
            break;
        }
      } catch (ArgumentNullException) {
        ErrorMessages.Add("Input value like rebar and position can not be null");
        return;
      } catch (ArgumentOutOfRangeException) {
        ErrorMessages.Add("Radius should be positive value and greater than zero");
        return;
      }
      RebarGroup.Value = new AdSecRebarGroup(group);
    }


    private IGroup CreateLineTypeGroup() {
      return ILineGroup.Create(Position1.Value, Position2.Value, SpacedRebars.Value);
    }

    private IGroup CreateSingleBarsTypeGroup() {
      var points = Oasys.Collections.IList<IPoint>.Create();
      foreach (var position in Positions.Value) {
        points.Add(position);
      }
      var group = ISingleBars.Create(RebarBundle.Value);
      group.Positions = points;
      return group;
    }

    private IGroup CreateArcTypeGroup() {
      var tolerance = Angle.FromRadians(1e-9);
      var sweepAngle = SweepAngleToAngle();
      if (sweepAngle.Equals(Angle.Zero, tolerance)) {
        WarningMessages.Add("Sweep angle is zero, create a circle instead of an arc.");
      }
      return IArcGroup.Create(CentreOfCircle.Value, RadiusToLength(), StartAngleToAngle(), sweepAngle, SpacedRebars.Value);
    }

    private IGroup CreateCircleTypeGroup() {
      return ICircleGroup.Create(CentreOfCircle.Value, RadiusToLength(), StartAngleToAngle(), SpacedRebars.Value);
    }

    private Angle SweepAngleToAngle() {
      return UnitHelpers.ParseToQuantity<Angle>(SweepAngle.Value, AngleUnit);
    }

    private Angle StartAngleToAngle() {
      return UnitHelpers.ParseToQuantity<Angle>(StartAngle.Value, AngleUnit);
    }

    private Length RadiusToLength() {
      return UnitHelpers.ParseToQuantity<Length>(RadiusOfCircle.Value, LengthUnitGeometry);
    }

    protected override void UpdateParameter() {
      string lengthUnitAbbreviation = Length.GetAbbreviation(LengthUnitGeometry);
      string angleUnitAbbreviation = Angle.GetAbbreviation(AngleUnit);
      SweepAngle.Name = $"SweepAngle [{angleUnitAbbreviation}]";
      SweepAngle.Description = $"The angle (in {angleUnitAbbreviation}) sweeped by the arc from its start angle. {PositiveAngleIsConsideredAntiClockwise}";
      StartAngle.Name = $"StartAngle [{angleUnitAbbreviation}]";
      StartAngle.Description = $"The starting angle (in {angleUnitAbbreviation}) of the circle. {PositiveAngleIsConsideredAntiClockwise} Default is 0";
      RadiusOfCircle.Name = $"Radius [{lengthUnitAbbreviation}]";
    }

    public void UpdateUnits() {
      AngleUnit = LocalAngleUnit;
      LengthUnitGeometry = LocalLengthUnit;
    }

    public IOptions[] Options() {
      var options = new List<IOptions> {
        new EnumOptions() {
          EnumType = typeof(RebarLayoutOption),
          Description = "Layout Type",
        }
      };
      switch (RebarLayoutOption) {
        case RebarLayoutOption.Circle:
        case RebarLayoutOption.Arc:
          options.Add(new UnitOptions() {
            Description = "Length measure",
            UnitType = typeof(LengthUnit),
            UnitValue = (int)LengthUnitGeometry,
          });
          options.Add(new UnitOptions() {
            Description = "Angle measure",
            UnitType = typeof(AngleUnit),
            UnitValue = (int)AngleUnit,
          });
          break;
      }
      return options.ToArray();
    }
  }
}
