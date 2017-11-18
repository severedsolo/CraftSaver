using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace CraftSaver
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR,GameScenes.SPACECENTER)]
    class CraftLoader : ScenarioModule
    {
        string settingsLocation = KSPUtil.ApplicationRootPath + "/GameData/CraftSaver/PluginData/Settings.cfg";
        public override void OnLoad(ConfigNode persistent)
        {
            ConfigNode[] crafts = persistent.GetNodes("CRAFT");
            if(crafts.Count() >0)
            {
                CraftSaver.instance.alwaysSave.Clear();
                for(int i = 0; i<crafts.Count();i++)
                {
                    ConfigNode node = crafts.ElementAt(i);
                    CraftSaver.instance.alwaysSave.Add(node.GetValue("name"));
                }
            }
            if(File.Exists(settingsLocation))
            {
                CraftSaver.instance.savedPath = File.ReadAllText(settingsLocation);
            }
            else
            {
                CraftSaver.instance.savedPath = KSPUtil.ApplicationRootPath + "/GameData/CraftSaver/PluginData/SavedCraft/";
            }
            string VABString = CraftSaver.instance.savedPath + "/VAB";
            string SPHString = CraftSaver.instance.savedPath + "/SPH";
            List<string> VABList = new List<string>();
            if (Directory.Exists(VABString)) VABList = Directory.GetFiles(VABString).ToList();
            List<string> SPHList = new List<string>();
            if (Directory.Exists(SPHString)) SPHList = Directory.GetFiles(SPHString).ToList();
            if (VABList.Count() >0)
            {
                for(int i = 0; i<VABList.Count(); i++)
                {
                    string s = VABList.ElementAt(i);
                    string substring = s.Substring(VABString.Length + 1);
                    if (File.Exists(CraftSaver.instance.VABPath + substring))
                    {
                        if (CraftSaver.instance.alwaysSave.Contains(substring)) File.Copy(CraftSaver.instance.VABPath + substring, s, true);
                    }
                    if (!File.Exists(CraftSaver.instance.VABPath + substring))
                    {
                        File.Copy(s, CraftSaver.instance.VABPath + substring);
                        Debug.Log("[CraftSaver] Restored backup copy of " + s);
                    }
                }
            }
            if (SPHList.Count() > 0)
            {
                for (int i = 0; i < SPHList.Count(); i++)
                {
                    string s = SPHList.ElementAt(i);
                    string substring = s.Substring(VABString.Length + 1);
                    if (File.Exists(CraftSaver.instance.SPHPath + substring))
                    {
                        if (CraftSaver.instance.alwaysSave.Contains(substring)) File.Copy(CraftSaver.instance.SPHPath + substring, s, true);
                    }
                    if (!File.Exists(CraftSaver.instance.SPHPath + substring))
                    {
                        File.Copy(s, CraftSaver.instance.SPHPath + substring);
                        Debug.Log("[CraftSaver]: Restored backup copy of " + s);
                    }
                }
            }
        }
        public override void OnSave(ConfigNode persistent)
        {
            File.WriteAllText(settingsLocation, CraftSaver.instance.savedPath);
            if (CraftSaver.instance.alwaysSave.Count() == 0) return;
            for(int i = 0; i<CraftSaver.instance.alwaysSave.Count(); i++)
            {
                ConfigNode node = new ConfigNode("CRAFT");
                node.SetValue("name", CraftSaver.instance.alwaysSave.ElementAt(i), true);
                persistent.AddNode(node);
            }
        }
    }
}
