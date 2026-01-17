using BepInEx.Logging;
using FunkyUI.Classes;
using UnityEngine;
using UnityEngine.UIElements;

namespace FunkyUI.Helpers
{
    public class UIDocumentHelper : MonoBehaviour
    {
        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc != null && doc.rootVisualElement != null)
            {
                doc.rootVisualElement.style.flexGrow = 1;
                doc.rootVisualElement.style.width = Length.Percent(100);
                doc.rootVisualElement.style.height = Length.Percent(100);

                var radialMenu = new RadialMenu();

                radialMenu.style.position = Position.Absolute;
                radialMenu.style.left = Length.Percent(50);
                radialMenu.style.top = Length.Percent(50);
                radialMenu.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50), 0);

                doc.rootVisualElement.Add(radialMenu);

                var controller = gameObject.AddComponent<RadialMenuController>();
                controller.LinkMenu(radialMenu);
                Plugin.LogSource.LogInfo("Finished RadialMenu Initialization.");
                Destroy(this);
            }
        }
    }
}