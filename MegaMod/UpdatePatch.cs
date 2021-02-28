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
}
