using HarmonyLib;
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
            
            bool lastQ = Input.GetKeyUp(KeyCode.Q);
            PlayerTools.closestPlayer = PlayerTools.GetClosestPlayer(PlayerControl.LocalPlayer, out DistLocalClosest);
            
            if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ && __instance.UseButton.isActiveAndEnabled)
            {
                PerformKillPatch.Prefix();
            }

            Role current = GetSpecialRole<Role>(PlayerControl.LocalPlayer.PlayerId);
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
            
            // TODO: Check if null
            bool showImpostorToJester = false;
            if (current != null)
            {
                current.SetNameColor();
                current.CheckDead(__instance);
                switch (current)
                {
                    case Doctor doctor:
                        doctor.SetShieldButton(__instance);
                        doctor.CheckShieldButton(__instance);
                        doctor.ShowShieldedPlayer();
                        break;
                    case Engineer engineer:
                        engineer.SetRepairButton(__instance);
                        engineer.sabotageActive = sabotageActive;
                        break;
                    case Jester jester:
                        jester.ClearTasks();
                        showImpostorToJester = jester.showImpostorToJester;
                        break;
                    case Detective detective:
                        detective.CheckKillButton(__instance);
                        break;
                }
            }
            
            rend.SetActive(rend == false);

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.nameText.Color = Color.white;

            if (!PlayerControl.LocalPlayer.Data.IsImpostor && !showImpostorToJester) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.nameText.Color = player.Data.IsImpostor ? Palette.ImpostorRed : player.nameText.Color;
        }
    }
}
