using HarmonyLib;
using MegaMod.Roles;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__11), nameof(IntroCutscene._CoBegin_d__11.MoveNext))]
    class IntroCutscenePatch
    {
        static bool Prefix(IntroCutscene._CoBegin_d__11 __instance)
        {
            GetSpecialRole(PlayerControl.LocalPlayer.PlayerId)?.SetIntro(__instance);
            return true;
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class ImpostorText
    {
        static void Postfix(IntroCutscene __instance)
        {
            if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Role _))
                __instance.ImpostorText.gameObject.SetActive(true);
        }
    }
}
