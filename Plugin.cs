using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.Visual;
using FunkyUI.Classes;
using FunkyUI.Helpers;
using FunkyUI.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;

namespace FunkyUI
{
    // first string below is your plugin's GUID, it MUST be unique to any other mod. Read more about it in BepInEx docs. Be sure to update it if you copy this project.
    [BepInPlugin("ajdj100.funkyui", "FunkyUI", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static Plugin Instance;
        public static GameObject UIContainer;

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            LogSource = Logger;
            Instance = this;

            new CommonUIAwakePatch().Enable();
        }

        public void InjectUI(EftBattleUIScreen battleUI)
        {
            if (battleUI == null) return;
            if (UIContainer != null) return;

            LogSource.LogInfo("FunkyUI: Injecting Radial Menu...");

            GameObject uiLayer = new GameObject("FunkyUI_Radial_Container");
            UIContainer = uiLayer;
            uiLayer.transform.SetParent(battleUI.transform, false);

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.fallbackDpi = 96;

            var document = uiLayer.AddComponent<UIDocument>();
            document.panelSettings = settings;
            document.sortingOrder = 100;

            //this adds the thing then kills itself
            uiLayer.AddComponent<UIDocumentHelper>();
        }

        //THIS IS ALL DEBUG, DONT INCLUDE IN RELEASE
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LogSource.LogInfo("Attempting to open menu");
                //string iconFolderPath = Path.Combine(Application.dataPath, "fakemod", "icons");

                //Texture2D tex1 = ModIconLoader.LoadPng(Path.Combine(iconFolderPath, "icon1.png"));
                //Texture2D tex2 = ModIconLoader.LoadPng(Path.Combine(iconFolderPath, "icon2.png"));

                //RadialMenuIcon iconA = tex1 != null ? new RadialMenuIcon(tex1) : null;
                //RadialMenuIcon iconB = tex2 != null ? new RadialMenuIcon(tex2) : null;
                var MenuController = RadialMenuController.Instance;
                if (MenuController == null)
                {
                    LogSource.LogWarning("MenuController is null!!!");
                    return;
                }

                GameObject container = UIContainer;
                if (container == null)
                {
                    LogSource.LogWarning("UI Container was destroyed");
                    return;
                }

                var doc = container.GetComponent<UIDocument>();
                if (doc == null || doc.rootVisualElement == null)
                {
                    LogSource.LogError("UIDocument or RootVisualElement is null");
                    return;
                }

                if (!doc.rootVisualElement.Contains(MenuController.Menu))
                {
                    LogSource.LogWarning("RadialMenu was detached from Root");

                    doc.rootVisualElement.style.width = Length.Percent(100);
                    doc.rootVisualElement.style.height = Length.Percent(100);
                    doc.rootVisualElement.style.flexGrow = 1;

                    var radialMenu = MenuController.Menu;
                    radialMenu.style.position = Position.Absolute;
                    radialMenu.style.left = Length.Percent(50);
                    radialMenu.style.top = Length.Percent(50);

                    doc.rootVisualElement.Add(radialMenu);
                }

                var option = new RadialMenuItem[]
                {
                new RadialMenuItem {
                    Name = "Use",
                    Description = "Consume the selected item immediately.",
                    //Icon = iconA,
                    Callback = () => LogSource.LogInfo("Action: Use")
                },
                new RadialMenuItem {
                    Name = "Inspect",
                    Description = "Examine item details and durability.",
                    //Icon = iconB,
                    Callback = () => LogSource.LogInfo("Action: Inspect")
                },
                new RadialMenuItem {
                    Name = "Drop",
                    Description = "Discard item on the ground for teammates.",
                    //Icon = iconA,
                    Callback = () => LogSource.LogInfo("Action: Drop")
                },
                    new RadialMenuItem {
                    Name = "Use",
                    Description = "Consume the selected item immediately.",
                    //Icon = iconA,
                    Callback = () => LogSource.LogInfo("Action: Use")
                },
                new RadialMenuItem {
                    Name = "Inspect",
                    Description = "Examine item details and durability.",
                    //Icon = iconB,
                    Callback = () => LogSource.LogInfo("Action: Inspect")
                },
                new RadialMenuItem {
                    Name = "Inspect",
                    Description = "Examine item details and durability.",
                    //Icon = iconB,
                    Callback = () => LogSource.LogInfo("Action: Inspect")
                },
                new RadialMenuItem {
                    Name = "Drop",
                    Description = "Discard item on the ground for teammates.",
                    //Icon = iconA,
                    Callback = () => LogSource.LogInfo("Action: Drop")
                },
                    new RadialMenuItem {
                    Name = "Use",
                    Description = "Consume the selected item immediately.",
                    //Icon = iconA,
                    Callback = () => LogSource.LogInfo("Action: Use")
                },
                new RadialMenuItem {
                    Name = "Inspect",
                    Description = "Examine item details and durability.",
                    //Icon = iconB,
                    Callback = () => LogSource.LogInfo("Action: Inspect")
                },
                new RadialMenuItem {
                    Name = "Drop",
                    Description = "Discard item on the ground for teammates.",
                    //Icon = iconA,
                    Callback = () => LogSource.LogInfo("Action: Drop")
                }

                };
                MenuController.Show(option);
            }

        }

    }

}
