using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdSecGH.Converters
{
    public static class SpeckleConverter
    {
        public static Type GetTypeFor(Type type)
        {
            switch (type.Name)
            {
                case nameof(IAdSecSection):
                    //return typeof(AdSecComputeTypes.Section);
                    return null;

                default:
                    return null;
            }
        }

        public static bool IsPresent()
        {
            //try
            //{
            //    AdSecComputeTypes.Section section = new AdSecComputeTypes.Section();
            //}
            //catch (DllNotFoundException)
            //{
            //    return false;
            //}
            //return true;
            return false;
        }
    }
}
