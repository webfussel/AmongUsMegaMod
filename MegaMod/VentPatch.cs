using HarmonyLib;
using System;
using System.Collections.Generic;
using MegaMod.Roles;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    public static class PlayerVentTimeExtension
    {
        private static readonly IDictionary<byte, DateTime> AllVentTimes = new Dictionary<byte, DateTime>();

        public static void SetLastVent(byte player)
        {
            if (AllVentTimes.ContainsKey(player))
                AllVentTimes[player] = DateTime.UtcNow;
            else
                AllVentTimes.Add(player, DateTime.UtcNow);
        }

        public static DateTime GetLastVent(byte player)
        {
            return AllVentTimes.ContainsKey(player) ? AllVentTimes[player] : new DateTime(0);
        }
    }

    [HarmonyPatch(typeof(Vent), "CanUse")]
    public static class VentPatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            float num = float.MaxValue;
            PlayerControl localPlayer = pc.Object;
            if (SpecialRoleIsAssigned(out KeyValuePair<byte, Engineer> engineerKvp))
                couldUse = (engineerKvp.Value.player.PlayerId == PlayerControl.LocalPlayer.PlayerId || localPlayer.Data.IsImpostor) && !localPlayer.Data.IsDead;
            else
                couldUse = localPlayer.Data.IsImpostor && !localPlayer.Data.IsDead;
            canUse = couldUse;
            if ((DateTime.UtcNow - PlayerVentTimeExtension.GetLastVent(pc.Object.PlayerId)).TotalMilliseconds > 1000)
            {
                num = Vector2.Distance(localPlayer.GetTruePosition(), __instance.transform.position);
                canUse &= num <= __instance.UsableDistance;
            }
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), "Method_38")]
    public static class VentEnterPatch
    {
        public static void Postfix(PlayerControl NMEAPOJFNKA)
        {
            PlayerVentTimeExtension.SetLastVent(NMEAPOJFNKA.PlayerId);

            if(SpecialRoleIsAssigned<Seer>(out var seerKvp))
                seerKvp.Value.SendVentMessage(Seer.MessageType.EnteredVent);
        }
    }

    [HarmonyPatch(typeof(Vent), "Method_1")]
    public static class VentExitPatch
    {
        public static void Postfix(PlayerControl NMEAPOJFNKA)
        {
            PlayerVentTimeExtension.SetLastVent(NMEAPOJFNKA.PlayerId);

            if(SpecialRoleIsAssigned<Seer>(out var seerKvp))
                seerKvp.Value.SendVentMessage(Seer.MessageType.ExitedVent);
        }
    }
}
