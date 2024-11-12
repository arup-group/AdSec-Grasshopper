using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using Oasys.Business;

using OasysGH;
using OasysGH.Components;

namespace Oasys.GH.Helpers {

  public abstract class BusinessOasysGlue<T> : GH_OasysComponent, IDefaultValues where T : IBusinessComponent {

    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    public BusinessOasysGlue() : base("", "", "", "", "") {
      Name = BusinessComponent.Metadata.Name;
      NickName = BusinessComponent.Metadata.NickName;
      Description = BusinessComponent.Metadata.Description;
      Category = BusinessComponent.Organisation.Category;
      SubCategory = BusinessComponent.Organisation.SubCategory;
    }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;
    public void SetDefaultValues() { BusinessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      BusinessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      BusinessComponent.PopulateOutputParams(this);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      BusinessComponent.UpdateInputValues(this, DA);
      BusinessComponent.Compute();
      BusinessComponent.SetOutputValues(this, DA);
    }
  }

  public interface IDefaultValues {
    void SetDefaultValues();
  }

  public abstract class BusinessOasysDropdownGlue<T> : GH_OasysDropDownComponent, IDefaultValues
    where T : IBusinessComponent {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    public BusinessOasysDropdownGlue() : base("", "", "", "", "") {
      Name = BusinessComponent.Metadata.Name;
      NickName = BusinessComponent.Metadata.NickName;
      Description = BusinessComponent.Metadata.Description;
      Category = BusinessComponent.Organisation.Category;
      SubCategory = BusinessComponent.Organisation.SubCategory;
    }

    public override Guid ComponentGuid { get; }

    public override OasysPluginInfo PluginInfo { get; }

    public void SetDefaultValues() { BusinessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      BusinessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      BusinessComponent.PopulateOutputParams(this);
    }

    public override void SetSelected(int i, int j) { }

    protected override void SolveInternal(IGH_DataAccess da) {
      BusinessComponent.UpdateInputValues(this, da);
      BusinessComponent.Compute();
      BusinessComponent.SetOutputValues(this, da);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>();
      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();
      _isInitialised = true;
    }
  }
}
