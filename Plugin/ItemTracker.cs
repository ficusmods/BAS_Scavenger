using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using ThunderRoad;

using System.Text.RegularExpressions;

namespace Scavenger
{
    public class ItemTracker : MonoBehaviour
    {
        private HashSet<ItemSpot> itemSpots;
        private LinkedList<ItemSpot> spotSpawnOrder;
        private HashSet<ItemSpot> grabbableItemSpots;

        private HashSet<ItemData.Type> NonTrackedItemTypes;
        private HashSet<Item> trackedItems;
        private HashSet<string> knownToIgnore;
        private HashSet<string> knownToUse;

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
            EventManager.onLevelLoad += EventManager_onLevelLoad;
            EventManager.onLevelUnload += EventManager_onLevelUnload;
            trackingEnabled = true;
            NonTrackedItemTypes = new HashSet<ItemData.Type> { ItemData.Type.Prop, ItemData.Type.Body, ItemData.Type.Wardrobe, ItemData.Type.Spell, ItemData.Type.Misc };
            itemSpots = new HashSet<ItemSpot>();
            spotSpawnOrder = new LinkedList<ItemSpot>();
            grabbableItemSpots = new HashSet<ItemSpot>();
            trackedItems = new HashSet<Item>();
            knownToIgnore = new HashSet<string>();
            knownToUse = new HashSet<string>();
        }

        private void EventManager_onLevelUnload(LevelData levelData, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart) trackingEnabled = false;
        }

        private void EventManager_onLevelLoad(LevelData levelData, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart) trackingEnabled = false;
            else trackingEnabled = true;
        }

        private void RemoveOldestItemSpot()
        {
            // Need to iterate since the oldest one might have already been removed
            while (true)
            {
                if (spotSpawnOrder.First == null) break;
                ItemSpot currLatestSpot = spotSpawnOrder.First.Value;
                if (itemSpots.Contains(currLatestSpot))
                {
                    spotSpawnOrder.RemoveFirst();
                    itemSpots.Remove(currLatestSpot);
                    grabbableItemSpots.Remove(currLatestSpot);
                    GameObject.Destroy(currLatestSpot.gameObject);
                    break;
                }
                else
                {
                    spotSpawnOrder.RemoveFirst();
                }
            }
        }

        private void AddToUsed(string id)
        {
            Logger.Detailed("{0} added to used ids", id);
            knownToUse.Add(id);
        }

        private void AddToIgnored(string id)
        {
            Logger.Detailed("{0} added to ignored ids", id);
            knownToIgnore.Add(id);
        }

        private bool ItemIncludeTest(Item item)
        {
            if (knownToUse.Contains(item.data.id)) return true;
            else if (knownToIgnore.Contains(item.data.id)) return false;

            if (NonTrackedItemTypes.Contains(item.data.type))
            {
                AddToIgnored(item.data.id);
                return false;
            }

            if (Config.ItemExclusionListUseRegex)
            {
                foreach (Regex rx in Config.ItemExclusionListRegex)
                {
                    if (rx.Match(item.data.id).Success)
                    {
                        AddToIgnored(item.data.id);
                        return false;
                    }
                }
            }
            else
            {
                if (Config.ItemExclusionList.Contains(item.data.id))
                {
                    AddToIgnored(item.data.id);
                    return false;
                }
            }

            AddToUsed(item.data.id);
            return true;
        }

        private void FixedUpdate()
        {
            if (GameManager.isQuitting) trackingEnabled = false;

            if (!trackingEnabled) return;

            while (this.itemSpots.Count > Config.TrackedItemCount)
            {
                RemoveOldestItemSpot();
            }

            foreach (Item item in Item.allActive)
            {
                if (item.data == null) continue;

                if (ItemIncludeTest(item) && !trackedItems.Contains(item))
                {
                    ItemTrackingModule trackingModule = item.gameObject.GetComponent<ItemTrackingModule>();
                    if (trackingModule == null)
                    {
                        trackingModule = item.gameObject.AddComponent<ItemTrackingModule>();
                        trackingModule.onItemDeactivated += TrackingModule_onItemDeactivated;
                    }
                    trackedItems.Add(item);
                }
            }
        }

        private void TrackingModule_onItemDeactivated(Item item)
        {
            trackedItems.Remove(item);
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
