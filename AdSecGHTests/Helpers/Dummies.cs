using AdSecGH.Helpers;

namespace AdSecGHTests.Helpers {
  public static class Dummies {
    public class DummyContext : IGrasshopperDocumentContext {
      public void AddObject(object obj, bool recordUndo = false) {
        //only for tests, so can be empty
      }
    }
  }
}
