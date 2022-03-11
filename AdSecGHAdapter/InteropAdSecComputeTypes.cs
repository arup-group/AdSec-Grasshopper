using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdSecComputeTypes;
using Oasys.AdSec;

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

            return outSection;
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
                        outPolygonProfile.Points.Add((Point)CastToIPoint(point));
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

        public static object CastToIPoint(Oasys.Profiles.IPoint point)
        {
            AdSecComputeTypes.Point outPoint = new AdSecComputeTypes.Point();
            outPoint.Y = point.Y.Millimeters;
            outPoint.Z = point.Z.Millimeters;

            return outPoint;
        }
    }
}
