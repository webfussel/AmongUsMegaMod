using HarmonyLib;
using MegaMod.Roles;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod
{
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
            if (PlayerControl.LocalPlayer.Data.IsImpostor) __instance.KillButton.renderer.sprite = defaultKillButton;
            
            bool lastQ = Input.GetKeyUp(KeyCode.Q);
            PlayerTools.closestPlayer = PlayerTools.GetClosestPlayer(PlayerControl.LocalPlayer, out distLocalClosest);
            
            if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ && __instance.UseButton.isActiveAndEnabled)
            {
                PerformKillPatch.Prefix();
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

            // Jester Tasks have to be reset every frame... I don't know why but huh.
            if (SpecialRoleIsAssigned<Jester>(out var jesterKvp))
                jesterKvp.Value.ClearTasks();
            
            if (SpecialRoleIsAssigned<Doctor>(out var doctorKvp))
                doctorKvp.Value.ShowShieldedPlayer();
                
            bool showImpostorToJester = false;
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
                    case Jester jester:
                        showImpostorToJester = jester.showImpostorToJester;
                        break;
                    case Detective detective:
                        detective.CheckKillButton(__instance);
                        detective.SetCooldown(Time.deltaTime);
                        __instance.KillButton.renderer.sprite = defaultKillButton;
                        break;
                }
            }

            if (!PlayerControl.LocalPlayer.Data.IsImpostor && (!(current is Jester) || !showImpostorToJester)) return;
            
            
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.Data.IsImpostor)
                {
                    if (MeetingHud.Instance != null)
                    {
                        foreach (PlayerVoteArea playerVote in MeetingHud.Instance.playerStates)
                            if (player.PlayerId == playerVote.TargetPlayerId)
                                playerVote.NameText.Color = Palette.ImpostorRed;
                    }
                    player.nameText.Color = Palette.ImpostorRed;
                }
        }
    }
}
