using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScenarioEditor;
using ThunderRoad;

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
        }

        protected override NodeState TryStart()
        {
            if (location == null) return NodeState.FAILURE;
            if (itemData == null) return NodeState.FAILURE;

            var spot = ItemSpot.TryCreate(location.pos, itemData);
            if (spot != null) {
                spot.onItemSpotItemSpawned += (Item item) =>
                {
                    Scene.Blackboard.UpdateVariable(bbSpawnedId, item.gameObject);
                    spawnedItem = item;
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
        protected Item spawnedItem;
    }
}
