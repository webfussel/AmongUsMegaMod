using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(EndGameManager), "SetEverythingUp")]
    public static class EndGamePatch
    {
        public static bool Prefix(EndGameManager __instance)
        {
            gameIsRunning = false;

            if (TempData.winners.Count > 1 && TempData.DidHumansWin(TempData.EndReason))
            {
                TempData.winners.Clear();
                List<PlayerControl> orderLocalPlayers = new List<PlayerControl>();
                foreach (PlayerControl player in Crew)
                    if (player.PlayerId == localPlayer.PlayerId)
                        orderLocalPlayers.Add(player);
                foreach (PlayerControl player in Crew)
                    if (player.PlayerId != localPlayer.PlayerId)
                        orderLocalPlayers.Add(player);
                foreach (PlayerControl winner in orderLocalPlayers)
                    TempData.winners.Add(new WinningPlayerData(winner.Data));
            }

            return true;
        }

        public static void Postfix(EndGameManager __instance)
        {
            if (!TempData.DidHumansWin(TempData.EndReason)) return;
            
            foreach (PlayerControl player in Crew)
                if (player.PlayerId == localPlayer.PlayerId)
                    return;
            
            __instance.WinText.Text = "Defeat";
            __instance.WinText.Color = Palette.ImpostorRed;
            __instance.BackgroundBar.material.color = new Color(1, 0, 0);
        }
    }
}
