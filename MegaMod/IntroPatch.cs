using HarmonyLib;
using System;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer == Jester.player)
            {
                var jokerTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jokerTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jokerTeam;
                return true;
            }
            return true;
        }
    }
}
