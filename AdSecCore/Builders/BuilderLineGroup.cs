using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {
  public class BuilderLayer : IBuilder<ILayer> {
    private static readonly IReinforcement material = Reinforcement.Steel.IS456.Edition_2000.S415;
    private static readonly double rebarDiameter = 2;
    private readonly IBarBundle _barBundle = IBarBundle.Create(material, Length.FromCentimeters(rebarDiameter));
    private readonly int _count = 2;

    public ILayer Build() {
      return ILayerByBarCount.Create(_count, _barBundle);
    }
  }

  public class BuilderLineGroup : IBuilder<ILineGroup> {
    private readonly IPoint _firstBarPosition = Geometry.Position(3, 3);
    private readonly IPoint _lastBarPosition = Geometry.Position(3, 10);
    private readonly ILayer _layer = new BuilderLayer().Build();

    public ILineGroup Build() {
      return ILineGroup.Create(_firstBarPosition, _lastBarPosition, _layer);
    }
  }

  public class BuilderTopTemplateGroup : IBuilder<ITemplateGroup> {
    public ITemplateGroup Build() {
      var templateGroup = ITemplateGroup.Create(ITemplateGroup.Face.Top);
      templateGroup.Layers.Add(new BuilderLayer().Build());
      return templateGroup;
    }
  }

}
