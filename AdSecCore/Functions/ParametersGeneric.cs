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

  public class IntegerParameter : ParameterAttribute<int> { }
  public class TextParameter : ParameterAttribute<string> { }
  public class SolutionParameter : ParameterAttribute<ISolution> { }
  public class LoadParameter : ParameterAttribute<ILoad> { }
  public class CrackParameter : ParameterAttribute<ICrack> { }

}
