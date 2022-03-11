using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdSecComputeTypes;
using AdSecGH.AdapterInterfaces;

namespace AdSecGHAdapter
{
    public class AdSecSection : IAdSecSection
    {
        private Section section;

        public object GetSection()
        {
            return section;
        }
    }
}
