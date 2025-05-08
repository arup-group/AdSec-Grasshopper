using System;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysUnits;

namespace AdSecCore.Functions {
  public class CreatePreLoadFunction : Function {
    public RebarGroupParameter RebarGroupInput { get; set; } = new RebarGroupParameter {
      Name = "RebarGroup",
      NickName = "RbG",
      Description = "AdSec Reinforcement Group to apply Preload to",
      Access = Access.Item,
    };

    public GenericParameter PreloadInput { get; set; } = new GenericParameter {
      Name = "Preload",
      NickName = "PL",
      Description = "The preload value (Force, Strain, or Stress) to apply to the reinforcement group",
      Access = Access.Item,
    };

    public RebarGroupParameter PreloadedRebarGroupOutput { get; set; } = new RebarGroupParameter {
      Name = "Prestressed RebarGroup",
      NickName = "RbG",
      Description = "Preloaded Rebar Group for AdSec Section",
      Access = Access.Item,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        RebarGroupInput,
        PreloadInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        PreloadedRebarGroupOutput,
      };
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Prestress Load",
      NickName = "Prestress",
      Description = "Create an AdSec Prestress Load for Reinforcement Layout as either Preforce, Prestrain, or Prestress",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat5(),
    };


    public override void Compute() {
      // Get the rebar group
      var rebarGroup = RebarGroupInput.Value;
      var outRebarGroup = rebarGroup;
      if (outRebarGroup == null) {
        throw new ArgumentException("Invalid RebarGroup input.");
      }

      // Determine the preload type and create the appropriate preload
      IPreload preload = null;
      if (PreloadInput.Value is Force force) {
        preload = IPreForce.Create(force);
      } else if (PreloadInput.Value is Strain strain) {
        preload = IPreStrain.Create(strain);
      } else if (PreloadInput.Value is Pressure stress) {
        preload = IPreStress.Create(stress);
      } else {
        throw new ArgumentException("Invalid Preload input type.");
      }

      // Apply the preload to the rebar group
      var longitudinalGroup = outRebarGroup.Group as ILongitudinalGroup;
      if (longitudinalGroup == null) {
        throw new ArgumentException("RebarGroup must be a longitudinal group.");
      }

      longitudinalGroup.Preload = preload;

      // Set the output
      PreloadedRebarGroupOutput.Value = outRebarGroup;
    }
  }
}
