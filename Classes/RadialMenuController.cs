using System;
using Comfort.Common;
using EFT.InputSystem;
using EFT.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static EFT.Player;

namespace FunkyUI.Classes
{
    public class RadialMenuController : InputNode
    {
        // Singleton access for other mods
        public static RadialMenuController Instance { get; private set; }

        public RadialMenu Menu { get; private set; }
        private bool _isActive;
        private static InputTree _cachedInputTree;

        public event Action OnClickOutside;
        public event Action<int> OnItemSelected;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GetInputTree().Add(this);
            _isActive = false;
        }

        public void Show(RadialMenuOption[] items)
        {
            var ui = Singleton<CommonUI>.Instance.EftBattleUIScreen;
            if (ui == null)
            {
                Plugin.LogSource.LogError("Battle Screen doesnt exist, abandoning");
                return;
            }

            Plugin.LogSource.LogWarning("Controller attempting to show menu");
            Plugin.LogSource.LogInfo($"Trying to load menu with {items.Length} items");
            if (Menu == null) return;

            var doc = Plugin.UIContainer.GetComponent<UIDocument>();
            if (!doc.rootVisualElement.Contains(Menu))
            {
                doc.rootVisualElement.style.width = Length.Percent(100);
                doc.rootVisualElement.style.height = Length.Percent(100);
                doc.rootVisualElement.style.flexGrow = 1;

                var radialMenu = Menu;
                radialMenu.style.position = Position.Absolute;
                radialMenu.style.left = Length.Percent(50);
                radialMenu.style.top = Length.Percent(50);

                doc.rootVisualElement.Add(radialMenu);
            }

            // Clear old mod data and load new data
            Menu.ClearSubscribers();
            Menu.SetItems(items);

            Menu.OnClickOutside += () =>
            {
                Plugin.LogSource.LogInfo("ClickOutside");
                OnClickOutside?.Invoke();
                Close();
            };
            Menu.OnItemSelected += (id) =>
            {
                Plugin.LogSource.LogInfo("Selected");
                OnItemSelected?.Invoke(id);
                Close();
            };

            _isActive = true;
            Menu.style.display = DisplayStyle.Flex;
            Menu.visible = true;
            Menu.SetVisible(true);
        }

        public void Close()
        {
            _isActive = false;
            Menu.style.display = DisplayStyle.None;
            Menu.visible = false;
        }

        // Logic to link the UI class to this controller
        public void LinkMenu(RadialMenu menu)
        {
            Menu = menu;
        }

        public override ETranslateResult TranslateCommand(ECommand command)
        {
            if (!_isActive) return ETranslateResult.Ignore;
            if (command.IsCommand(ECommand.Escape))
            {
                Close();
                return ETranslateResult.Block;
            }
            return ETranslateResult.Ignore;
        }

        public override void TranslateAxes(ref float[] axes)
        {
            //if (_isActive) axes = null;
            if (_isActive)
            {
                axes[2] = 0;
                axes[3] = 0;
                axes[4] = 0;
                axes[5] = 0;
            }
        }

        public override ECursorResult ShouldLockCursor()
        {
            return _isActive ? ECursorResult.ShowCursor : ECursorResult.Ignore;
        }

        private static InputTree GetInputTree()
        {
            if (_cachedInputTree == null)
            {
                var inputObj = GameObject.Find("___Input") ?? throw new System.NullReferenceException("Could not find ___Input!");
                _cachedInputTree = inputObj.GetComponent<InputTree>();
            }
            return _cachedInputTree;
        }

        private void OnDestroy()
        {
            var tree = GetInputTree();
            if (tree != null) tree.Remove(this);

            if (Instance == this) Instance = null;
        }
    }
}