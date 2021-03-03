using HarmonyLib;
using MegaMod.Roles;
using UnityEngine;
using static MegaMod.MegaModManager;
using System.Linq;
using System.Collections.Generic;

namespace MegaMod
{
    // This is for smaller Game Settings in the beginning, so nothing vanishes off screen
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
    class GameOptionsData_ToHudString
    {
        static void Postfix(ref string __result)
        {
            DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.5f;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
            if (defaultKillButton == null) defaultKillButton = __instance.KillButton.renderer.sprite;
            if (PlayerControl.LocalPlayer.Data.IsImpostor)
            {
                __instance.KillButton.gameObject.SetActive(true);
                __instance.KillButton.renderer.enabled = true;
                __instance.KillButton.renderer.sprite = defaultKillButton;
            }

            bool lastQ = Input.GetKeyUp(KeyCode.Q);

            if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ &&
                __instance.UseButton.isActiveAndEnabled)
            {
                PerformKillPatch.Prefix(null);
            }

            bool sabotageActive = false;
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
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
            if (SpecialRoleIsAssigned<Maniac>(out var maniacKvp))
                maniacKvp.Value.ClearTasks();

            if (SpecialRoleIsAssigned<Doctor>(out var doctorKvp))
                doctorKvp.Value.ShowShieldedPlayer();

            bool showImpostorToManiac = false;
            Role current = GetSpecialRole(PlayerControl.LocalPlayer.PlayerId);
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
                        showImpostorToManiac = maniac.showImpostorToManiac;
                        break;
                    case Detective detective:
                        detective.CheckKillButton(__instance);
                        detective.SetCooldown(Time.deltaTime);
                        __instance.KillButton.renderer.sprite = defaultKillButton;
                        break;
                    case Seer seer:
                        seer.SetChatActive(__instance);
                        break;
                    case Ninja ninja:
                        ninja.CheckKillButton(__instance);
                        break;
                }
            }

            if (!PlayerControl.LocalPlayer.Data.IsImpostor && (!(current is Maniac) || !showImpostorToManiac)) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsImpostor) continue;
                
                if (MeetingHud.Instance != null)
                {
                    foreach (PlayerVoteArea playerVote in MeetingHud.Instance.playerStates)
                        if (player.PlayerId == playerVote.TargetPlayerId)
                            playerVote.NameText.Color = Palette.ImpostorRed;
                }

                player.nameText.Color = TryGetSpecialRole(player.PlayerId, out Role role)
                    ? role.color
                    : Palette.ImpostorRed;
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

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.Data.IsDead || player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        continue;
                   
                    List<Pathfinder.FootPrint> thisPlayersFootprints;

                    // Update existing footprints of this player, if there are any. If not, create one if the player is not in a vent
                    if (Pathfinder.FootPrint.allSorted.ContainsKey(player.PlayerId) && Pathfinder.FootPrint.allSorted[player.PlayerId].Count != 0)
                    {
                        thisPlayersFootprints = Pathfinder.FootPrint.allSorted[player.PlayerId];
                        for (int i = thisPlayersFootprints.Count - 1; i >= 0; i--)
                            thisPlayersFootprints[i].Update(interval);
                    }
                    else
                    {
                        if (!player.inVent)
                            new Pathfinder.FootPrint(player);
                        continue;
                    }

                    // Place new footprints for this player, if the last one isn't to close and the player is not inside a vent
                    if (
                        (thisPlayersFootprints.Count != 0 && Vector2.SqrMagnitude(thisPlayersFootprints.Last().Position - player.transform.position) > 0.1f && !player.inVent)
                        || (thisPlayersFootprints.Count == 0 && !player.inVent)
                    )
                        new Pathfinder.FootPrint(player);
                }
            }
        }
    }
}
