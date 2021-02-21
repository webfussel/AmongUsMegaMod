using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaMod
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static PlayerControl GetPlayerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id) return player;
            return null;
        }

        public static PlayerControl FindClosestTarget(PlayerControl currentPlayer)
        {
            PlayerControl playerControl1 = (PlayerControl) null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!(bool) ShipStatus.Instance)
                return null;
            Vector2 truePosition = currentPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int index = 0; index < allPlayers.Count; ++index)
            {
                GameData.PlayerInfo playerInfo = allPlayers[index];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != currentPlayer.PlayerId && !playerInfo.IsDead)
                {
                    PlayerControl playerControl2 = playerInfo.Object;
                    if ((bool) playerControl2)
                    {
                        Vector2 vector2 = playerControl2.GetTruePosition() - truePosition;
                        float magnitude = vector2.magnitude;
                        if (magnitude <= (double) num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector2.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            playerControl1 = playerControl2;
                            num = magnitude;
                        }
                    }
                }
            }
            return playerControl1;
        }
    }
}