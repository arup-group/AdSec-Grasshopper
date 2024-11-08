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

    private UnitSet CurrentUnits { get; set; }

    public LengthUnit LengthUnitGeometry {
      get => CurrentUnits.LengthUnitGeometry;
      set => CurrentUnits.LengthUnitGeometry = value;
    }

    public void SetDefaultUnits() {
      CurrentUnits = new DefaultUnitSet();
    }
  }

  public class DefaultUnitSet : UnitSet {

    public LengthUnit LengthUnitGeometry { get; set; } = LengthUnit.Meter;
  }

  public interface UnitSet {

    LengthUnit LengthUnitGeometry { get; set; }
  }

}
