using System.Collections.Generic;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {

  public class BuilderSingleBar : IBuilder<ISingleBars> {
    private readonly IReinforcement defaultMaterial = Reinforcement.Steel.IS456.Edition_2000.S415;
    private readonly List<IPoint> positions = new List<IPoint>();
    private IReinforcement material;
    private ISingleBars group;
    private GroupType groupType;
    private double rebarDiameter = 2;

    public ISingleBars Build() {
      var barBundle = IBarBundle.Create(material ?? defaultMaterial, Length.FromCentimeters(rebarDiameter), 1);
      switch (groupType) {
        case GroupType.SingleBar:
          var singleBars = ISingleBars.Create(barBundle);
          foreach (var position in positions) {
            singleBars.Positions.Add(position);
          }

          group = singleBars;
          break;
      }

      return group;
    }

    public BuilderSingleBar AtPosition(IPoint position) {
      positions.Add(position);
      return this;
    }

    public BuilderSingleBar WithSize(double size) {
      rebarDiameter = size;
      return this;
    }

    public BuilderSingleBar WithMaterial(IReinforcement rebarMaterial) {
      material = rebarMaterial;
      return this;
    }

    public BuilderSingleBar CreateSingleBar() {
      groupType = GroupType.SingleBar;
      return this;
    }

    internal enum GroupType {
      SingleBar,
    }
  }
}
