
using System;
using System.Collections.Generic;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public interface IFunction {

    FuncAttribute Metadata { get; set; }
    Organisation Organisation { get; set; }
    Attribute[] GetAllInputAttributes();

    Attribute[] GetAllOutputAttributes();

    void Compute();
  }

  public abstract class Function : IFunction {
    private AngleUnit _angleUnit = AngleUnit.Radian;
    private LengthUnit _lengthUnitGeometry = LengthUnit.Meter;
    private LengthUnit _lengthUnitResult = LengthUnit.Millimeter;
    private StrainUnit _strainUnitResult = StrainUnit.Ratio;
    private StrainUnit _materialStrainUnit = StrainUnit.Ratio;
    private CurvatureUnit _curvatureUnit = CurvatureUnit.PerMeter;
    private MomentUnit _momentUnit = MomentUnit.NewtonMeter;
    private AxialStiffnessUnit _axialStiffnessUnit = AxialStiffnessUnit.Newton;
    private BendingStiffnessUnit _bendingStiffnessUnit = BendingStiffnessUnit.NewtonSquareMeter;
    private PressureUnit _stressUnitResult = PressureUnit.Megapascal;
    private ForceUnit _forceUnit = ForceUnit.Newton;

    public AngleUnit AngleUnit {
      get => _angleUnit;
      set {
        _angleUnit = value;
        UpdateParameter();
      }
    }
    public LengthUnit LengthUnitGeometry {
      get => _lengthUnitGeometry;
      set {
        _lengthUnitGeometry = value;
        UpdateParameter();
      }
    }

    public LengthUnit LengthUnitResult {
      get => _lengthUnitResult;
      set {
        _lengthUnitResult = value;
        UpdateParameter();
      }
    }

    public StrainUnit StrainUnitResult {
      get => _strainUnitResult;
      set {
        _strainUnitResult = value;
        UpdateParameter();
      }
    }

    public StrainUnit MaterialStrainUnit {
      get => _materialStrainUnit;
      set {
        _materialStrainUnit = value;
        UpdateParameter();
      }
    }

    public CurvatureUnit CurvatureUnit {
      get => _curvatureUnit;
      set {
        _curvatureUnit = value;
        UpdateParameter();
      }
    }

    public MomentUnit MomentUnit {
      get => _momentUnit;
      set {
        _momentUnit = value;
        UpdateParameter();
      }
    }

    public AxialStiffnessUnit AxialStiffnessUnit {
      get => _axialStiffnessUnit;
      set {
        _axialStiffnessUnit = value;
        UpdateParameter();
      }
    }

    public BendingStiffnessUnit BendingStiffnessUnit {
      get => _bendingStiffnessUnit;
      set {
        _bendingStiffnessUnit = value;
        UpdateParameter();
      }
    }

    public PressureUnit StressUnitResult {
      get => _stressUnitResult;
      set {
        _stressUnitResult = value;
        UpdateParameter();
      }
    }

    public ForceUnit ForceUnit {
      get => _forceUnit;
      set {
        _forceUnit = value;
        UpdateParameter();
      }
    }

    public List<string> ErrorMessages { get; set; } = new List<string>();
    public List<string> WarningMessages { get; set; } = new List<string>();
    public List<string> RemarkMessages { get; set; } = new List<string>();

    public virtual bool ValidateInputs() {
      var inputs = GetAllInputAttributes();
      foreach (var input in inputs) {
        var valueProperty = input.GetType().GetProperty("Value");
        var value = valueProperty.GetValue(input);
        if (!input.Optional && value == null) {
          ErrorMessages.Add($"{input.Name} input is null");
          return false;
        }
      }

      return true;
    }

    public void ClearMessages() {
      ErrorMessages.Clear();
      WarningMessages.Clear();
      RemarkMessages.Clear();
    }

    public void ClearInputs() {
      foreach (var input in GetAllInputAttributes()) {
        if (input is IDefault inputAttribute) {
          inputAttribute.SetDefault();
        }
      }
    }

    public void ClearOutputs() {
      foreach (var output in GetAllOutputAttributes()) {
        if (output is IDefault outputAttribute) {
          outputAttribute.SetDefault();
        }
      }
    }

    public abstract FuncAttribute Metadata { get; set; }
    public abstract Organisation Organisation { get; set; }
    public virtual Attribute[] GetAllInputAttributes() { return Array.Empty<Attribute>(); }

    public virtual Attribute[] GetAllOutputAttributes() { return Array.Empty<Attribute>(); }

    protected virtual void UpdateParameter() { }

    public abstract void Compute();
  }

  public interface ILocalUnits {
    void UpdateUnits();
  }

  public class FuncAttribute {
    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
  }

  public class Organisation {
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public bool Hidden { get; set; }
  }

  public class Attribute {

    public string Name { get; set; }
    public string NickName { get; set; }
    public string Description { get; set; }
    public bool Optional { get; set; } = false;

    public void Update(ref Attribute update) {
      update.Name = Name;
      update.NickName = NickName;
      update.Description = Description;
      update.Optional = Optional;

      if (this is IAccessible accessible && update is IAccessible AdSecSection) {
        AdSecSection.Access = accessible.Access;
      }
    }
  }

  public enum Access {
    Item,
    List,
  }

  public interface IDefault {
    void SetDefault();
  }

  public interface IAccessible {
    Access Access { get; set; }
  }

  public class ParameterAttribute<T> : Attribute, IDefault, IAccessible {
    private T _value;
    public T Value {
      get {
        var defValue = default(T);
        if (Equals(_value, defValue) && !Equals(Default, defValue)) {
          _value = Default;
        }

        return _value;
      }
      set {
        _value = value;
        OnValueChanged?.Invoke(value);
      }
    }
    public virtual Access Access { get; set; } = Access.Item;
    public T Default { get; set; }

    public void SetDefault() {
      Value = Default;
    }

    public event Action<T> OnValueChanged;
  }

  public class BaseArrayParameter<T> : ParameterAttribute<T[]> {
    public override Access Access { get; set; } = Access.List;
  }

}
