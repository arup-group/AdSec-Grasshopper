using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.AdSec;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace AdSecGH.Converters
{
  public class SpeckleConverter : ISpeckleConverter
  {
    public string Description => "Speckle Kit for AdSecGH";

    public string Name => nameof(SpeckleConverter);

    public string Author => "Arup";

    public string WebsiteOrEmail => "https://www.arup.com";

    public ProgressReport Report { get; private set; } = new ProgressReport();

    public static string AdSecGHAppName = "AdSecGH";

    public static Type GetTypeFor(Type type)
    {
      switch (type.Name)
      {
        case nameof(IAdSecSection):
          return typeof(Objects.Structural.AdSec.Geometry.AdSecSection);

        default:
          return null;
      }
    }

    public static bool IsPresent()
    {
      try
      {
        // Get a list of all available kits
        IEnumerable<ISpeckleKit> kits = KitManager.Kits;

        // Get a specific kit by name or from the assembly full name
        //var kitByName = Speckle.Core.Kits.KitManager.Kits.FirstOrDefault(kit => kit.Name == "CoreKit");
        //var kitFromAssembly = Speckle.Core.Kits.KitManager.GetKit(typeof(CoreKit).Assembly.FullName);

        // Load the default Objects Kit and the included Revit converter
        var kit = Speckle.Core.Kits.KitManager.GetDefaultKit();


        //var converter = kit.LoadConverter(ConnectorRevitUtils.RevitAppName);
        //converter.SetContextDocument(CurrentDoc.Document);



        Objects.Structural.AdSec.Geometry.AdSecSection section = new Objects.Structural.AdSec.Geometry.AdSecSection();
      }
      catch (DllNotFoundException)
      {
        return false;
      }
      return true;
    }

    public bool CanConvertToNative(Base @object)
    {
      switch (@object)
      {
        case Objects.Structural.AdSec.Geometry.AdSecSection _:
          return true;

        default:
          return false;
      }
    }

    public bool CanConvertToSpeckle(object @object)
    {
      switch (@object)
      {
        case ISection _:
          return true;

        default:
          return false;
      }
    }

    public object ConvertToNative(Base @object)
    {
      throw new NotImplementedException();
    }

    public List<object> ConvertToNative(List<Base> objects)
    {
      return objects.Select(x => ConvertToNative(x)).ToList();
    }

    public Base ConvertToSpeckle(object @object)
    {
      Base @base = null;

      switch (@object)
      {
        case ISection o:
          @base = ISectionToSpeckle(o);
          Report.Log($"Converted ISection {o}");
          break;

        default:
          Report.Log($"Skipped not supported type: {@object.GetType()}");
          throw new NotSupportedException();
      }

      return @base;
    }

    private Base ISectionToSpeckle(ISection o)
    {
      throw new NotImplementedException();
    }

    public List<Base> ConvertToSpeckle(List<object> objects)
    {
      return objects.Select(x => ConvertToSpeckle(x)).ToList();
    }

    public IEnumerable<string> GetServicedApplications()
    {
      return new string[] { AdSecGHAppName };
    }

    public void SetContextDocument(object doc)
    {
      throw new NotImplementedException();
    }

    public void SetContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      throw new NotImplementedException();
    }

    public void SetConverterSettings(object settings)
    {
      throw new NotImplementedException();
    }

    public void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      throw new NotImplementedException();
    }
  }
}
