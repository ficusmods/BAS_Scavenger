using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScenarioEditor;
using ThunderRoad;
using UnityEngine;

namespace Scavenger.SE
{
    public class ActionCreateItemSpot : ScenarioEditor.Scene.SESceneActionNode
    {
        public string locationId;
        public string itemId;
        public string bbSpawnedId;

        public ActionCreateItemSpot()
        {
            id = "CreateItemSpot";
        }

        public override void RefreshReferences()
        {
            if (locationId != null && Scene.Scenario.Locations.Find(locationId, out ScenarioEditor.Data.SEDataLocation loc))
            {
                location = loc;
            }

            if (itemId != null)
            {
                itemData = Catalog.GetData<ItemData>(itemId);
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (spawnedItem != null)
            {
                spawnedItem.Despawn();
                spawnedItem = null;
            }

            if (spawnedItemSpot != null)
            {
                GameObject.Destroy(spawnedItemSpot.gameObject);
            }
        }

        protected override NodeState TryStart()
        {
            if (location == null) return NodeState.FAILURE;
            if (itemData == null) return NodeState.FAILURE;

            spawnedItemSpot = ItemSpot.TryCreate(location.pos, itemData);
            if (spawnedItemSpot != null) {
                spawnedItemSpot.onItemSpotItemSpawned += (Item item) =>
                {
                    Scene.Blackboard.UpdateVariable(bbSpawnedId, item.gameObject);
                    spawnedItem = item;
                    spawnedItemSpot = null;
                };

                return NodeState.SUCCESS;
            } else
            {
                return NodeState.FAILURE;
            }
        }

        protected override NodeState Continue()
        {
            return NodeState.SUCCESS;
        }

        protected ScenarioEditor.Data.SEDataLocation location;
        protected ItemData itemData;
        protected ItemSpot spawnedItemSpot;
        protected Item spawnedItem;
    }
}
