using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scavenger
{
    public class Config
    {
        public static uint TrackedItemCount = 50;
        public static float ItemSpotSpawnHeight = 0.1f;
        public static float ItemSpotLabelVisibleDistance = 0.6f;
        public static float ItemSpotShineVisibleDistance = 5.0f;
        public static float ItemSpotGrabDistance = 0.5f;
        public static float ItemSpotLabelScale = 0.0005f;
        public static float ItemSpotLabelHeight = 0.3f;
        public static float ItemDropRaycastSphereSize = 0.2f;
        public static float ItemDropRaycastLength = 3.0f;

        public static float[] ShineColorOOR = new float[3] { 0.95f, 0.35f, 0.1f }; // out of range
        public static float[] ShineColorIR = new float[3] { 0.35f, 0.95f, 0.1f }; // in range

    }
}
