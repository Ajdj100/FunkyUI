using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FunkyUI.Patches
{
    public class CommonUIAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // one way methods can be patched is by targeting both their class name and the name of the method itself
            // the example in this patch is the Jump() method in the Player class
            return AccessTools.Method(typeof(CommonUI), nameof(CommonUI.Awake));
        }

        [PatchPostfix]
        public static void Postfix()
        {
            var ui = Singleton<CommonUI>.Instance.EftBattleUIScreen;
            if (ui == null)
            {
                Plugin.LogSource.LogError("Battle Screen doesnt exist, abandoning");
                return;
            }
            Plugin.Instance.InjectUI(ui);
            Plugin.LogSource.LogError("Injecting UI");

        }
    }
}
