using HarmonyLib;
using MegaMod.Roles;
using UnityEngine;
using static MegaMod.MegaModManager;

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
            if (SpecialRoleIsAssigned<Maniac>(out var maniacKvp))
                maniacKvp.Value.ClearTasks();

            if (SpecialRoleIsAssigned<Doctor>(out var doctorKvp))
                doctorKvp.Value.ShowShieldedPlayer();

            bool showImpostorToManiac = false;
            
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
                        showImpostorToManiac = maniac.showImpostorToManiac;
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

            if (!localPlayer.Data.IsImpostor && (!(current is Maniac) || !showImpostorToManiac)) return;

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

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public class CalculateVisionPatch
    {
        public static void Postfix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo PlayerData)
        {
            if (SpecialRoleIsAssigned<Nocturnal>(out var nocturnalKvp) && nocturnalKvp.Key == PlayerData.PlayerId && !nocturnalKvp.Value.player.Data.IsDead)
            {
                // TODO: Speicher den Wert von normalVision am Anfang des Spiels in der Nocturnal-Instanz, damit er nicht jedes Frame berechnet werden muss
                float normalVision = __instance.MaxLightRadius * PlayerControl.GameOptions.CrewLightMod;
                __result = normalVision * 2 - __result;
            }
        }
    }
}
