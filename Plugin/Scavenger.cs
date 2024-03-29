﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;
using UnityEngine.UI;

namespace Scavenger
{
    public class Scavenger : MonoBehaviour
    {
        public ItemTracker itemTracker;

        Dictionary<Side, bool> lastFrameGrabbing = new Dictionary<Side, bool>()
        {
            { Side.Left, false },
            { Side.Right, false }
        };

        private void UpdateHand(Side side)
        {
            PlayerHand playerHand = Player.local.GetHand(side);

            if (!lastFrameGrabbing[side]
                && playerHand.controlHand.gripPressed
                && !PlayerControl.chainButtonPressed
                && !playerHand.ragdollHand.caster.isFiring
                && !playerHand.ragdollHand.grabbedHandle)
            {
                ItemSpot activeItemSpot = null;
                float minDist = float.MaxValue;
                Vector3 handPos = playerHand.transform.position;

                foreach (ItemSpot itemSpot in ItemSpot.GrabbableItemSpots)
                {
                    float currDist = Vector3.Distance(itemSpot.transform.position, handPos);
                    if (currDist < minDist)
                    {
                        activeItemSpot = itemSpot;
                        minDist = currDist;
                    }
                }

                if (activeItemSpot != null)
                {
                    Logger.Detailed("Spawning item at spot: {0} ({1}, {2})", activeItemSpot.spotName, activeItemSpot.itemData.id, activeItemSpot.GetInstanceID());
                    activeItemSpot.SpawnItem(item =>
                    {
                        if (item != null)
                        {
                            Physics.IgnoreCollision(playerHand.ragdollHand.touchCollider, item.GetMainHandle(side).touchCollider, true);
                            item.transform.MoveAlign(item.GetDefaultHolderPoint().anchor, playerHand.ragdollHand.transform);
                            Handle handle = item.GetMainHandle(side);
                            if (handle != null)
                            {
                                playerHand.ragdollHand.Grab(handle);
                            }
                        }
                    });
                    playerHand.ragdollHand.caster.telekinesis.TryRelease();
                }
            }

            lastFrameGrabbing[side] = playerHand.controlHand.gripPressed;
        }

        private void FixedUpdate()
        {
            if (!Player.local || !Player.local.creature) return;
            UpdateHand(Side.Left);
            UpdateHand(Side.Right);
        }
    }
}
