using System;
using System.Collections.Generic;
using System.IO;

using AdSecCore.Constants;
using AdSecCore.Functions;

using AdSecGHCore.Constants;

using Oasys.AdSec.IO.Serialization;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGHCore.Functions {

  public class OpenModelFunction : Function {
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Description = "Open an existing AdSec .ads file",
      Name = "Open Model",
      NickName = "Open",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat0(),
    };

    public PathParameter Path { get; set; } = new PathParameter() {
      Name = "Filename and path",
      NickName = "File",
      Description
        = $"AdSec file to open and work with.{Environment.NewLine}Input either path component, a text string with path and {Environment.NewLine}filename or an existing AdSec File created in Grasshopper.",
    };

    public PlaneParameter Plane { get; set; } = new PlaneParameter() {
      Name = "LocalPlane",
      NickName = "Pln",
      Description = "[Optional] Plane representing local coordinate system, by default a YZ-plane is used",
      Optional = true,
      Default = new[] { OasysPlane.PlaneYZ },
    };
    public SectionArrayParameter Sections { get; set; } = new SectionArrayParameter() {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Sections",
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Path, Plane,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { Sections };
    }

    public override void Compute() {
      string json = File.ReadAllText(Path.Value);
      var jsonParser = JsonParser.Deserialize(json);

      DesignCode designCode = new DesignCode() {
        IDesignCode = AdSecFileHelper.Codes["EC2_04"],
        DesignCodeName = "EC2_04"
      };

      if (AdSecFileHelper.ProcessJsonIntoDesignCodeParts(json, out var designCodeLevelsSplit)) {
        var codeType = AdSecFileHelper.GetDesignCodeType(designCodeLevelsSplit);
        designCode.IDesignCode = AdSecFileHelper.GetDesignCode(designCodeLevelsSplit, codeType, false);
        designCode.DesignCodeName = designCodeLevelsSplit[0];
      }

      var sections = new List<SectionDesign>();

      for (int i = 0; i < jsonParser.Sections.Count; i++) {
        sections.Add(new SectionDesign() {
          Section = jsonParser.Sections[i],
          DesignCode = designCode,
        });
      }

      if (sections.Count == 0) {
        WarningMessages.Add("File contains no valid sections");
      }

      foreach (var warning in jsonParser.Warnings) {
        WarningMessages.Add(warning.Description);
      }

      Sections.Value = sections.ToArray();
    }
  }
}
