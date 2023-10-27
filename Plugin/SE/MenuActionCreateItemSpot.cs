using ScenarioEditor;
using ScenarioEditor.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

namespace Scavenger.SE
{
    public class MenuActionCreateItemSpot : SELazyMenu
    {
        protected override IEnumerator InitLazyMenuCoroutine()
        {
            StartHorizontalGroup();
            selectorItemType = AddSelector("Select category");
            selectorItemType.onOptionSelected += SelectorCategory_onOptionSelected;

            selectorItem = AddSelector("Select item");
            selectorItem.onOptionSelected += SelectorItem_onOptionSelected;
            EndHorizontalGroup();

            StartVerticalGroup();
            inputBBItemId = AddInputField("BB id:", (text) => {
                (NodeEditorManager.NowEditing as ActionCreateItemSpot).bbSpawnedId = text;
            });
            EndVerticalGroup();

            StartVerticalGroup();
            AddLabel("AT");
            selectorLocation = AddSelector("Select location");
            selectorLocation.onOptionSelected += SelectorLocation_onOptionSelected;
            EndVerticalGroup();

            StartVerticalGroup();
            labelItemId = AddLabel();
            labelLocationId = AddLabel();
            EndVerticalGroup();


            yield return new WaitForEndOfFrame();
        }

        private void SelectorCategory_onOptionSelected(string selected)
        {
            Enum.TryParse(selected, out selectedItemType);
            Refresh();
        }

        private void SelectorItem_onOptionSelected(string selected)
        {
            var createSpot = (NodeEditorManager.NowEditing as ActionCreateItemSpot);
            createSpot.itemId = selected;
            UpdateLabels();
        }

        private void SelectorLocation_onOptionSelected(string selected)
        {
            var createSpot = (NodeEditorManager.NowEditing as ActionCreateItemSpot);
            createSpot.locationId = selected;
            UpdateLabels();
        }

        protected virtual void UpdateLabels()
        {
            var createSpot = (NodeEditorManager.NowEditing as ActionCreateItemSpot);
            labelLocationId.text = createSpot.locationId;
            labelItemId.text = createSpot.itemId;
        }

        public override void Refresh()
        {
            ActionCreateItemSpot actionNode = (NodeEditorManager.NowEditing as ActionCreateItemSpot);

            selectorItemType.RefreshContent(Enum.GetNames(typeof(ItemData.Type)));
            selectorLocation.RefreshContent(actionNode.Scene.Scenario.Locations.Select((entry) => entry.Key));
            selectorItem.RefreshContent(OrderedCatalog.GetDataList_IDOrder<ItemData>()
                .Where((item) => item.type == selectedItemType)
                .Select((table) => table.id));
            UpdateLabels();
            inputBBItemId.text = actionNode.bbSpawnedId;
        }


        protected ContentSelectorElement selectorItemType;
        protected ContentSelectorElement selectorItem;
        protected ContentSelectorElement selectorLocation;

        protected InputField inputBBItemId;

        protected Text labelLocationId;
        protected Text labelItemId;

        protected ItemData.Type selectedItemType = ItemData.Type.Prop;
    }
}
