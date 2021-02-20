using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaMod
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static PlayerControl closestPlayer = null;
        
        // TODO: Momentan ungenutzt.
        public static List<PlayerControl> GetCrewMates()
        {
            List<PlayerControl> crewmateIds = new List<PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                bool isInfected = player.Data.IsImpostor;
                if (!isInfected) crewmateIds.Add(player);
            }
            return crewmateIds;
        }

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


        public static PlayerControl GetClosestPlayer(PlayerControl refPlayer, out double distance)
        {
            Vector2 refPos = refPlayer.GetTruePosition();
            double minSqrDistance = double.MaxValue;
            PlayerControl closestPlayer = null;
            
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player == refPlayer) continue;

                Vector2 otherPos = player.GetTruePosition();
                double sqrDistance = Math.Pow(refPos.x - otherPos.x, 2) + Math.Pow(refPos.y - otherPos.y, 2);
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closestPlayer = player;
                }
            }

            distance = Math.Sqrt(minSqrDistance);
            return closestPlayer;
        }

        public static PlayerControl GetClosestPlayer(PlayerControl refPlayer)
        {
            Vector2 refPos = refPlayer.GetTruePosition();
            float minSimpleDistance = float.MaxValue;
            PlayerControl closestPlayer = null;
            
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player == refPlayer) continue;

                Vector2 otherPos = player.GetTruePosition();
                float simpleDistance = Math.Abs(refPos.x - otherPos.x) + Math.Abs(refPos.y - otherPos.y);
                if (simpleDistance < minSimpleDistance)
                {
                    minSimpleDistance = simpleDistance;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }

        public static PlayerControl getPlayerFromId(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }
        
        // TODO: Wird momentan nicht gebraucht. Bitte löschen, falls das so bleibt x)
        public static double getDistBetweenPlayers(PlayerControl player, PlayerControl refplayer)
        {
            var refpos = refplayer.GetTruePosition();
            var playerpos = player.GetTruePosition();

            return Math.Sqrt(Math.Pow(refpos.x - playerpos.x, 2) + Math.Pow(refpos.y - playerpos.y, 2));
        }
    }
}