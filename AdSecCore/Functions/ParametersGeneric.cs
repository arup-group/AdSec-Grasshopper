using Oasys.AdSec;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Functions {

  public class DoubleParameter : ParameterAttribute<double> { }

  public class DoubleArrayParameter : BaseArrayParameter<double> { }

  public class IntegerArrayParameter : BaseArrayParameter<int> { }

  public class SectionParameter : ParameterAttribute<ISection> { }
  public class PointArrayParameter : BaseArrayParameter<IPoint> { }
  public class PointParameter : ParameterAttribute<IPoint> { }
  public class StringArrayParam : BaseArrayParameter<string> { }
  public class LengthParameter : ParameterAttribute<Length> { }
}
