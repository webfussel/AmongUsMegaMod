using HarmonyLib;
using System;
using static MegaMod.MegaModManager;
using UnhollowerBaseLib;
using MegaMod.Roles;

namespace MegaMod
{

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject) return;
            if (!SpecialRoleIsAssigned<Maniac>(out var maniacKvp)) return;
            if (ExileController.Instance.exiled?.PlayerId != maniacKvp.Key) return;
            
            WriteImmediately(RPC.ManiacWin);
            
            Maniac maniac = maniacKvp.Value;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == maniac.player) continue;
                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }
            maniac.player.Revive();
            maniac.player.Data.IsDead = false;
            maniac.player.Data.IsImpostor = true;
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
                    if (role is Maniac)
                        __result = "";
                    break;
                }
            }
        }
    }
}
