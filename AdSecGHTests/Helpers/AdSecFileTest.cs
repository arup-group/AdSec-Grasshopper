using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using AdSecCore.Constants;

using AdSecGH;
using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecFileTest {
    public AdSecFileTest() {
      //workaround for loading API dll
      AddReferencePriority.AdSecAPI
        = Assembly.LoadFile(Path.GetFullPath($"{Environment.CurrentDirectory}//AdSec_API.dll"));
    }

    private static string CreateSampleJson(string codeName, bool valid = true) {
      return valid ?
        $"something before codes \n\n \"codes\": {{\r\n        \"concrete\": \"{codeName}\"\n    }},\n    \n something after" :
        $" \"codes\": {{\"concrete\": \"{codeName}";
    }

    [Fact]
    public void GetDesignCode_ForExistingCodesInAdSecFile_Test() {
      foreach (string key in AdSecFileHelper.Codes.Keys) {
        string json = CreateSampleJson(key);
        var code = AdSecFile.GetDesignCode(json);

        Assert.True(AdSecFileHelper.Codes.ContainsValue(code.DesignCode));
        Assert.True(AdSecFileHelper.CodesStrings.ContainsValue(code.DesignCodeName.Replace(" ", "+")));
      }
    }

    [Fact]
    public void GetDesignCode_ForInvalidCode_Test() {
      string json = CreateSampleJson("I'm invalid design code!");

      var code = AdSecFile.GetDesignCode(json);

      Assert.Null(code);
    }

    [Fact]
    public void GetDesignCode_ForInvalidJson_Test() {
      string json = CreateSampleJson(AdSecFileHelper.Codes.Keys.FirstOrDefault(), false);

      var code = AdSecFile.GetDesignCode(json);

      Assert.Null(code);
    }
  }
}
