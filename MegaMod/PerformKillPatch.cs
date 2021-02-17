using HarmonyLib;
using Hazel;
using System;
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
                Role current = GetSpecialRole<Role>(PlayerControl.LocalPlayer.PlayerId);
                PlayerControl target = CurrentTarget;
                if (current != null)
                {
                    switch (current)
                    {
                        case Doctor doctor:
                            return doctor.SetProtectedPlayer(target);
                        case Engineer engineer:
                            return engineer.ShowRepairMap();
                        case Detective detective:
                            return detective.KillOrCommitSuicide(target);
                    }
                }
            }

            if (!SpecialRoleIsAssigned<Doctor>(out var doctorKv)) return true;
            return doctorKv.Value.protectedPlayer == null || PlayerTools.closestPlayer.PlayerId != doctorKv.Value.protectedPlayer.PlayerId;
        }
        

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                if (Detective.player != null)
                {
                    //check if the player is an officer
                    if (__instance == Detective.player)
                    {
                        //if so, set them to impostor for one frame so they aren't banned for anti-cheat
                        __instance.Data.IsImpostor = true;
                    }
                }
                return true;
            }

            //handle the murder after it's ran
            public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                var deadBody = new DeadPlayer();
                deadBody.PlayerId = CAKODNGLPDF.PlayerId;
                deadBody.KillerId = __instance.PlayerId;
                deadBody.KillTime = DateTime.UtcNow;
                deadBody.DeathReason = DeathReason.Kill;
                if (Detective.player != null)
                {
                    //check if killer is officer
                    if (__instance == Detective.player)
                    {
                        //finally, set them back to normal
                        __instance.Data.IsImpostor = false;
                    }
                    if (__instance.PlayerId == CAKODNGLPDF.PlayerId)
                    {
                        deadBody.DeathReason = (DeathReason)3;
                    }
                }
                killedPlayers.Add(deadBody);
            }
        }
    }
}
