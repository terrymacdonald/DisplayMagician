using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATI.ADL;

namespace DisplayMagicianShared.AMD
{
    internal class AMDProfileData : VideoLibraryProfileData
    {

        // Struct to be used as the AMD Profile
        internal struct AMDProfile
        {
            public List<AMDAdapter> Adapters;
        }

        // Struct to store the Display
        internal struct AMDAdapter
        {
            internal ADLAdapterInfoX2 AdapterInfoX2;
            internal List<AMDDisplay> Displays;
        }

        // Struct to store the Display
        internal struct AMDDisplay
        {
            internal List<ADLMode> DisplayModes;
        }

        internal AMDProfile ProfileData {get; set;}

    }
}
