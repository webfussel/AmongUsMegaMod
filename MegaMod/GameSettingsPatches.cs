using HarmonyLib;

namespace MegaMod
{
    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class GameSettingsPatch {
        static void Postfix(HudManager __instance)
        {
            __instance.GameSettings.m_fontScale = 0.43f;
        }
    }
    
        
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class Update {
        static void Postfix(ref GameOptionsMenu __instance) {
            __instance.GetComponentInParent<Scroller>().YBounds.max = 16f;
        }
    }
}