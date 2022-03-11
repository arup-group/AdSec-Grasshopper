using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdSecComputeTypes;
using Oasys.AdSec;
using Oasys.AdSec.Reinforcement;

namespace AdSecGHAdapter
{
    public static class InteropAdSecComputeTypes
    {

        public static bool IsPresent()
        {
            try
            {
                Section section = new Section();
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            return true;
        }

        public static Type GetType(Type type)
        {
            switch (type.Name)
            {
                case nameof(IAdSecSection):
                    return typeof(Section);

                default:
                    return null;
            }
        }

        public static object CastToSection(Oasys.AdSec.ISection section)
        {
            Section outSection = new Section();
            outSection.Profile = (IProfile)CastToIProfile(section.Profile);
            foreach (Oasys.AdSec.Reinforcement.Groups.IGroup group in section.ReinforcementGroups)
            {
                outSection.ReinforcementGroups.Add((IGroup)CastToIGroup(group));
            }
            outSection.StandardMaterial = section.Material.ToString();

            return outSection;
        }

        public static object CastToIGroup(Oasys.AdSec.Reinforcement.Groups.IGroup group)
        {
            switch (group.GetType().Name)
            {
                case (nameof(Oasys.AdSec.Reinforcement.Groups.ILineGroup)):
                    Oasys.AdSec.Reinforcement.Groups.ILineGroup lineGroup = (Oasys.AdSec.Reinforcement.Groups.ILineGroup)group;
                    AdSecComputeTypes.LineGroup outGroup = new AdSecComputeTypes.LineGroup();
                    outGroup.FirstBarPosition = (Point)CastToPoint(lineGroup.FirstBarPosition);
                    outGroup.FinalBarPosition = (Point)CastToPoint(lineGroup.LastBarPosition);
                    outGroup.Layer = (ILayer)CastToILayer(lineGroup.Layer);
                    return outGroup;

                case (nameof(Oasys.AdSec.Reinforcement.Groups.ISingleBars)):
                    Oasys.AdSec.Reinforcement.Groups.ISingleBars singleBars = (Oasys.AdSec.Reinforcement.Groups.ISingleBars)group;
                    AdSecComputeTypes.SingleBars outSingleBars = new AdSecComputeTypes.SingleBars();
                    outSingleBars.BarBundle = (BarBundle)CastToBarBundle(singleBars.BarBundle);
                    foreach (Oasys.Profiles.IPoint position in singleBars.Positions)
                    {
                        outSingleBars.Positions.Add((Point)CastToPoint(position));
                    }
                    return outSingleBars;

                case (nameof(Oasys.AdSec.Reinforcement.Groups.ICircleGroup)):
                    Oasys.AdSec.Reinforcement.Groups.ICircleGroup circleGroup = (Oasys.AdSec.Reinforcement.Groups.ICircleGroup)group;
                    AdSecComputeTypes.CircleGroup outCircleGroup = new AdSecComputeTypes.CircleGroup();
                    outCircleGroup.CentreOfTheCircle = (Point)CastToPoint(circleGroup.Centre);
                    outCircleGroup.Radius = circleGroup.Radius.Millimeters;
                    outCircleGroup.Angle = circleGroup.StartAngle.Radians;
                    outCircleGroup.Layer = (ILayer)CastToILayer(circleGroup.Layer);
                    return outCircleGroup;

                // todo: publish new nuget package
                //case (nameof(Oasys.AdSec.Reinforcement.Groups.IPerimeterGroup)):
                //    Oasys.AdSec.Reinforcement.Groups.IPerimeterGroup perimeterGroup = (Oasys.AdSec.Reinforcement.Groups.IPerimeterGroup)group;
                //    AdSecComputeTypes.PerimeterGroup outPerimeterGroup = new AdSecComputeTypes.PerimeterGroup();
                //    outPerimeterGroup.CentreOfTheCircle =
                //    return outPerimeterGroup;

                //case (nameof(Oasys.AdSec.Reinforcement.Groups.ITemplateGroup)):
                //    Oasys.AdSec.Reinforcement.Groups.ITemplateGroup templateGroup = (Oasys.AdSec.Reinforcement.Groups.ITemplateGroup)group;
                //    AdSecComputeTypes.TemplateGroup outTemplateGroup = new AdSecComputeTypes.TemplateGroup();
                //    return outTemplateGroup;

                //case (nameof(Oasys.AdSec.Reinforcement.Groups.ILinkGroup)):
                //    Oasys.AdSec.Reinforcement.Groups.ILinkGroup linkGroup = (Oasys.AdSec.Reinforcement.Groups.ILinkGroup)group;
                //    AdSecComputeTypes.LinkGroup outLinkGroup = new AdSecComputeTypes.LinkGroup();
                //    return outLinkGroup;

                default:
                    return null;
            }
        }

        private static object CastToILayer(Oasys.AdSec.Reinforcement.Layers.ILayer layer)
        {
            switch (layer.GetType().Name)
            {
                case (nameof(Oasys.AdSec.Reinforcement.Layers.ILayerByBarCount)):
                    Oasys.AdSec.Reinforcement.Layers.ILayerByBarCount layerByBarCount = (Oasys.AdSec.Reinforcement.Layers.ILayerByBarCount)layer;
                    AdSecComputeTypes.LayerByBarCount outLayerByBarCount = new AdSecComputeTypes.LayerByBarCount();
                    outLayerByBarCount.Count = layerByBarCount.Count;
                    outLayerByBarCount.BarBundle = (BarBundle)CastToBarBundle(layerByBarCount.BarBundle);
                    return outLayerByBarCount;

                // todo: publish new nuget package
                //case (nameof(Oasys.AdSec.Reinforcement.Layers.ILayerByBarPitch)):
                //    Oasys.AdSec.Reinforcement.Layers.ILayerByBarPitch layerByBarPitch = (Oasys.AdSec.Reinforcement.Layers.ILayerByBarPitch)layer;
                //    AdSecComputeTypes.LayerByBarPitch outLayerByBarPitch = new AdSecComputeTypes.LayerByBarPitch();
                //    outLayerByBarPitch.Count = layerByBarPitch.Count;
                //    outLayerByBarPitch.BarBundle = (BarBundle)CastToBarBundle(layerByBarPitch.BarBundle);
                //    return outLayerByBarPitch;

                default:
                    return null;
            }
        }

        private static object CastToBarBundle(Oasys.AdSec.Reinforcement.IBarBundle barBundle)
        {
            AdSecComputeTypes.BarBundle outBarBundle = new AdSecComputeTypes.BarBundle();
            outBarBundle.CountPerBundle = barBundle.CountPerBundle;
            outBarBundle.Diameter = barBundle.Diameter.Millimeters;
            outBarBundle.StandardMaterial = barBundle.Material.ToString();

            return outBarBundle;
        }

        public static object CastToIProfile(Oasys.Profiles.IProfile profile)
        {
            switch (profile.GetType().Name)
            {
                case (nameof(Oasys.Profiles.ICircleProfile)):
                    Oasys.Profiles.ICircleProfile circleProfile = (Oasys.Profiles.ICircleProfile)profile;
                    AdSecComputeTypes.CircleProfile outCircleProfile = new AdSecComputeTypes.CircleProfile();
                    outCircleProfile.Diameter = circleProfile.Diameter.Millimeters;
                    return outCircleProfile;

                case (nameof(Oasys.Profiles.IPolygon)):
                    Oasys.Profiles.IPolygon polygon = (Oasys.Profiles.IPolygon)profile;
                    AdSecComputeTypes.PolygonProfile outPolygonProfile = new AdSecComputeTypes.PolygonProfile();
                    foreach (Oasys.Profiles.IPoint point in polygon.Points)
                    {
                        outPolygonProfile.Points.Add((Point)CastToPoint(point));
                    }
                    return outPolygonProfile;

                case (nameof(Oasys.Profiles.IRectangleProfile)):
                    Oasys.Profiles.IRectangleProfile rectangleProfile = (Oasys.Profiles.IRectangleProfile)profile;
                    AdSecComputeTypes.RectangleProfile outRectangleProfile = new AdSecComputeTypes.RectangleProfile();
                    outRectangleProfile.Width = rectangleProfile.Width.Millimeters;
                    outRectangleProfile.Depth = rectangleProfile.Depth.Millimeters;
                    return outRectangleProfile;

                // todo: publish new nuget package
                //case (nameof(Oasys.Profiles.ITSectionProfile)):
                //    Oasys.Profiles.ITSectionProfile tSectionProfile = (Oasys.Profiles.ITSectionProfile)profile;
                //    AdSecComputeTypes.TProfile outTProfile = new AdSecComputeTypes.RectangleProfile();
                //    outRectangleProfile.Width = rectangleProfile.Width.Millimeters;
                //    outRectangleProfile.Depth = rectangleProfile.Depth.Millimeters;
                //    return outRectangleProfile;

                default:
                    return null;
            }
        }

        public static object CastToPoint(Oasys.Profiles.IPoint point)
        {
            AdSecComputeTypes.Point outPoint = new AdSecComputeTypes.Point();
            outPoint.Y = point.Y.Millimeters;
            outPoint.Z = point.Z.Millimeters;

            return outPoint;
        }
    }
}
