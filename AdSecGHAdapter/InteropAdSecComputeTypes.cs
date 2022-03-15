using AdSecComputeTypes;
using Oasys.AdSec.Reinforcement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdSecGHAdapter
{
    public static class InteropAdSecComputeTypes
    {

        public static bool IsPresent()
        {
            try
            {
                AdSecComputeTypes.Section section = new AdSecComputeTypes.Section();
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
                    return typeof(AdSecComputeTypes.Section);

                default:
                    return null;
            }
        }

        public static object CastToSection(Oasys.AdSec.ISection section, string codeName, string materialName)
        {
            // build standard material string
            List<string> values = new List<string>(codeName.Split(' '));
            values.RemoveAt(values.Count - 2);
            string standardMaterial = "Concrete." + string.Join(".", values) + "." + materialName;

            AdSecComputeTypes.Section outSection = new AdSecComputeTypes.Section();
            // outSection.Cover = CastToCover(section.Cover);
            outSection.Profile = (AdSecComputeTypes.IProfile)CastToIProfile(section.Profile);
            foreach (Oasys.AdSec.Reinforcement.Groups.IGroup group in section.ReinforcementGroups)
            {
                //outSection.ReinforcementGroups.Add((AdSecComputeTypes.IGroup)CastToIGroup(group, standardMaterial));
            }
            outSection.StandardMaterial = standardMaterial;

            return outSection;
        }

        private static Cover CastToCover(Oasys.AdSec.Reinforcement.ICover cover)
        {
            AdSecComputeTypes.Cover outCover = new AdSecComputeTypes.Cover(cover.UniformCover.Millimeters);

            return outCover;
        }

        public static object CastToIGroup(Oasys.AdSec.Reinforcement.Groups.IGroup group, string standardMaterial)
        {
            if (group.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Groups.ILineGroup).ToString() + "_Implementation"))
            {
                Oasys.AdSec.Reinforcement.Groups.ILineGroup lineGroup = (Oasys.AdSec.Reinforcement.Groups.ILineGroup)group;
                AdSecComputeTypes.LineGroup outGroup = new AdSecComputeTypes.LineGroup();
                outGroup.FirstBarPosition = (AdSecComputeTypes.Point)CastToPoint(lineGroup.FirstBarPosition);
                outGroup.FinalBarPosition = (AdSecComputeTypes.Point)CastToPoint(lineGroup.LastBarPosition);
                outGroup.Layer = (AdSecComputeTypes.ILayer)CastToILayer(lineGroup.Layer, standardMaterial);
                return outGroup;
            }
            else if (group.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Groups.ISingleBars).ToString() + "_Implementation"))
            {
                Oasys.AdSec.Reinforcement.Groups.ISingleBars singleBars = (Oasys.AdSec.Reinforcement.Groups.ISingleBars)group;
                AdSecComputeTypes.SingleBars outSingleBars = new AdSecComputeTypes.SingleBars();
                outSingleBars.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(singleBars.BarBundle, standardMaterial);
                foreach (Oasys.Profiles.IPoint position in singleBars.Positions)
                {
                    outSingleBars.Positions.Add((AdSecComputeTypes.Point)CastToPoint(position));
                }
                return outSingleBars;
            }
            else if (group.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Groups.ICircleGroup).ToString() + "_Implementation"))
            {
                Oasys.AdSec.Reinforcement.Groups.ICircleGroup circleGroup = (Oasys.AdSec.Reinforcement.Groups.ICircleGroup)group;
                AdSecComputeTypes.CircleGroup outCircleGroup = new AdSecComputeTypes.CircleGroup();
                outCircleGroup.CentreOfTheCircle = (AdSecComputeTypes.Point)CastToPoint(circleGroup.Centre);
                outCircleGroup.Radius = circleGroup.Radius.Millimeters;
                outCircleGroup.Angle = circleGroup.StartAngle.Radians;
                outCircleGroup.Layer = (AdSecComputeTypes.ILayer)CastToILayer(circleGroup.Layer, standardMaterial);
                return outCircleGroup;
            }
            // todo: publish new nuget package
            else if (group.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Groups.IPerimeterGroup).ToString() + "_Implementation"))
            {
                Oasys.AdSec.Reinforcement.Groups.IPerimeterGroup perimeterGroup = (Oasys.AdSec.Reinforcement.Groups.IPerimeterGroup)group;
                AdSecComputeTypes.PerimeterGroup outPerimeterGroup = new AdSecComputeTypes.PerimeterGroup();
                foreach (Oasys.AdSec.Reinforcement.Layers.ILayer layer in perimeterGroup.Layers)
                {
                    outPerimeterGroup.Layers.Add((AdSecComputeTypes.ILayer)CastToILayer(layer, standardMaterial));
                }
                return outPerimeterGroup;
            }
            else if (group.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Groups.ITemplateGroup).ToString() + "_Implementation"))
            {
                //Oasys.AdSec.Reinforcement.Groups.ITemplateGroup templateGroup = (Oasys.AdSec.Reinforcement.Groups.ITemplateGroup)group;

                //AdSecComputeTypes.Face face = AdSecComputeTypes.Face.Bottom;
                //AdSecComputeTypes.TemplateGroup outTemplateGroup = new AdSecComputeTypes.TemplateGroup(face);
                //outTemplateGroup.Layers = new List<AdSecComputeTypes.ILayer>();
                ////if (templateGroup.Layers != null)
                ////{
                //    foreach (Oasys.AdSec.Reinforcement.Layers.ILayer layer in templateGroup.Layers)
                //    {
                //        outTemplateGroup.Layers.Add((AdSecComputeTypes.ILayer)CastToILayer(layer, standardMaterial));
                //    }
                ////}
                //return outTemplateGroup;
            }
            else if (group.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Groups.ILinkGroup).ToString() + "_Implementation"))
            {
                Oasys.AdSec.Reinforcement.Groups.ILinkGroup linkGroup = (Oasys.AdSec.Reinforcement.Groups.ILinkGroup)group;
                AdSecComputeTypes.LinkGroup outLinkGroup = new AdSecComputeTypes.LinkGroup();
                outLinkGroup.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(linkGroup.BarBundle, standardMaterial);
                return outLinkGroup;
            }
            return null;
        }

        private static object CastToILayer(Oasys.AdSec.Reinforcement.Layers.ILayer layer, string standardMaterial)
        {
            if (layer.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Layers.ILayerByBarCount).ToString() + "_Implementation"))
            {
                Oasys.AdSec.Reinforcement.Layers.ILayerByBarCount layerByBarCount = (Oasys.AdSec.Reinforcement.Layers.ILayerByBarCount)layer;
                AdSecComputeTypes.LayerByBarCount outLayerByBarCount = new AdSecComputeTypes.LayerByBarCount();
                outLayerByBarCount.Count = layerByBarCount.Count;
                outLayerByBarCount.BarBundle = (AdSecComputeTypes.BarBundle)CastToBarBundle(layerByBarCount.BarBundle, standardMaterial);
                return outLayerByBarCount;
            }
            else if (layer.GetType().ToString().Equals(typeof(Oasys.AdSec.Reinforcement.Layers.ILayerByBarPitch).ToString() + "_Implementation"))
            {
                // todo: publish new nuget package
                //    Oasys.AdSec.Reinforcement.Layers.ILayerByBarPitch layerByBarPitch = (Oasys.AdSec.Reinforcement.Layers.ILayerByBarPitch)layer;
                //    AdSecComputeTypes.LayerByBarPitch outLayerByBarPitch = new AdSecComputeTypes.LayerByBarPitch();
                //    outLayerByBarPitch.Count = layerByBarPitch.Count;
                //    outLayerByBarPitch.BarBundle = (BarBundle)CastToBarBundle(layerByBarPitch.BarBundle);
                //    return outLayerByBarPitch;
            }
            return null;
        }

        private static object CastToBarBundle(Oasys.AdSec.Reinforcement.IBarBundle barBundle, string standardMaterial)
        {
            AdSecComputeTypes.BarBundle outBarBundle = new AdSecComputeTypes.BarBundle();
            outBarBundle.CountPerBundle = barBundle.CountPerBundle;
            outBarBundle.Diameter = barBundle.Diameter.Millimeters;
            outBarBundle.StandardMaterial = standardMaterial;

            return outBarBundle;
        }

        public static object CastToIProfile(Oasys.Profiles.IProfile profile)
        {
            if (profile.GetType().ToString().Equals(typeof(Oasys.Profiles.ICircleProfile).ToString() + "_Implementation"))
            {
                Oasys.Profiles.ICircleProfile circleProfile = (Oasys.Profiles.ICircleProfile)profile;
                AdSecComputeTypes.CircleProfile outCircleProfile = new AdSecComputeTypes.CircleProfile();
                outCircleProfile.Diameter = circleProfile.Diameter.Millimeters;
                return outCircleProfile;
            }
            else if (profile.GetType().ToString().Equals(typeof(Oasys.Profiles.IPolygon).ToString() + "_Implementation"))
            {
                Oasys.Profiles.IPolygon polygon = (Oasys.Profiles.IPolygon)profile;
                AdSecComputeTypes.PolygonProfile outPolygonProfile = new AdSecComputeTypes.PolygonProfile();
                foreach (Oasys.Profiles.IPoint point in polygon.Points)
                {
                    outPolygonProfile.Points.Add((AdSecComputeTypes.Point)CastToPoint(point));
                }
                return outPolygonProfile;
            }
            else if (profile.GetType().ToString().Equals(typeof(Oasys.Profiles.IRectangleProfile).ToString() + "_Implementation"))
            {
                Oasys.Profiles.IRectangleProfile rectangleProfile = (Oasys.Profiles.IRectangleProfile)profile;
                AdSecComputeTypes.RectangleProfile outRectangleProfile = new AdSecComputeTypes.RectangleProfile();
                outRectangleProfile.Width = rectangleProfile.Width.Millimeters;
                outRectangleProfile.Depth = rectangleProfile.Depth.Millimeters;
                return outRectangleProfile;
            }
            else if (profile.GetType().ToString().Equals(typeof(Oasys.Profiles.ICircleProfile).ToString() + "_Implementation"))
            {
                // todo: publish new nuget package
                //    Oasys.Profiles.ITSectionProfile tSectionProfile = (Oasys.Profiles.ITSectionProfile)profile;
                //    AdSecComputeTypes.TProfile outTProfile = new AdSecComputeTypes.RectangleProfile();
                //    outRectangleProfile.Width = rectangleProfile.Width.Millimeters;
                //    outRectangleProfile.Depth = rectangleProfile.Depth.Millimeters;
                //    return outRectangleProfile;
            }
            return null;
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
