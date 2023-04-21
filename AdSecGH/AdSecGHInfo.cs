using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace AdSecGH {

  public class AdSecGHInfo : GH_AssemblyInfo {
    public override Bitmap AssemblyIcon => Icon;
    public override string AuthorContact
      =>
        //Return a string representing your preferred contact details.
        Contact;
    public override string AuthorName
      =>
        //Return a string identifying you or your company.
        Company;
    public override string Description
      =>
        //Return a short string describing the purpose of this GHA library.
        "Official Oasys AdSec Grasshopper Plugin" + Environment.NewLine + (isBeta ? Disclaimer : "")
        + Environment.NewLine + "The plugin requires an AdSec 10 license to load. "
        + Environment.NewLine + "Contact oasys@arup.com to request a free trial version. "
        + Environment.NewLine + TermsConditions + Environment.NewLine + Copyright;
    public override Bitmap Icon
      =>
        //Return a 24x24 pixel bitmap to represent this GHA library.
        null;
    public string IconUrl
      => "https://raw.githubusercontent.com/arup-group/GSA-Grasshopper/main/Documentation/GettingStartedGuide/Icons/GsaGhLogo.jpg";
    public override Guid Id => GUID;
    public override string Name => ProductName;
    public override string Version {
      get {
        if (isBeta) {
          return Vers + "-beta";
        } else {
          return Vers;
        }
      }
    }
    internal const string Company = "Oasys";
    internal const string Contact = "https://www.oasys-software.com/";
    internal const string Copyright = "Copyright © Oasys 1985 - 2022";
    internal const string PluginName = "AdSecGH";
    internal const string ProductName = "AdSec";
    internal const string TermsConditions
      = "Oasys terms and conditions apply. See https://www.oasys-software.com/terms-conditions for details. ";
    internal const string Vers = "0.9.17";
    internal static string Disclaimer = PluginName
      + " is pre-release and under active development, including further testing to be undertaken. It is provided \"as-is\" and you bear the risk of using it. Future versions may contain breaking changes. Any files, results, or other types of output information created using "
      + PluginName + " should not be relied upon without thorough and independent checking. ";
    internal static Guid GUID = new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9");
    internal static bool isBeta = true;
  }
}
