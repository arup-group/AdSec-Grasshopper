using OasysUnits.Units;

namespace AdSecGHCore.Constants {
  // A Singleton That Holds all the LengthUnit
  public class ContextUnits {

    private static ContextUnits instance;
    public static ContextUnits Instance {
      get {
        if (instance == null) {
          instance = new ContextUnits();
        }

        return instance;
      }
    }

    private IUnitSet CurrentUnits { get; set; } = new DefaultUnitSet();

    public LengthUnit LengthUnitGeometry {
      get => CurrentUnits.LengthUnitGeometry;
      set => CurrentUnits.LengthUnitGeometry = value;
    }
    public ForceUnit ForceUnit => CurrentUnits.ForceUnit;
    public StrainUnit StrainUnit => CurrentUnits.StrainUnit;
    public PressureUnit PressureUnit => CurrentUnits.PressureUnit;

    public void SetDefaultUnits() {
      CurrentUnits = new DefaultUnitSet();
    }
  }

  public class DefaultUnitSet : IUnitSet {

    public LengthUnit LengthUnitGeometry { get; set; } = LengthUnit.Meter;
    public ForceUnit ForceUnit { get; set; } = ForceUnit.Kilonewton;
    public StrainUnit StrainUnit { get; set; } = StrainUnit.Ratio;
    public PressureUnit PressureUnit { get; set; } = PressureUnit.Megapascal;
  }

  public interface IUnitSet {

    LengthUnit LengthUnitGeometry { get; set; }
    ForceUnit ForceUnit { get; set; }
    StrainUnit StrainUnit { get; set; }
    PressureUnit PressureUnit { get; set; }
  }

}
