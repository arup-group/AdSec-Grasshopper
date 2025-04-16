using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

using Grasshopper;

using Oasys.AdSec;

namespace AdSecGH.UI {
  [SuppressMessage("Style", "IDE1006:Naming Styles",
    Justification = "Naming convention exception for this file")]
  internal partial class AboutBox : Form {
    public static string AssemblyCompany => "Copyright © Oasys 1985 - 2024";
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string packagePath = @"rhino://package/search?name=adsec";
#pragma warning restore S1075 // URIs should not be hardcoded

    private const string productName = "AdSecGH";
    private const string productLabelText = "AdSec Grasshopper Plugin";
    private const string oasysHtml = @"www.oasys-software.com";
    private const string oasysMail = @"oasys@arup.com";
    private const string supportText = "Contact and support:";
    private readonly string title = $"About {productName}";

    public AboutBox() {
      var adsecPlugin = Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));

      string versionText = $"Version {adsecPlugin.Version}";
      string apiVersionText = $"API Version {IVersion.Api()}";

      InitializeComponent();
      Text = title;
      SetLabelsText(versionText, apiVersionText);
    }

    private void SetLabelsText(string versionText, string apiVersionText) {
      labelProductName.Text = productLabelText;
      labelVersion.Text = versionText;
      labelApiVersion.Text = apiVersionText;
      labelCompanyName.Text = AssemblyCompany;
      linkWebsite.Text = oasysHtml;
      labelContact.Text = supportText;
      linkEmail.Text = oasysMail;
    }

    private void button1_Click(object sender, EventArgs e) {
      Process.Start(packagePath);
    }

    private void linkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
      var adsecPlugin = Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));
      string pluginvers = adsecPlugin.Version;
      Process.Start($@"mailto:oasys@arup.com?subject=Oasys AdSecGH version {pluginvers}");
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
      Process.Start($@"https://{oasysHtml}");
    }

    private void okButton_Click(object sender, EventArgs e) {
      Close();
    }
  }
}
