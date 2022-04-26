﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using ThunderRoad;

namespace Scavenger
{
    public class ItemSpot : MonoBehaviour
    {
        public ItemData itemData;
        public GameObject spotObject;
        public string spotName = "ItemSpot_Unknown";
        public bool spawned = false;

        private Text labelText;
        private ParticleSystem shineParticleSystem;
        private ParticleSystemRenderer shineParticleRenderer;

        bool _grabbable;
        public bool CanGrab
        {
            get => _grabbable;
        }

        public delegate void ItemSpotGrabStatusChange(ItemSpot spot, bool grabbable);
        public event ItemSpotGrabStatusChange onItemSpotGrabStatusChange;
        public delegate void ItemSpotOnItemSpawned(ItemSpot spot);
        public event ItemSpotOnItemSpawned onItemSpotItemSpawned;

        public static ItemSpot TryCreate(Vector3 position, ItemData idata)
        {
            string name = "ItemSpot_" + idata.displayName;
            ItemSpot comp = null;

            RaycastHit rcHit;
            if (Physics.SphereCast(new Ray(position + new Vector3(0.0f, 1.0f, 0.0f), Vector3.down), Config.ItemDropRaycastSphereSize, out rcHit, Config.ItemDropRaycastLength + 1.0f, GameManager.local.groundLayer, QueryTriggerInteraction.Ignore))
            {
                GameObject itemSpot = new GameObject(name);
                itemSpot.transform.position = rcHit.point;
                itemSpot.transform.position += new Vector3(0.0f, Config.ItemSpotSpawnHeight - Config.ItemDropRaycastSphereSize / 2.0f, 0.0f);
                comp = itemSpot.AddComponent<ItemSpot>();
                comp.itemData = idata;
                comp.spotName = name;
                comp.Spawn();
                Logger.Detailed("Ground hit, spawning item spot at: ({0},{1},{2})", rcHit.point.x, rcHit.point.y, rcHit.point.z);
            }
            else
            {
                Logger.Detailed("Raycast didn't hit the ground, can't spawn item spot");
            }

            return comp;
        }

        public void Spawn()
        {
            Catalog.InstantiateAsync<GameObject>("fks.scavenger.ItemLabel",
                (GameObject obj) =>
                {
                    spotObject = obj;
                    spotObject.transform.SetParent(this.gameObject.transform, false);
                    labelText = spotObject.GetComponentInChildren<Text>();
                    labelText.text = itemData.displayName;
                    labelText.transform.localScale = new Vector3(Config.ItemSpotLabelScale, Config.ItemSpotLabelScale, Config.ItemSpotLabelScale);
                    labelText.transform.localPosition = new Vector3(0.0f, Config.ItemSpotLabelHeight, 0.0f);
                    labelText.gameObject.SetActive(false);
                    
                    shineParticleSystem = spotObject.GetComponentInChildren<ParticleSystem>();
                    shineParticleRenderer = shineParticleSystem.GetComponent<ParticleSystemRenderer>();
                    shineParticleSystem.gameObject.SetActive(false);
                    spawned = true;
                }, "Label");
        }

        public void SpawnItem(Action<Item> callback)
        {
            itemData.SpawnAsync(callback);
            if(onItemSpotItemSpawned != null)
            {
                onItemSpotItemSpawned(this);
            }
        }

        private void Update()
        {
            if (!spawned) return;

            PlayerHand leftHand = Player.local.handLeft;
            PlayerHand rightHand = Player.local.handRight;
            float leftDist = Vector3.Distance(leftHand.transform.position, gameObject.transform.position);
            float rightDist = Vector3.Distance(rightHand.transform.position, gameObject.transform.position);
            float dist = Math.Min(leftDist, rightDist);

            bool changeFlag = true;

            if (dist <= Config.ItemSpotLabelVisibleDistance)
            {
                labelText.gameObject.SetActive(true);
                labelText.transform.LookAt(Camera.main.transform.position);
                labelText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }
            else
            {
                labelText.gameObject.SetActive(false);
            }

            if (dist <= Config.ItemSpotShineVisibleDistance)
            {
                shineParticleSystem.gameObject.SetActive(true);
                shineParticleSystem.Play();
            }
            else
            {
                shineParticleSystem.Stop();
                shineParticleSystem.gameObject.SetActive(false);
            }

            if (dist <= Config.ItemSpotGrabDistance)
            {
                if (!_grabbable) changeFlag = true;
                _grabbable = true;
                labelText.color = new Color(Config.ShineColorIR[0], Config.ShineColorIR[1], Config.ShineColorIR[2]);
                shineParticleRenderer.material.color = new Color(Config.ShineColorIR[0], Config.ShineColorIR[1], Config.ShineColorIR[2]);
            }
            else
            {
                if (_grabbable) changeFlag = true;
                _grabbable = false;
                labelText.color = new Color(Config.ShineColorOOR[0], Config.ShineColorOOR[1], Config.ShineColorOOR[2]);
                shineParticleRenderer.material.color = new Color(Config.ShineColorOOR[0], Config.ShineColorOOR[1], Config.ShineColorOOR[2]);
            }

            if (changeFlag)
            {
                if (onItemSpotGrabStatusChange != null)
                {
                    onItemSpotGrabStatusChange(this, _grabbable);
                }
            }
        }
    }
}
