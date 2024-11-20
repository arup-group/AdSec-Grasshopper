using Oasys.AdSec;
using Oasys.Business;
using Oasys.Profiles;

namespace AdSecCore.Parameters {

  public class DoubleParameter : ParameterAttribute<double> { }

  public class DoubleArrayParameter : BaseArrayParameter<double> { }

  public class IntegerArrayParameter : BaseArrayParameter<int> { }

  public class SectionParameter : ParameterAttribute<ISection> { }
  public class PointArrayParameter : BaseArrayParameter<IPoint> { }
  public class StringArrayParam : BaseArrayParameter<string> { }
}
