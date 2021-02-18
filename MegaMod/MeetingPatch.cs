using MegaMod;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MegaMod.MegaModManager;
using UnhollowerBaseLib;
using System.Collections;

namespace MegaMod
{

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject) return;
            if (!SpecialRoleIsAssigned<Jester>(out var jester)) return;
            if (ExileController.Instance.exiled?.PlayerId != jester.Key) return;

            Jester jesterInstance = GetSpecialRole<Jester>(jester.Key);
            
            WriteImmediately(RPC.JesterWin);

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == jesterInstance.player) continue;
                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }
            jesterInstance.player.Revive();
            jesterInstance.player.Data.IsDead = false;
            jesterInstance.player.Data.IsImpostor = true;
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class TranslationPatch
    {
        static void Postfix(ref string __result, StringNames HKOIECMDOKL, Il2CppReferenceArray<Il2CppSystem.Object> EBKIKEILMLF)
        {
            if (ExileController.Instance == null || ExileController.Instance.exiled == null) return;
            
            byte playerId = ExileController.Instance.exiled.Object.PlayerId;
            Role role = GetSpecialRole<Role>(playerId);

            switch (HKOIECMDOKL)
            {
                case StringNames.ExileTextPN:
                case StringNames.ExileTextSN:
                {
                    string playerName = ExileController.Instance.exiled.PlayerName;
                    if (role != null)
                        __result = role.EjectMessage(playerName);
                    else
                        __result = playerName + " was not The Impostor.";
                    break;
                }
                case StringNames.ImpostorsRemainP:
                case StringNames.ImpostorsRemainS:
                {
                    if (role is Jester)
                        __result = "";
                    break;
                }
            }
        }
    }
}
