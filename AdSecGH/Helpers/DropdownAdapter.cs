using System;
using System.Collections.Generic;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;

namespace Oasys.GH.Helpers {
  public abstract class DropdownAdapter<T> : GH_OasysDropDownComponent, IDefaultValues where T : IFunction {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    protected DropdownAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
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
