using System;

//using Oasys.AdSec;
//using Oasys.AdSec.Materials;
//using Oasys.AdSec.Reinforcement;
//using Oasys.AdSec.Reinforcement.Groups;
//using Oasys.AdSec.Reinforcement.Layers;
//using Oasys.Profiles;

namespace AdSecGHAdapter {
  public static class InteropAdSecComputeTypes {
    //public static object CastToBarBundle(IBarBundle barBundle, string codeName)
    //{
    //    AdSecComputeTypes.BarBundle outBarBundle = new AdSecComputeTypes.BarBundle();
    //    outBarBundle.CountPerBundle = barBundle.CountPerBundle;
    //    outBarBundle.Diameter = barBundle.Diameter.Millimeters;
    //    outBarBundle.StandardMaterial = GetStandardMaterial(barBundle.Material, codeName);
    //    return outBarBundle;
    //}

    //private static string GetStandardMaterial(IReinforcement material, string codeName)
    //{
    //    List<string> values = new List<string>(codeName.Split(' '));
    //    string standardMaterial = "Reinforcement.Steel." + string.Join(".", values) + "." + "S500B";
    //    return standardMaterial;
    //}

    //public static AdSecComputeTypes.Cover CastToCover(ICover cover)
    //{
    //    AdSecComputeTypes.Cover outCover = new AdSecComputeTypes.Cover(cover.UniformCover.Millimeters);

    //    return outCover;
    //}

    //public static AdSecComputeTypes.Flange CastToFlange(IFlange flange)
    //{
    //    AdSecComputeTypes.Flange outFlange = new AdSecComputeTypes.Flange();
    //    outFlange.Thickness = flange.Thickness.Millimeters;
    //    outFlange.Width = flange.Width.Millimeters;

    //    return outFlange;
    //}

    //public static object CastToIGroup(IGroup group, string codeName)
    //{
    //    if (group.GetType().ToString().Equals(typeof(ILineGroup).ToString() + "_Implementation"))
    //    {
    //        ILineGroup lineGroup = (ILineGroup)group;
    //        AdSecComputeTypes.LineGroup outGroup = new AdSecComputeTypes.LineGroup();
    //        outGroup.FirstBarPosition = (AdSecComputeTypes.Point)CastToIPoint(lineGroup.FirstBarPosition);
    //        outGroup.FinalBarPosition = (AdSecComputeTypes.Point)CastToIPoint(lineGroup.LastBarPosition);
    //        outGroup.Layer = (AdSecComputeTypes.ILayer)CastToILayer(lineGroup.Layer, codeName);

    //        return outGroup;
    //    }
    //    else if (group.GetType().ToString().Equals(typeof(ISingleBars).ToString() + "_Implementation"))
    //    {
    //        ISingleBars singleBars = (ISingleBars)group;
    //        AdSecComputeTypes.SingleBars outSingleBars = new AdSecComputeTypes.SingleBars();
    //        outSingleBars.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(singleBars.BarBundle, codeName);
    //        foreach (IPoint position in singleBars.Positions)
    //        {
    //            outSingleBars.Positions.Add((AdSecComputeTypes.Point)CastToIPoint(position));
    //        }

    //        return outSingleBars;
    //    }
    //    else if (group.GetType().ToString().Equals(typeof(ICircleGroup).ToString() + "_Implementation"))
    //    {
    //        ICircleGroup circleGroup = (ICircleGroup)group;
    //        AdSecComputeTypes.CircleGroup outCircleGroup = new AdSecComputeTypes.CircleGroup();
    //        outCircleGroup.CentreOfTheCircle = (AdSecComputeTypes.Point)CastToIPoint(circleGroup.Centre);
    //        outCircleGroup.Radius = circleGroup.Radius.Millimeters;
    //        outCircleGroup.Angle = circleGroup.StartAngle.Radians;
    //        outCircleGroup.Layer = (AdSecComputeTypes.ILayer)CastToILayer(circleGroup.Layer, codeName);

    //        return outCircleGroup;
    //    }
    //    else if (group.GetType().ToString().Equals(typeof(IPerimeterGroup).ToString() + "_Implementation"))
    //    {
    //        IPerimeterGroup perimeterGroup = (IPerimeterGroup)group;
    //        AdSecComputeTypes.PerimeterGroup outPerimeterGroup = new AdSecComputeTypes.PerimeterGroup();
    //        foreach (ILayer layer in perimeterGroup.Layers)
    //        {
    //            outPerimeterGroup.Layers.Add((AdSecComputeTypes.ILayer)CastToILayer(layer, codeName));
    //        }

    //        return outPerimeterGroup;
    //    }
    //    else if (group.GetType().ToString().Equals(typeof(ITemplateGroup).ToString() + "_Implementation"))
    //    {
    //        ITemplateGroup templateGroup = (ITemplateGroup)group;
    //        AdSecComputeTypes.Face face = AdSecComputeTypes.Face.Bottom;
    //        AdSecComputeTypes.TemplateGroup outTemplateGroup = new AdSecComputeTypes.TemplateGroup(face);
    //        outTemplateGroup.Layers = new List<AdSecComputeTypes.ILayer>();
    //        if (templateGroup.Layers != null)
    //        {
    //            foreach (ILayer layer in templateGroup.Layers)
    //            {
    //                outTemplateGroup.Layers.Add((AdSecComputeTypes.ILayer)CastToILayer(layer, codeName));
    //            }
    //        }
    //        return outTemplateGroup;
    //    }
    //    else if (group.GetType().ToString().Equals(typeof(ILinkGroup).ToString() + "_Implementation"))
    //    {
    //        ILinkGroup linkGroup = (ILinkGroup)group;
    //        AdSecComputeTypes.LinkGroup outLinkGroup = new AdSecComputeTypes.LinkGroup();
    //        outLinkGroup.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(linkGroup.BarBundle, codeName);

    //        return outLinkGroup;
    //    }
    //    return null;
    //}

    //public static object CastToILayer(ILayer layer, string codeName)
    //{
    //    if (layer.GetType().ToString().Equals(typeof(ILayerByBarCount).ToString() + "_Implementation"))
    //    {
    //        ILayerByBarCount layerByBarCount = (ILayerByBarCount)layer;
    //        AdSecComputeTypes.LayerByBarCount outLayerByBarCount = new AdSecComputeTypes.LayerByBarCount();
    //        outLayerByBarCount.Count = layerByBarCount.Count;
    //        outLayerByBarCount.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(layerByBarCount.BarBundle, codeName);

    //        return outLayerByBarCount;
    //    }
    //    else if (layer.GetType().ToString().Equals(typeof(ILayerByBarPitch).ToString() + "_Implementation"))
    //    {
    //        ILayerByBarPitch layerByBarPitch = (ILayerByBarPitch)layer;
    //        AdSecComputeTypes.LayerByBarPitch outLayerByBarPitch = new AdSecComputeTypes.LayerByBarPitch();
    //        outLayerByBarPitch.Pitch = layerByBarPitch.Pitch.Millimeters;
    //        outLayerByBarPitch.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(layerByBarPitch.BarBundle, codeName);

    //        return outLayerByBarPitch;
    //    }
    //    return null;
    //}

    //public static object CastToIPoint(IPoint point)
    //{
    //    AdSecComputeTypes.Point outPoint = new AdSecComputeTypes.Point();
    //    outPoint.Y = point.Y.Millimeters;
    //    outPoint.Z = point.Z.Millimeters;

    //    return outPoint;
    //}

    //public static object CastToIProfile(IProfile profile)
    //{
    //    if (profile.GetType().ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
    //    {
    //        ICircleProfile circleProfile = (ICircleProfile)profile;
    //        AdSecComputeTypes.CircleProfile outCircleProfile = new AdSecComputeTypes.CircleProfile();
    //        outCircleProfile.Diameter = circleProfile.Diameter.Millimeters;

    //        return outCircleProfile;
    //    }
    //    else if (profile.GetType().ToString().Equals(typeof(IPolygon).ToString() + "_Implementation"))
    //    {
    //        IPolygon polygon = (IPolygon)profile;
    //        AdSecComputeTypes.PolygonProfile outPolygonProfile = new AdSecComputeTypes.PolygonProfile();
    //        foreach (IPoint point in polygon.Points)
    //        {
    //            outPolygonProfile.Points.Add((AdSecComputeTypes.Point)CastToIPoint(point));
    //        }

    //        return outPolygonProfile;
    //    }
    //    else if (profile.GetType().ToString().Equals(typeof(IRectangleProfile).ToString() + "_Implementation"))
    //    {
    //        IRectangleProfile rectangleProfile = (IRectangleProfile)profile;
    //        AdSecComputeTypes.RectangleProfile outRectangleProfile = new AdSecComputeTypes.RectangleProfile();
    //        outRectangleProfile.Width = rectangleProfile.Width.Millimeters;
    //        outRectangleProfile.Depth = rectangleProfile.Depth.Millimeters;

    //        return outRectangleProfile;
    //    }
    //    else if (profile.GetType().ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
    //    {
    //        ITSectionProfile tSectionProfile = (ITSectionProfile)profile;
    //        AdSecComputeTypes.TProfile outTProfile = new AdSecComputeTypes.TProfile();
    //        outTProfile.Depth = tSectionProfile.Depth.Millimeters;
    //        outTProfile.Flange = (AdSecComputeTypes.Flange)CastToFlange(tSectionProfile.Flange);
    //        outTProfile.Web = (AdSecComputeTypes.Web)CastToWeb(tSectionProfile.Web);

    //        return tSectionProfile;
    //    }
    //    return null;
    //}

    //public static object CastToSection(ISection section, string codeName, string materialName)
    //{
    //    // build standard material string
    //    List<string> values = new List<string>(codeName.Split(' '));
    //    string standardMaterial = "Concrete." + string.Join(".", values) + "." + materialName;

    //    AdSecComputeTypes.Section outSection = new AdSecComputeTypes.Section();
    //    outSection.Cover = CastToCover(section.Cover);
    //    outSection.Profile = (AdSecComputeTypes.IProfile)CastToIProfile(section.Profile);
    //    foreach (IGroup group in section.ReinforcementGroups)
    //    {
    //        outSection.ReinforcementGroups.Add((AdSecComputeTypes.IGroup)CastToIGroup(group, codeName));
    //    }
    //    outSection.StandardMaterial = standardMaterial;

    //    return outSection;
    //}

    //public static AdSecComputeTypes.Web CastToWeb(IWeb web)
    //{
    //    AdSecComputeTypes.Web outWeb = new AdSecComputeTypes.Web();
    //    outWeb.BottomThickness = web.BottomThickness.Millimeters;
    //    outWeb.TopThickness = web.TopThickness.Millimeters;

    //    return outWeb;
    //}

    public static Type GetType(Type type) {
      switch (type.Name) {
        //case nameof(IAdSecSection):
        //  return typeof(AdSecComputeTypes.Section);

        default:
          return null;
      }
    }

    public static bool IsPresent() {
      //    try
      //    {
      //        AdSecComputeTypes.Section section = new AdSecComputeTypes.Section();
      //    }
      //    catch (DllNotFoundException)
      //    {
      return false;
      //}
      //return true;
    }
  }
}
