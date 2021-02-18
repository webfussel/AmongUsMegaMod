using HarmonyLib;
using System;
using Il2CppSystem.Collections.Generic;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
        {
            Jester local = GetSpecialRole<Jester>(PlayerControl.LocalPlayer.PlayerId);
            if (local == null) return true;
            
            var jokerTeam = new List<PlayerControl>();
            jokerTeam.Add(PlayerControl.LocalPlayer);
            __instance.yourTeam = jokerTeam;
            return true;
        }
    }
}
