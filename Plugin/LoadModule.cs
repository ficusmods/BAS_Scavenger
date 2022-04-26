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

        public uint TrackedItemCount
        {
            get => Config.TrackedItemCount;
            set => Config.TrackedItemCount = value;
        }
        public float ItemSpotSpawnHeight
        {
            get => Config.ItemSpotSpawnHeight;
            set => Config.ItemSpotSpawnHeight = value;
        }
        public float ItemSpotLabelVisibleDistance
        {
            get => Config.ItemSpotLabelVisibleDistance;
            set => Config.ItemSpotLabelVisibleDistance = value;
        }
        public float ItemSpotShineVisibleDistance
        {
            get => Config.ItemSpotShineVisibleDistance;
            set => Config.ItemSpotShineVisibleDistance = value;
        }
        public float ItemSpotGrabDistance
        {
            get => Config.ItemSpotGrabDistance;
            set => Config.ItemSpotGrabDistance = value;
        }
        public float ItemSpotLabelScale
        {
            get => Config.ItemSpotLabelScale;
            set => Config.ItemSpotLabelScale = value;
        }
        public float ItemSpotLabelHeight
        {
            get => Config.ItemSpotLabelHeight;
            set => Config.ItemSpotLabelHeight = value;
        }
        public float ItemDropRaycastSphereSize
        {
            get => Config.ItemDropRaycastSphereSize;
            set => Config.ItemDropRaycastSphereSize = value;
        }
        public float ItemDropRaycastLength
        {
            get => Config.ItemDropRaycastLength;
            set => Config.ItemDropRaycastLength = value;
        }
        public float[] ShineColorOOR
        {
            get => Config.ShineColorOOR;
            set => Config.ShineColorOOR = value;
        }
        public float[] ShineColorIR
        {
            get => Config.ShineColorIR;
            set => Config.ShineColorIR = value;
        }

        public override IEnumerator OnLoadCoroutine()
        {
            Logger.init(mod_name, mod_version, logger_level);

            Logger.Basic("Loading " + mod_name);
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
