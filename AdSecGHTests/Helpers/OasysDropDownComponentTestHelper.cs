using System;
using System.IO;

using GH_IO;

using Grasshopper.Kernel;

using OasysGH.Components;

using Xunit;

namespace AdSecGHTests.Helpers {
  public static class OasysDropDownComponentTestHelper {

    public static void ChangeDropDownTest(GH_OasysDropDownComponent comp, bool ignoreSpacerDescriptionsCount = false) {
      if (!ignoreSpacerDescriptionsCount) {
        Assert.Equal(comp.DropDownItems.Count, comp.SpacerDescriptions.Count);
      }
      Assert.Equal(comp.DropDownItems.Count, comp.SelectedItems.Count);

      for (int i = 0; i < comp.DropDownItems.Count; i++) {
        comp.SetSelected(i, 0);

        for (int j = 0; j < comp.DropDownItems[i].Count; j++) {
          comp.SetSelected(i, j);
          Assert.Empty(DeserializeTest(comp));
          Assert.Equal(comp.SelectedItems[i], comp.DropDownItems[i][j]);
        }
      }
    }

    public static string DeserializeTest(GH_OasysDropDownComponent comp, string customIdentifier = "") {
      var originalComponent = PrepareComponentForSerialization(comp, out var serialize);
      string pathFileName = GetFilePath(comp, customIdentifier);

      if (!serialize.SaveQuiet(pathFileName)) {
        return "Save quite operation failed";
      }

      var deserialize = new GH_DocumentIO();
      if (!deserialize.Open(pathFileName)) {
        return "Open operation failed";
      }

      var deserializedComponent = GetDeserialisedComponent(deserialize);

      bool areEqual = Duplicates.AreEqual(originalComponent, deserializedComponent, true);
      return areEqual ? string.Empty : "Objects are not equal";
    }

    private static GH_Component PrepareComponentForSerialization(IGH_DocumentObject comp, out GH_DocumentIO serialize) {
      comp.CreateAttributes();

      var doc = new GH_Document();
      doc.AddObject(comp, true);

      serialize = new GH_DocumentIO { Document = doc, };
      var originalComponent = (GH_Component)serialize.Document.Objects[0];

      originalComponent.Attributes.PerformLayout();
      originalComponent.ExpireSolution(true);
      originalComponent.Params.Output[0].CollectData();

      return originalComponent;
    }

    private static string GetFilePath(GH_ISerializable comp, string customIdentifier) {
      string path = Path.Combine(Environment.CurrentDirectory, "GH-Test-Files");
      Directory.CreateDirectory(path);

      return $"{Path.Combine(path, comp.GetType().Name)}{customIdentifier}.gh";
    }

    private static GH_Component GetDeserialisedComponent(GH_DocumentIO deserialize) {
      var deserializedComponent = (GH_Component)deserialize.Document.Objects[0];
      deserializedComponent.Attributes.PerformLayout();
      deserializedComponent.ExpireSolution(true);
      deserializedComponent.Params.Output[0].CollectData();
      return deserializedComponent;
    }
  }
}
