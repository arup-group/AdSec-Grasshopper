using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AdSecGH.Helpers
{
  public class PostHog
  {
    public static void AddedToDocument(GH_Component component)
    {
      string eventName = "AddedToDocument";
      Dictionary<string, object> properties = new Dictionary<string, object>()
      {
        { "componentName", component.Name },
      };
      _ = OasysGH.Helpers.PostHog.SendToPostHog(PluginInfo.Instance, eventName, properties);
    }

    public static void ModelIO(string interactionType, int size = 0)
    {
      string eventName = "ModelIO";
      Dictionary<string, object> properties = new Dictionary<string, object>()
      {
        { "interactionType", interactionType },
        { "size", size },
      };
      _ = OasysGH.Helpers.PostHog.SendToPostHog(PluginInfo.Instance, eventName, properties);
    }

    public static void PluginLoaded(string error = "")
    {
      string eventName = "PluginLoaded";

      Dictionary<string, object> properties = new Dictionary<string, object>()
        {
          { "rhinoVersion", Rhino.RhinoApp.Version.ToString().Split('.')
             + "." + Rhino.RhinoApp.Version.ToString().Split('.')[1] },
          { "rhinoMajorVersion", Rhino.RhinoApp.ExeVersion },
          { "rhinoServiceRelease", Rhino.RhinoApp.ExeServiceRelease },
          { "loadingError", error },
        };
      _ = OasysGH.Helpers.PostHog.SendToPostHog(PluginInfo.Instance, eventName, properties);
    }

    internal static void RemovedFromDocument(GH_Component component)
    {
      if (component.Attributes.Selected)
      {
        string eventName = "RemovedFromDocument";
        Dictionary<string, object> properties = new Dictionary<string, object>()
        {
          { "componentName", component.Name },
          { "runCount", component.RunCount },
        };
        _ = OasysGH.Helpers.PostHog.SendToPostHog(PluginInfo.Instance, eventName, properties);
      }
    }
  }
}
