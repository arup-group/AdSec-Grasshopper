using System.Collections.Generic;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {

  public class BuilderReinforcementGroup : IBuilder<ISingleBars> {
    private readonly List<IPoint> positions = new List<IPoint>();
    private ISingleBars group;
    private GroupType groupType;
    private readonly IReinforcement material = Reinforcement.Steel.IS456.Edition_2000.S415;
    private double rebarDiameter = 2;

    public ISingleBars Build() {
      switch (groupType) {
        case GroupType.SingleBar:
          var singleBars = ISingleBars.Create(IBarBundle.Create(material, Length.FromCentimeters(rebarDiameter), 1));
          foreach (var position in positions) {
            singleBars.Positions.Add(position);
          }

          group = singleBars;
          break;
      }

      return group;
    }

    public BuilderReinforcementGroup AtPosition(IPoint position) {
      positions.Add(position);
      return this;
    }

    public BuilderReinforcementGroup WithSize(double size) {
      rebarDiameter = size;
      return this;
    }

    public BuilderReinforcementGroup CreateSingleBar() {
      groupType = GroupType.SingleBar;
      return this;
    }

    internal enum GroupType {
      SingleBar,
    }
  }
}
