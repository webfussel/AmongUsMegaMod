﻿using HarmonyLib;
using MegaMod.Roles;
using UnityEngine;
using static MegaMod.MegaModManager;
using System.Linq;
using System.Collections.Generic;

namespace MegaMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static void Postfix(HudManager __instance)
        {
            PlayerControl localPlayer = PlayerControl.LocalPlayer;

            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
            if (defaultKillButton == null) defaultKillButton = __instance.KillButton.renderer.sprite;
            if (localPlayer.Data.IsImpostor)
            {
                __instance.KillButton.gameObject.SetActive(true);
                __instance.KillButton.renderer.enabled = true;
                __instance.KillButton.renderer.sprite = defaultKillButton;
            }

            bool lastQ = Input.GetKeyUp(KeyCode.Q);
            
            if (!localPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ && __instance.UseButton.isActiveAndEnabled)
            {
                PerformKillPatch.Prefix(null);
            }

            bool sabotageActive = false;
            foreach (PlayerTask task in localPlayer.myTasks)
            {
                sabotageActive = task.TaskType switch
                {
                    TaskTypes.FixLights => true,
                    TaskTypes.RestoreOxy => true,
                    TaskTypes.ResetReactor => true,
                    TaskTypes.ResetSeismic => true,
                    TaskTypes.FixComms => true,
                    _ => sabotageActive
                };
            }

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.nameText.Color = Color.white;

            // Maniac Tasks have to be reset every frame... I don't know why but huh.
            // Guess I goofed here. But I'll leave it in until I'll fix it. (It works.)
            if (SpecialRoleIsAssigned<Maniac>(out var maniacKvp))
                maniacKvp.Value.ClearTasks();

            if (SpecialRoleIsAssigned<Doctor>(out var doctorKvp))
                doctorKvp.Value.ShowShieldedPlayer();

            bool maniacCanSeeRoles = false;
            
            Role current = GetSpecialRole(localPlayer.PlayerId);
            if (current != null)
            {
                current.SetNameColor();
                current.CheckDead(__instance);
                switch (current)
                {
                    case Doctor doctor:
                        doctor.CheckShieldButton(__instance);
                        doctor.SetCooldown(Time.deltaTime);
                        break;
                    case Engineer engineer:
                        engineer.CheckRepairButton(__instance);
                        engineer.sabotageActive = sabotageActive;
                        break;
                    case Maniac maniac:
                        maniacCanSeeRoles = maniac.showImpostorToManiac;
                        break;
                    case Detective detective:
                        detective.CheckKillButton(__instance);
                        detective.SetCooldown(Time.deltaTime);
                        __instance.KillButton.renderer.sprite = defaultKillButton;
                        break;
                    case Seer seer:
                        seer.AdjustChat(__instance, localPlayer.Data.IsDead);
                        break;
                    case Tracker tracker:
                        tracker.CheckMarkButton(__instance);
                        tracker.AdjustChat(__instance, localPlayer.Data.IsDead);
                        tracker.sabotageActive = sabotageActive;
                        break;
                    case Ninja ninja:
                        ninja.CheckKillButton(__instance);
                        break;
                }
            }

            if (current is Maniac && maniacCanSeeRoles)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {

                    player.nameText.Color = TryGetSpecialRole(player.PlayerId, out Role role)
                        ? role.color
                        : player.nameText.Color;

                    if (MeetingHud.Instance == null) continue;
                    
                    foreach (PlayerVoteArea playerVote in MeetingHud.Instance.playerStates)
                        if (player.PlayerId == playerVote.TargetPlayerId)
                            playerVote.NameText.Color = player.nameText.Color;
                }
            }
            else if (localPlayer.Data.IsImpostor)
            {

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsImpostor) continue;

                    player.nameText.Color = TryGetSpecialRole(player.PlayerId, out Role role)
                        ? role.color
                        : Palette.ImpostorRed;

                    if (MeetingHud.Instance == null) continue;
                    
                    foreach (PlayerVoteArea playerVote in MeetingHud.Instance.playerStates)
                        if (player.PlayerId == playerVote.TargetPlayerId)
                            playerVote.NameText.Color = player.nameText.Color;
                }
            }
        }
    }


    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyButtonPatch
    {
        static void Postfix(EmergencyMinigame __instance)
        {
            if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Seer seer))
                seer.SetEmergencyButtonInactive(__instance);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public class CalculateVisionPatch
    {
        public static void Postfix(ref float __result)
        {
            if (!TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Nocturnal nocturnal) ||
                PlayerControl.LocalPlayer.Data.IsDead) return;
            
            __result = nocturnal.CalculateCurrentVision(__result);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class UpdatePlayerPatch
    {
        private static readonly float interval = MainConfig.OptPathfinderFootprintInterval.GetValue();
        private static float time = 0;
        private static float nextUpdate = interval;

        public static void Postfix(PlayerControl __instance)
        {
            if (!gameIsRunning || !SpecialRoleIsAssigned<Pathfinder>(out var pathfinderKvp) || __instance.PlayerId != pathfinderKvp.Key || PlayerControl.LocalPlayer.PlayerId != pathfinderKvp.Key) return;

            time += Time.fixedDeltaTime;

            if (time >= nextUpdate)
            {
                nextUpdate += interval;

                pathfinderKvp.Value.FixedUpdate(interval);
            }
        }
    }
}
