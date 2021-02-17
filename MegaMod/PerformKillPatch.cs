﻿using HarmonyLib;
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
                PlayerControl target = CurrentTarget;
                if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Role current))
                {
                    // TODO: Bin mir nicht sicher, ob wir so den Typ der Rolle vergleichen können.
                    // Ich denke eher, dass wir es so schreiben müssen:
                    // if(current is Doctor)
                    // else if(current is Engineer)
                    // Aber das sehen wir dann beim Testen x)
                    
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
                //set the detective to impostor for one frame when he kills, so he isn't banned for anti-cheat
                if (GetSpecialRole<Role>(__instance.PlayerId) is Detective)
                    __instance.Data.IsImpostor = true;
                return true;
            }

            //handle the murder after it's ran
            public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                DeadPlayer deadPlayer = new DeadPlayer(__instance.PlayerId, CAKODNGLPDF.PlayerId, DateTime.UtcNow, DeathReason.Kill);
                if (SpecialRoleIsAssigned<Detective>(out KeyValuePair<byte, Detective> detectiveKvp))
                {
                    // If the killer is the detective, set him back to crewmate
                    if (__instance == detectiveKvp.Value)
                        __instance.Data.IsImpostor = false;
                    if (__instance.PlayerId == CAKODNGLPDF.PlayerId)
                        // TODO: Was zum Fick?! DeathReason hat gar keinen Index 3?!?!?! What?!?!?!?!?!?!?!?! Und wieso würde man hier nen Integer casten statt einfach zu schreiben DeathReason.Irgendwas?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!
                        deadPlayer.DeathReason = (DeathReason)3;
                }
                killedPlayers.Add(deadPlayer);
            }
        }
    }
}
