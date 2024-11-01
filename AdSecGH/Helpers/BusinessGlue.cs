using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using Oasys.Business;

using OasysGH;
using OasysGH.Components;

namespace Oasys.GH.Helpers {

  public abstract class BusinessOasysGlue<T> : GH_OasysComponent, IDefaultValues where T : IBusinessComponent {

    private readonly T _businessComponent = Activator.CreateInstance<T>();

    public BusinessOasysGlue() : base("", "", "", "", "") {
      Name = _businessComponent.Metadata.Name;
      NickName = _businessComponent.Metadata.NickName;
      Description = _businessComponent.Metadata.Description;
      Category = _businessComponent.Organisation.Category;
      SubCategory = _businessComponent.Organisation.SubCategory;
    }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;
    public void SetDefaultValues() { _businessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      _businessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      _businessComponent.PopulateOutputParams(this);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      _businessComponent.Compute();
      _businessComponent.SetOutputValues(this, DA);
    }
  }

  public interface IDefaultValues {
    void SetDefaultValues();
  }

  public abstract class BusinessGlue<T> : GH_OasysDropDownComponent, IDefaultValues where T : IBusinessComponent {
    private readonly T _businessComponent = Activator.CreateInstance<T>();

    public BusinessGlue() : base("", "", "", "", "") {
      Name = _businessComponent.Metadata.Name;
      NickName = _businessComponent.Metadata.NickName;
      Description = _businessComponent.Metadata.Description;
      Category = _businessComponent.Organisation.Category;
      SubCategory = _businessComponent.Organisation.SubCategory;
    }

    public override Guid ComponentGuid { get; }

    public override OasysPluginInfo PluginInfo { get; }

    public void SetDefaultValues() { _businessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      _businessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      _businessComponent.PopulateOutputParams(this);
    }

    public override void SetSelected(int i, int j) { }

    protected override void SolveInternal(IGH_DataAccess da) {
      _businessComponent.Compute();
      _businessComponent.SetOutputValues(this, da);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>();
      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();
      _isInitialised = true;
    }
  }
}
