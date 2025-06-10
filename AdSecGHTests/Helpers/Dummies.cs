using AdSecGH.Helpers;

using Grasshopper.Kernel;

namespace AdSecGHTests.Helpers {
  public static class Dummies {
    public class DummyContext : IGrasshopperDocumentContext {
      public void AddObject(IGH_DocumentObject obj, bool recordUndo = false) {
        //only for tests, so can be empty
      }
    }
  }
}
