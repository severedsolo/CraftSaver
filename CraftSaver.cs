using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using System.IO;

namespace CraftSaver
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class CraftSaver : MonoBehaviour
    {
        public static CraftSaver instance;
        public string savedPath = KSPUtil.ApplicationRootPath + "/GameData/CraftSaver/PluginData/SavedCraft/";
        public string VABPath = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/Ships/VAB/";
        public string SPHPath = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/Ships/SPH/";
        public List<string> alwaysSave = new List<string>();
        bool alwaysSaveToggle = false;
        bool showGUI = false;
        bool askToConfirm = false;
        bool confirmed = false;
        bool promptToSave = false;
        ApplicationLauncherButton ToolbarButton;
        Rect Window = new Rect(20, 100, 480, 50);
        bool showCrafts = false;
        Vector2 scrollPosition1;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            instance = this;
        }

        private void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Add(SceneSwitch);
        }

        private void SceneSwitch(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            showGUI = false;
        }

        private void GUIReady()
        {
            confirmed = false;
            if (HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            if (ToolbarButton == null)
            {
                ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GUISwitch, GUISwitch, null, null, null, null, ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH, GameDatabase.Instance.GetTexture("CraftSaver/Icon", false));
            }
        }

        void GUISwitch()
        {
            showGUI = !showGUI;
        }

        private void OnGUI()
        {
            if (!showGUI) return;
            Window = GUILayout.Window(55901837, Window, GUIDisplay, "CraftSaver", GUILayout.Width(480));
        }
        void GUIDisplay(int windowID)
        { 
            GUILayout.Label("Saved Craft Directory");
            savedPath = GUILayout.TextField(savedPath);
            if (GUILayout.Button("View Saved Crafts")) showCrafts = !showCrafts;
            if(showCrafts)
            {
                scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Width(480), GUILayout.Height(200));
                List<string> vabCrafts = new List<string>();
                List<string> sphCrafts = new List<string>();
                try
                {
                    vabCrafts = Directory.GetFiles(savedPath + "/VAB").ToList();
                    sphCrafts = Directory.GetFiles(savedPath + "/SPH").ToList();
                }
                catch
                {
                    vabCrafts = new List<string>();
                    sphCrafts = new List<string>();
                }
                if(vabCrafts.Count()>0)
                {
                    for(int i = 0; i<vabCrafts.Count(); i++)
                    {
                        GUILayout.Label("Name: " + vabCrafts.ElementAt(i));
                        if (GUILayout.Button("Delete"))
                        {
                            File.Delete(vabCrafts.ElementAt(i));
                            alwaysSave.Remove(vabCrafts.ElementAt(i));
                        }
                    }
                }
                if (sphCrafts.Count() > 0)
                {
                    for (int i = 0; i < sphCrafts.Count(); i++)
                    {
                        GUILayout.Label("Name: " + sphCrafts.ElementAt(i));
                        if (GUILayout.Button("Delete"))
                        {
                            File.Delete(sphCrafts.ElementAt(i));
                            alwaysSave.Remove(sphCrafts.ElementAt(i));
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("Save Craft"))
            {
                confirmed = false;
                string sourcePath;
                if (EditorLogic.fetch.ship.shipFacility == EditorFacility.VAB) sourcePath = VABPath + EditorLogic.fetch.ship.shipName + ".craft";
                else sourcePath = SPHPath + EditorLogic.fetch.ship.shipName + ".craft";
                string destPath;
                if (EditorLogic.fetch.ship.shipFacility == EditorFacility.VAB) destPath = savedPath + "/VAB/" + EditorLogic.fetch.ship.shipName + ".craft";
                else destPath = savedPath + "/SPH/" + EditorLogic.fetch.ship.shipName + ".craft";
                Directory.CreateDirectory(savedPath + "/VAB");
                Directory.CreateDirectory(savedPath + "/SPH");
                if(!File.Exists(sourcePath))
                {
                    promptToSave = true;
                    return;
                }
                if (!File.Exists(destPath))
                {
                    SaveCraft();
                }
                else askToConfirm = true;
            }
            alwaysSaveToggle = GUILayout.Toggle(alwaysSaveToggle, "Save all future versions of this craft");
            if(askToConfirm)
            {
                GUILayout.Label("Craft File Already Exists, do you want to overwrite?");
                if(GUILayout.Button("Yes"))
                {
                    SaveCraft();
                }
                if(GUILayout.Button("No"))
                {
                    askToConfirm = false;
                }
            }
            else if(confirmed)
            {
                GUILayout.Label("Craft saved successfully");
            }
            if(promptToSave)
            {
                GUILayout.Label("Save your craft first");
            }
            
            GUI.DragWindow();
        }
        void SaveCraft()
        {
            string sourcePath;
            if (EditorLogic.fetch.ship.shipFacility == EditorFacility.VAB) sourcePath = VABPath + EditorLogic.fetch.ship.shipName + ".craft";
            else sourcePath = SPHPath + EditorLogic.fetch.ship.shipName + ".craft";
            string destPath;
            if (EditorLogic.fetch.ship.shipFacility == EditorFacility.VAB) destPath = savedPath + "/VAB/" + EditorLogic.fetch.ship.shipName + ".craft";
            else destPath = savedPath + "/SPH/" + EditorLogic.fetch.ship.shipName + ".craft";
            Directory.CreateDirectory(savedPath + "/VAB");
            Directory.CreateDirectory(savedPath + "/SPH");
            File.Copy(sourcePath, destPath, true);
            if (alwaysSaveToggle && !alwaysSave.Contains(EditorLogic.fetch.ship.shipName + ".craft")) alwaysSave.Add(EditorLogic.fetch.ship.shipName + ".craft");
            Debug.Log("[CraftSaver]: Saved " + EditorLogic.fetch.ship.shipName);
            askToConfirm = false;
            confirmed = true;
            promptToSave = false;
            alwaysSaveToggle = false;
        }
        private void OnDestroy()
        {
            if (ToolbarButton != null) ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
            showGUI = false;
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
        }
    }
}
