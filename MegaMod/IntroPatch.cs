using HarmonyLib;
using System;
using Il2CppSystem.Collections.Generic;
using MegaMod.Roles;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePatch
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
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
