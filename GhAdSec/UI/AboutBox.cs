using Grasshopper.Kernel;
using Oasys.AdSec;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdSecGH.UI
{
  partial class AboutBox : Form
  {
    public AboutBox()
    {
      GH_AssemblyInfo adsecPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));

      string api = IVersion.Api();
      string pluginvers = adsecPlugin.Version;
      string pluginloc = adsecPlugin.Location;

      InitializeComponent();
      this.Text = String.Format("About {0}", "AdSecGH");
      this.labelProductName.Text = "AdSec Grasshopper Plugin";
      this.labelVersion.Text = String.Format("Version {0}", pluginvers);
      this.labelApiVersion.Text = String.Format("API Version {0}", api);
      this.labelCompanyName.Text = AssemblyCompany;
      this.linkWebsite.Text = @"www.oasys-software.com";
      this.labelContact.Text = "Contact and support:";
      this.linkEmail.Text = @"oasys@arup.com";
      this.disclaimer.Text = AdSecGHInfo.Disclaimer;
    }

    #region Assembly Attribute Accessors

    public string AssemblyTitle
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length > 0)
        {
          AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
          if (titleAttribute.Title != "")
          {
            return titleAttribute.Title;
          }
        }
        return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    public string AssemblyDescription
    {
      get
      {
        return "AdSec demonstrates how sections respond to combined axial load and bending moments. It can analyse concrete, steel and fibre-reinforced polymer (FRP) sections and compound sections.";
      }
    }

    public string AssemblyCompany
    {
      get
      {
        return "Copyright © Oasys 1985 - 2022";
      }
    }
    #endregion

    private void labelProductName_Click(object sender, EventArgs e)
    {

    }

    private void labelVersion_Click(object sender, EventArgs e)
    {

    }

    private void AboutBox_Load(object sender, EventArgs e)
    {

    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(@"https://www.oasys-software.com/");
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    private void button1_Click(object sender, EventArgs e)
    {
      Process.Start(@"rhino://package/search?name=adsec");
    }

    private void linkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      GH_AssemblyInfo adsecPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));
      string pluginvers = adsecPlugin.Version;
      Process.Start(@"mailto:oasys@arup.com?subject=Oasys AdSecGH version " + pluginvers);
    }

    private void labelApiVersion_Click(object sender, EventArgs e)
    {

    }
  }
}
