using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using ThunderRoad;

namespace Scavenger
{
    public class ItemTrackingModule : MonoBehaviour
    {
        Item item;

        public delegate void ItemActivated(Item item);
        public event ItemActivated onItemActivated;
        public delegate void ItemDeactivated(Item item);
        public event ItemDeactivated onItemDeactivated;

        private void Awake()
        {
            item = GetComponent<Item>();
        }

        private void OnDisable()
        {
            if (onItemDeactivated != null) onItemDeactivated(item);
        }

        private void OnEnable()
        {
            if (onItemActivated != null) onItemActivated(item);

        }
    }
}
