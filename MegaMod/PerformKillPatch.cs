using HarmonyLib;
using System;
using System.Collections.Generic;
using MegaMod.Roles;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        public static bool Prefix(KillButtonManager __instance)
        {
            if (__instance == null || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Detective detective))
                detective.KillOrCommitSuicide(__instance);
            
            if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                engineer.ShowRepairMap();
                
            if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Doctor doctor))
                doctor.SetProtectedPlayer(__instance);

            PlayerControl closest = PlayerTools.FindClosestTarget(PlayerControl.LocalPlayer);
            if (closest != null && PlayerControl.LocalPlayer.Data.IsImpostor && SpecialRoleIsAssigned<Doctor>(out var doctorCheckProtected) && doctorCheckProtected.Value.CheckProtectedPlayer(closest.PlayerId))
                PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            
            if (!SpecialRoleIsAssigned<Doctor>(out var doctorKvp)) return true;
            return doctorKvp.Value.protectedPlayer == null || PlayerTools.FindClosestTarget(PlayerControl.LocalPlayer).PlayerId != doctorKvp.Value.protectedPlayer?.PlayerId;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                //set the detective to impostor for one frame when he kills, so he isn't banned for anti-cheat
                if (GetSpecialRole<Role>(__instance.PlayerId) is Detective)
                    __instance.Data.IsImpostor = true;
                return true;
            }

            //handle the murder after it's ran
            public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                PlayerControl current = __instance;
                PlayerControl target = CAKODNGLPDF;
                
                DeadPlayer deadPlayer = new DeadPlayer(current, target, DateTime.UtcNow);
                
                if (TryGetSpecialRole(current.PlayerId, out Detective _))
                {
                    current.Data.IsImpostor = false;
                }
                KilledPlayers.Add(deadPlayer);
            }
        }
    }
}
