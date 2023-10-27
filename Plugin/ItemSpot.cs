using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using ThunderRoad;
using UnityEngine.Windows;

namespace Scavenger
{
    public class ItemSpot : MonoBehaviour
    {
        public ItemData itemData;
        public GameObject spotObject;
        public string spotName = "ItemSpot_Unknown";
        public bool spawned = false;
        
        public static HashSet<ItemSpot> GrabbableItemSpots
        {
            get => grabbableItemSpots;
        }

        private static HashSet<ItemSpot> grabbableItemSpots = new HashSet<ItemSpot>();

        private Canvas labelCanvas;
        private Text labelText;


        // Particles version
        private ParticleSystem shineParticleSystem = null;
        private ParticleSystemRenderer shineParticleRenderer = null;
        private ParticleSystem sparkParticleSystem = null;
        private ParticleSystemRenderer sparkParticleRenderer = null;

        // No particles version
        private GameObject meshObject = null;
        private MeshRenderer meshRenderer = null;

        private bool noParticles = false;

        bool canGrab;
        public bool CanGrab
        {
            get => canGrab;
        }

        public delegate void ItemSpotGrabStatusChange(ItemSpot spot, bool grabbable);
        public event ItemSpotGrabStatusChange onItemSpotGrabStatusChange;
        public delegate void ItemSpotOnItemSpawned(Item item);
        public event ItemSpotOnItemSpawned onItemSpotItemSpawned;

        public static ItemSpot TryCreate(Vector3 position, ItemData idata)
        {
            string name = "ItemSpot_" + idata.displayName;
            ItemSpot comp = null;

            RaycastHit rcHit;
            if (Physics.SphereCast(
                new Ray(position + new Vector3(0.0f, 0.1f + Config.ItemDropRaycastSphereSize, 0.0f), Vector3.down),
                Config.ItemDropRaycastSphereSize,
                out rcHit,
                Config.ItemDropRaycastLength + 0.1f + Config.ItemDropRaycastSphereSize,
                ThunderRoadSettings.current.groundLayer, QueryTriggerInteraction.Ignore))
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

        private void SpawnWithParticles()
        {
            Catalog.InstantiateAsync("ficus.scavenger.ItemSpot", gameObject.transform.position, gameObject.transform.rotation, gameObject.transform,
                (GameObject obj) =>
                {
                    spotObject = obj;
                    spotObject.transform.SetParent(this.gameObject.transform, false);

                    labelCanvas = spotObject.GetComponentInChildren<Canvas>();
                    labelCanvas.transform.localPosition = new Vector3(0.0f, Config.ItemSpotLabelHeight, 0.0f);

                    labelText = spotObject.GetComponentInChildren<Text>();
                    labelText.text = itemData.displayName;
                    labelText.transform.localScale = new Vector3(Config.ItemSpotLabelScale, Config.ItemSpotLabelScale, Config.ItemSpotLabelScale);
                    labelText.gameObject.SetActive(false);

                    GameObject particleSystemRoot = spotObject.transform.Find("ParticleSystem").gameObject;
                    GameObject shineParticleSystemRoot = particleSystemRoot.transform.Find("ShineParticleSystem").gameObject;
                    GameObject sparkParticleSystemRoot = particleSystemRoot.transform.Find("SparkParticleSystem").gameObject;

                    shineParticleSystem = shineParticleSystemRoot.GetComponentInChildren<ParticleSystem>();
                    var mainShineParticleModule = shineParticleSystem.main;
                    mainShineParticleModule.startSizeX = mainShineParticleModule.startSizeX.constant * Config.ItemSpotShineScale;
                    mainShineParticleModule.startSizeY = mainShineParticleModule.startSizeY.constant * Config.ItemSpotShineScale;
                    mainShineParticleModule.startSizeZ = mainShineParticleModule.startSizeZ.constant * Config.ItemSpotShineScale;
                    shineParticleRenderer = shineParticleSystem.GetComponent<ParticleSystemRenderer>();
                    shineParticleSystem.gameObject.SetActive(false);

                    sparkParticleSystem = sparkParticleSystemRoot.GetComponentInChildren<ParticleSystem>();
                    var mainSparkParticleModule = sparkParticleSystem.main;
                    mainSparkParticleModule.startSize = mainSparkParticleModule.startSize.constant * Config.ItemSpotShineScale;
                    sparkParticleRenderer = sparkParticleSystem.GetComponent<ParticleSystemRenderer>();
                    sparkParticleSystem.gameObject.SetActive(false);

                    spawned = true;
                }, "ItemSpot");
        }
        private void SpawnWithNoParticles()
        {
            Catalog.InstantiateAsync("ficus.scavenger.ItemSpotNoParticles", gameObject.transform.position, gameObject.transform.rotation, gameObject.transform,
                (GameObject obj) =>
                {
                    spotObject = obj;
                    spotObject.transform.SetParent(this.gameObject.transform, false);

                    labelCanvas = spotObject.GetComponentInChildren<Canvas>();
                    labelCanvas.transform.localPosition = new Vector3(0.0f, Config.ItemSpotLabelHeight, 0.0f);

                    labelText = spotObject.GetComponentInChildren<Text>();
                    labelText.text = itemData.displayName;
                    labelText.transform.localScale = new Vector3(Config.ItemSpotLabelScale, Config.ItemSpotLabelScale, Config.ItemSpotLabelScale);
                    labelText.gameObject.SetActive(false);

                    meshObject = spotObject.transform.Find("ShineMesh").gameObject;
                    meshRenderer = meshObject.GetComponent<MeshRenderer>();
                    meshObject.SetActive(false);
                    spawned = true;
                }, "ItemSpot");
        }

        public void Spawn()
        {
            if(Config.ItemSpotNoParticles)
            {
                noParticles = true;
                SpawnWithNoParticles();
            }
            else
            {
                noParticles = false;
                SpawnWithParticles();
            }
        }

        public void SpawnItem(Action<Item> callback)
        {
            itemData.SpawnAsync((Item item) => {
                if (onItemSpotItemSpawned != null)
                {
                    onItemSpotItemSpawned(item);
                }
                callback(item);
                Logger.Detailed("Deleted item spot: {0} ({1}, {2})", spotName, itemData.id, GetInstanceID());
                GameObject.Destroy(gameObject);
            });
        }

        private void UpdateLabelVisibility(float playerDist)
        {
            if (playerDist <= Config.ItemSpotLabelVisibleDistance)
            {
                labelText.gameObject.SetActive(true);
                labelText.transform.LookAt(Camera.main.transform.position);
                labelText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }
            else
            {
                labelText.gameObject.SetActive(false);
            }
        }

        private void UpdateShineVisibility(float playerDist)
        {
            if (playerDist <= Config.ItemSpotShineVisibleDistance)
            {
                if (noParticles)
                {
                    meshObject.SetActive(true);
                }
                else
                {
                    shineParticleSystem.gameObject.SetActive(true);
                    sparkParticleSystem.gameObject.SetActive(true);
                    shineParticleSystem.Play();
                    sparkParticleSystem.Play();
                }
            }
            else
            {
                if (noParticles)
                {
                    meshObject.SetActive(false);
                }
                else
                {
                    shineParticleSystem.Stop();
                    sparkParticleSystem.Stop();
                    shineParticleSystem.gameObject.SetActive(false);
                    sparkParticleSystem.gameObject.SetActive(false);
                }
            }
        }

        private bool UpdateGrabStatus(float playerDist)
        {
            bool changeFlag = false;

            if (playerDist <= Config.ItemSpotGrabDistance)
            {
                if (!canGrab)
                {
                    changeFlag = true;
                    canGrab = true;
                    labelText.color = new Color(Config.ShineColorIR[0], Config.ShineColorIR[1], Config.ShineColorIR[2]);

                    if (noParticles)
                    {
                        meshRenderer.material.color = new Color(Config.ShineColorIR[0], Config.ShineColorIR[1], Config.ShineColorIR[2]);
                    }
                    else
                    {
                        shineParticleRenderer.material.color = new Color(Config.ShineColorIR[0], Config.ShineColorIR[1], Config.ShineColorIR[2]);
                        sparkParticleRenderer.material.color = new Color(Config.ShineColorIR[0], Config.ShineColorIR[1], Config.ShineColorIR[2]);
                    }
                }
            }
            else
            {
                if (canGrab)
                {
                    changeFlag = true;
                    canGrab = false;
                    labelText.color = new Color(Config.ShineColorOOR[0], Config.ShineColorOOR[1], Config.ShineColorOOR[2]);

                    if (noParticles)
                    {
                        meshRenderer.material.color = new Color(Config.ShineColorOOR[0], Config.ShineColorOOR[1], Config.ShineColorOOR[2]);
                    }
                    else
                    {
                        shineParticleRenderer.material.color = new Color(Config.ShineColorOOR[0], Config.ShineColorOOR[1], Config.ShineColorOOR[2]);
                        sparkParticleRenderer.material.color = new Color(Config.ShineColorOOR[0], Config.ShineColorOOR[1], Config.ShineColorOOR[2]);
                    }
                }
            }

            return changeFlag;
        }

        private void OnDestroy()
        {
            grabbableItemSpots.Remove(this);
        }

        private void FixedUpdate()
        {
            if (!spawned) return;
            if (!Player.local || !Player.local.creature) return;
            if (GameManager.isQuitting) return;

            PlayerHand leftHand = Player.local.handLeft;
            PlayerHand rightHand = Player.local.handRight;
            float leftDist = Vector3.Distance(leftHand.transform.position, gameObject.transform.position);
            float rightDist = Vector3.Distance(rightHand.transform.position, gameObject.transform.position);
            float dist = Math.Min(leftDist, rightDist);

            UpdateLabelVisibility(dist);
            UpdateShineVisibility(dist);
            bool changeFlag = UpdateGrabStatus(dist);
            if (changeFlag)
            {
                if (canGrab) grabbableItemSpots.Add(this);
                else grabbableItemSpots.Remove(this);
                if (onItemSpotGrabStatusChange != null)
                {
                    onItemSpotGrabStatusChange(this, canGrab);
                }
            }
        }
    }
}
