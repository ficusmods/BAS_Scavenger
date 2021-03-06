using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using ThunderRoad;


namespace Scavenger
{
    public class ItemTracker : MonoBehaviour
    {
        private HashSet<ItemSpot> itemSpots = new HashSet<ItemSpot>();
        private LinkedList<ItemSpot> spotSpawnOrder = new LinkedList<ItemSpot>();
        private HashSet<ItemSpot> grabbableItemSpots = new HashSet<ItemSpot>();

        HashSet<ItemData.Type> NonTrackedItemTypes = new HashSet<ItemData.Type> { ItemData.Type.Prop, ItemData.Type.Body, ItemData.Type.Wardrobe, ItemData.Type.Spell, ItemData.Type.Misc };

        bool trackingEnabled = false;

        // Key: InstanceId , Value: ItemSpot component's gameobject
        public HashSet<ItemSpot> ItemSpots
        {
            get => itemSpots;
        }

        public HashSet<ItemSpot> GrabbableItemSpots
        {
            get => grabbableItemSpots;
        }

        private void Awake()
        {
            EventManager.onLevelUnload += EventManager_onLevelUnload;
            trackingEnabled = true;
        }

        private void EventManager_onLevelUnload(LevelData levelData, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart) trackingEnabled = false;
        }

        private void RemoveOldestItemSpot()
        {
            bool flag = false;
            // Need to iterate since the oldest one might have already been removed
            while (!flag)
            {
                if (spotSpawnOrder.First == null) break;
                ItemSpot currLatestSpot = spotSpawnOrder.First.Value;
                if (itemSpots.Contains(currLatestSpot))
                {
                    flag = true;
                    itemSpots.Remove(currLatestSpot);
                    grabbableItemSpots.Remove(currLatestSpot);
                    GameObject.Destroy(currLatestSpot.gameObject);
                }
                spotSpawnOrder.RemoveFirst();
            }
        }

        private void Update()
        {
            if (GameManager.isQuitting) trackingEnabled = false;
            if (!trackingEnabled) return;

            while (this.itemSpots.Count > Config.TrackedItemCount)
            {
                RemoveOldestItemSpot();
            }

            foreach (Item item in Item.allActive)
            {
                if (!NonTrackedItemTypes.Contains(item.data.type) && !Config.ItemExclusionList.Contains(item.data.id))
                {
                    ItemTrackingModule trackingModule = item.gameObject.GetComponent<ItemTrackingModule>();
                    if (trackingModule == null)
                    {
                        trackingModule = item.gameObject.AddComponent<ItemTrackingModule>();
                    }
                    trackingModule.onItemDeactivated -= TrackingModule_onItemDeactivated;
                    trackingModule.onItemDeactivated += TrackingModule_onItemDeactivated;
                }
            }
        }

        private void TrackingModule_onItemDeactivated(Item item)
        {
            if (!trackingEnabled) return;
            if (Config.TrackedItemCount == 0) return;

            Logger.Detailed("Trying to create item spot at: ({0},{1},{2}) ({3})", item.transform.position.x, item.transform.position.y, item.transform.position.z, item.data.id);
            ItemSpot ispot = ItemSpot.TryCreate(item.transform.position, item.data);
            if (ispot != null)
            {
                ispot.onItemSpotGrabStatusChange += (ItemSpot spot, bool grabbable) =>
                {
                    if (grabbable) grabbableItemSpots.Add(spot);
                    else grabbableItemSpots.Remove(spot);
                };

                ispot.onItemSpotItemSpawned += (ItemSpot spot) =>
                {
                    grabbableItemSpots.Remove(spot);
                    itemSpots.Remove(spot);
                    GameObject.Destroy(spot.gameObject);
                    Logger.Detailed("Deleted item spot: {0} ({1}, {2})", spot.spotName, spot.itemData.id, spot.GetInstanceID());
                };

                Logger.Detailed("Created item spot: {0} ({1}, {2})", ispot.spotName, ispot.itemData.id, ispot.GetInstanceID());
                itemSpots.Add(ispot);
                spotSpawnOrder.AddLast(ispot);
            }
        }

        public void Unload()
        {
            Logger.Detailed("ItemTracker unloading...");
            spotSpawnOrder.Clear();
            grabbableItemSpots.Clear();
            foreach(ItemSpot spot in itemSpots)
            {
                GameObject.Destroy(spot);
            }
            itemSpots.Clear();

            foreach (Item item in Item.allActive)
            {
                if (!NonTrackedItemTypes.Contains(item.data.type) && !Config.ItemExclusionList.Contains(item.data.id))
                {
                    ItemTrackingModule trackingModule;
                    if (Level.current.gameObject.TryGetComponent<ItemTrackingModule>(out trackingModule))
                    {
                        GameObject.Destroy(trackingModule);
                    }
                }
            }
        }
    }
}
