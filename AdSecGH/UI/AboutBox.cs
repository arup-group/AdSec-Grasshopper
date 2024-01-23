using Grasshopper.Kernel;
using Oasys.AdSec;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace AdSecGH.UI {
  internal partial class AboutBox : Form {
    public string AssemblyCompany {
      get {
        return "Copyright © Oasys 1985 - 2024";
      }
    }

    public string AssemblyDescription {
      get {
        return "AdSec demonstrates how sections respond to combined axial load and bending moments. It can analyse concrete, steel and fibre-reinforced polymer (FRP) sections and compound sections.";
      }
    }

    public string AssemblyTitle {
      get {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length > 0) {
          AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
          if (titleAttribute.Title != "") {
            return titleAttribute.Title;
          }
        }
        return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    public AboutBox() {
      GH_AssemblyInfo adsecPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));

      string api = IVersion.Api();
      string pluginvers = adsecPlugin.Version;
      string pluginloc = adsecPlugin.Location;

      InitializeComponent();
      Text = String.Format("About {0}", "AdSecGH");
      labelProductName.Text = "AdSec Grasshopper Plugin";
      labelVersion.Text = String.Format("Version {0}", pluginvers);
      labelApiVersion.Text = String.Format("API Version {0}", api);
      labelCompanyName.Text = AssemblyCompany;
      linkWebsite.Text = @"www.oasys-software.com";
      labelContact.Text = "Contact and support:";
      linkEmail.Text = @"oasys@arup.com";
    }

    private void AboutBox_Load(object sender, EventArgs e) {
    }

    private void button1_Click(object sender, EventArgs e) {
      Process.Start(@"rhino://package/search?name=adsec");
    }

    private void labelApiVersion_Click(object sender, EventArgs e) {
    }

    private void labelProductName_Click(object sender, EventArgs e) {
    }

    private void labelVersion_Click(object sender, EventArgs e) {
    }

    private void linkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
      GH_AssemblyInfo adsecPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));
      string pluginvers = adsecPlugin.Version;
      Process.Start(@"mailto:oasys@arup.com?subject=Oasys AdSecGH version " + pluginvers);
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
      Process.Start(@"https://www.oasys-software.com/");
    }

    private void okButton_Click(object sender, EventArgs e) {
      Close();
    }
  }
}
