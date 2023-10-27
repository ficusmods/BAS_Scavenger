using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ThunderRoad;
using UnityEngine;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Scavenger
{
    public class LoadModule : ThunderScript
    {

        public string mod_version = "1.4";
        public string mod_name = "Scavenger";
        public string logger_level = "Basic";

        public List<String> ItemExclusionList;

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            Logger.init(mod_name, mod_version, logger_level);

            Logger.Basic("Loading " + mod_name);
            LoadItemExclusionList();
            CompileItemExclusionList();
            EventManager.onLevelLoad += EventManager_onLevelLoad;
        }

        private void LoadItemExclusionList()
        {
            ItemExclusionList = JsonConvert.DeserializeObject<List<String>>(File.ReadAllText(
                FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, "Scavenger/exclude.json")
                ));
        }

        private void CompileItemExclusionList()
        {
            foreach (String entry in ItemExclusionList)
            {
                string trimmed = entry.Trim();
                if (trimmed.Length < 1) continue;
                if(Config.ItemExclusionListUseRegex)
                {
                    try
                    {
                        Regex.Match("", trimmed);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    Config.ItemExclusionListRegex.Add(new Regex(trimmed, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                else
                {
                    Config.ItemExclusionList.Add(entry);
                }
            }
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
