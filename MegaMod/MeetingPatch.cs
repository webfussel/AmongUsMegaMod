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
using MegaMod.Roles;

namespace MegaMod
{

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            //TODO: Das Spiel endet zwar, wenn man den Joker raus wirft, aber die komplette Crew hat gewonnen... halte ich für nich so praktisch
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject) return;
            if (SpecialRoleIsAssigned<Detective>(out var detectiveKvp))
                detectiveKvp.Value.ResetCooldown(ExileController.Instance);
            if (!SpecialRoleIsAssigned<Jester>(out var jesterKvp)) return;
            if (ExileController.Instance.exiled?.PlayerId != jesterKvp.Key) return;
            
            WriteImmediately(RPC.JesterWin);
            
            Jester jester = jesterKvp.Value;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == jester.player) continue;
                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }
            jester.player.Revive();
            jester.player.Data.IsDead = false;
            jester.player.Data.IsImpostor = true;
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
