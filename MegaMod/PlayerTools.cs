using HarmonyLib;
using System;
using System.Collections.Generic;
using Il2CppSystem;
using UnityEngine;

namespace MegaMod
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static PlayerControl closestPlayer = null;
        
        public static List<PlayerControl> getCrewMates()
        {
            List<PlayerControl> CrewmateIds = new List<PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                bool isInfected = false;
                if (player.Data.IsImpostor)
                {
                    isInfected = true;
                    break;
                }
                if (!isInfected)
                {
                    CrewmateIds.Add(player);
                }
            }
            return CrewmateIds;
        }

        public static PlayerControl getPlayerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }
            return null;
        }

        public static float GetOfficerKD()
        {
            if (MegaModManager.Detective.lastKilled == null)
            {
                return MegaModManager.Detective.OfficerCD;
            }
            DateTime now = DateTime.UtcNow;
            TimeSpan diff = (TimeSpan)(now - MegaModManager.Detective.lastKilled);

            var KillCoolDown = MegaModManager.Detective.OfficerCD * 1000.0f;
            if (KillCoolDown - (float)diff.TotalMilliseconds < 0) return 0;
            else
            {
                return (KillCoolDown - (float)diff.TotalMilliseconds) / 1000.0f;
            }
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