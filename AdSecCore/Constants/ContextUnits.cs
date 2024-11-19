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
      set => instance = value;
    }

    private UnitSet CurrentUnits { get; set; } = new DefaultUnitSet();

    public LengthUnit LengthUnitGeometry {
      get => CurrentUnits.LengthUnitGeometry;
      set => CurrentUnits.LengthUnitGeometry = value;
    }
    public ForceUnit ForceUnit {
      get => CurrentUnits.ForceUnit;
      set => CurrentUnits.ForceUnit = value;
    }
    public StrainUnit StrainUnit {
      get => CurrentUnits.StrainUnit;
      set => CurrentUnits.StrainUnit = value;
    }
    public PressureUnit PressureUnit {
      get => CurrentUnits.PressureUnit;
      set => CurrentUnits.PressureUnit = value;
    }

    public void SetDefaultUnits() {
      CurrentUnits = new DefaultUnitSet();
    }
  }

  public class DefaultUnitSet : UnitSet {

    public LengthUnit LengthUnitGeometry { get; set; } = LengthUnit.Meter;
    public ForceUnit ForceUnit { get; set; } = ForceUnit.Kilonewton;
    public StrainUnit StrainUnit { get; set; } = StrainUnit.Ratio;
    public PressureUnit PressureUnit { get; set; } = PressureUnit.Megapascal;
  }

  public interface UnitSet {

    LengthUnit LengthUnitGeometry { get; set; }
    ForceUnit ForceUnit { get; set; }
    StrainUnit StrainUnit { get; set; }
    PressureUnit PressureUnit { get; set; }
  }

}
