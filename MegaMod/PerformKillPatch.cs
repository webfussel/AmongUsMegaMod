using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using MegaMod.Roles;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;

            if (CurrentTarget != null)
            {
                PlayerControl target = CurrentTarget;

                if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Doctor doctor))
                    doctor?.SetProtectedPlayer(target);

                if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Detective detective))
                    detective?.KillOrCommitSuicide(target);
            }
            else
            {
                if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    engineer?.ShowRepairMap();
            }

            if (!SpecialRoleIsAssigned<Doctor>(out var doctorKvp)) return true;
            return doctorKvp.Value.protectedPlayer == null || PlayerTools.closestPlayer.PlayerId != doctorKvp.Value.protectedPlayer.PlayerId;
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
                
                DeadPlayer deadPlayer = new DeadPlayer(current.PlayerId, target.PlayerId, DateTime.UtcNow, DeathReason.Kill);
                
                if (SpecialRoleIsAssigned<Detective>(out KeyValuePair<byte, Detective> detectiveKvp))
                {
                    // If the killer is the detective, set him back to crewmate
                    if (current == detectiveKvp.Value.player)
                        current.Data.IsImpostor = false;
                    if (current.PlayerId == target.PlayerId)
                        deadPlayer.DeathReason = DEATH_REASON_SUICIDE;
                }
                killedPlayers.Add(deadPlayer);
            }
        }
    }
}
