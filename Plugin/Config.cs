using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using ThunderRoad;

namespace Scavenger
{
    public class Config
    {
        [ModOption(name = "Enable item tracking", defaultValueIndex = 1)]
        public static bool TrackingEnabled = true;
        [ModOption(name = "Disable particles")]
        public static bool ItemSpotNoParticles = false;
        public static int TrackedItemCount = 50;
        public static float ItemSpotSpawnHeight = 0.1f;
        public static float ItemSpotLabelVisibleDistance = 0.6f;
        public static float ItemSpotShineVisibleDistance = 5.0f;
        public static float ItemSpotGrabDistance = 0.5f;
        public static float ItemSpotShineScale = 1.0f;
        public static float ItemSpotLabelScale = 1.0f;
        public static float ItemSpotLabelHeight = 0.3f;
        public static float ItemDropRaycastSphereSize = 0.2f;
        public static float ItemDropRaycastLength = 3.0f;
        public static float[] ShineColorOOR = new float[3] { 1.0f, 1.0f, 1.0f }; // out of range
        public static float[] ShineColorIR = new float[3] { 0.35f, 0.95f, 0.1f }; // in range

        public static bool ItemExclusionListUseRegex = true;
        public static HashSet<string> ItemExclusionList = new HashSet<string>();
        public static List<Regex> ItemExclusionListRegex = new List<Regex>();
    }
}
