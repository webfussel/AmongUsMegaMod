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
            if (ExileController.Instance != null && obj == ExileController.Instance.gameObject)
            {
                if (Jester.player != null)
                {
                    if (ExileController.Instance.Field_10 != null && ExileController.Instance.Field_10.PlayerId == Jester.player.PlayerId)
                    {
                        WriteImmediately(CustomRPC.JesterWin);

                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player != Jester.player)
                            {
                                player.RemoveInfected();
                                player.Die(DeathReason.Exile);
                                player.Data.IsDead = true;
                                player.Data.IsImpostor = false;
                            }
                        }
                        Jester.player.Revive();
                        Jester.player.Data.IsDead = false;
                        Jester.player.Data.IsImpostor = true;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class TranslationPatch
    {
        static void Postfix(ref string __result, StringNames HKOIECMDOKL, Il2CppReferenceArray<Il2CppSystem.Object> EBKIKEILMLF)
        {
            if (ExileController.Instance != null && ExileController.Instance.Field_10 != null)
            {
                byte playerId = ExileController.Instance.Field_10.Object.PlayerId;

                if (HKOIECMDOKL == StringNames.ExileTextPN || HKOIECMDOKL == StringNames.ExileTextSN)
                {
                    string playerName = ExileController.Instance.Field_10.PlayerName;
                    Role roleInstance = null;

                    if (
                        (SpecialRoleIsAssigned<Detective>(out instance) && playerId == playerId) ||
                        (SpecialRoleIsAssigned<Doctor>(out instance)    && playerId == playerId) ||
                        (SpecialRoleIsAssigned<Engineer>(out instance)  && playerId == playerId) ||
                        (SpecialRoleIsAssigned<Jester>(out instance)    && playerId == playerId) ||
                    )
                        __result = roleInstance.EjectMessage(playerName);
                    else
                        __result = playerName + " was not The Impostor.";
                }
                if (HKOIECMDOKL == StringNames.ImpostorsRemainP || HKOIECMDOKL == StringNames.ImpostorsRemainS)
                {
                    if (Jester.player != null && playerId == Jester.player.PlayerId)
                        __result = "";
                }
            }
        }
    }
}
