using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;

namespace Scavenger
{
    public class LoadModule : LevelModule
    {

        public string mod_version = "0.0";
        public string mod_name = "UnnamedMod";
        public string logger_level = "Basic";

        public bool ItemSpotNoParticles
        {
            get => Config.ItemSpotNoParticles;
            set => Config.ItemSpotNoParticles = value;
        }
        public int TrackedItemCount
        {
            get => Config.TrackedItemCount;
            set => Config.TrackedItemCount = value >= 0 ? value : 0;
        }
        public float ItemSpotSpawnHeight
        {
            get => Config.ItemSpotSpawnHeight;
            set => Config.ItemSpotSpawnHeight = value >= 0 ? value : 0;
        }
        public float ItemSpotLabelVisibleDistance
        {
            get => Config.ItemSpotLabelVisibleDistance;
            set => Config.ItemSpotLabelVisibleDistance = value >= 0 ? value : 0;
        }
        public float ItemSpotShineVisibleDistance
        {
            get => Config.ItemSpotShineVisibleDistance;
            set => Config.ItemSpotShineVisibleDistance = value >= 0 ? value : 0;
        }
        public float ItemSpotGrabDistance
        {
            get => Config.ItemSpotGrabDistance;
            set => Config.ItemSpotGrabDistance = value >= 0 ? value : 0;
        }
        public float ItemSpotShineScale
        {
            get => Config.ItemSpotShineScale;
            set => Config.ItemSpotShineScale = value >= 0 ? value : 0;
        }
        public float ItemSpotLabelScale
        {
            get => Config.ItemSpotLabelScale;
            set => Config.ItemSpotLabelScale = value >= 0 ? value : 0;
        }
        public float ItemSpotLabelHeight
        {
            get => Config.ItemSpotLabelHeight;
            set => Config.ItemSpotLabelHeight = value >= 0 ? value : 0;
        }
        public float ItemDropRaycastSphereSize
        {
            get => Config.ItemDropRaycastSphereSize;
            set => Config.ItemDropRaycastSphereSize = value >= 0 ? value : 0;
        }
        public float ItemDropRaycastLength
        {
            get => Config.ItemDropRaycastLength;
            set => Config.ItemDropRaycastLength = value >= 0 ? value : 0;
        }

        public IList<String> ItemExclusionList;
        public float[] ShineColorOOR;
        public float[] ShineColorIR;

        public override IEnumerator OnLoadCoroutine()
        {
            Logger.init(mod_name, mod_version, logger_level);

            Logger.Basic("Loading " + mod_name);
            Config.ItemExclusionList = ItemExclusionList.ToHashSet();
            Config.ShineColorOOR[0] = ShineColorOOR[0]; Config.ShineColorOOR[1] = ShineColorOOR[1]; Config.ShineColorOOR[2] = ShineColorOOR[2];
            Config.ShineColorIR[0] = ShineColorIR[0]; Config.ShineColorIR[1] = ShineColorIR[1]; Config.ShineColorIR[2] = ShineColorIR[2];
            EventManager.onLevelLoad += EventManager_onLevelLoad;
            return base.OnLoadCoroutine();
        }

        private void EventManager_onLevelLoad(LevelData levelData, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                ItemTracker tracker;
                if (!Level.current.gameObject.TryGetComponent<ItemTracker>(out tracker))
                {
                    tracker = Level.current.gameObject.AddComponent<ItemTracker>();
                }

                Scavenger scavenger;
                if (!Player.local.TryGetComponent<Scavenger>(out scavenger))
                {
                    scavenger = Player.local.gameObject.AddComponent<Scavenger>();
                }
                scavenger.itemTracker = tracker;
            }
        }

    }
}
