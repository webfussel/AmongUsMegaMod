using HarmonyLib;

namespace MegaMod
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
    public class GameSettingsPatches
    {
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        class Update {
            static void Postfix(ref GameOptionsMenu __instance) {
                __instance.GetComponentInParent<Scroller>().YBounds.max = 16f;
            }
        }
    }
}