using MegaMod;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MegaMod.MegaMod;
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
                if (Jester.Joker != null)
                {
                    if (ExileController.Instance.Field_10 != null && ExileController.Instance.Field_10.PlayerId == Jester.Joker.PlayerId)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JesterWin, Hazel.SendOption.None, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player != Jester.Joker)
                            {
                                player.RemoveInfected();
                                player.Die(DeathReason.Exile);
                                player.Data.IsDead = true;
                                player.Data.IsImpostor = false;
                            }
                        }
                        Jester.Joker.Revive();
                        Jester.Joker.Data.IsDead = false;
                        Jester.Joker.Data.IsImpostor = true;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class MeetingEnd
    {
        static void Postfix(ExileController __instance)
        {
            Detective.lastKilled = DateTime.UtcNow.AddMilliseconds(__instance.Duration);
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

                    

                    if (Doctor.Medic != null && playerId == Doctor.Medic.PlayerId)
                        __result = playerName + " was The Medic.";
                    else if (Engineer.player != null && playerId == Engineer.player.PlayerId)
                        __result = Engineer.ejectMessage(playerName);
                    else if (Detective.Officer != null && playerId == Detective.Officer.PlayerId)
                        __result = playerName + " was The Officer.";
                    else if (Jester.Joker != null && playerId == Jester.Joker.PlayerId)
                        __result = playerName + " was The Joker.";
                    else
                        __result = playerName + " was not The Impostor.";
                }
                if (HKOIECMDOKL == StringNames.ImpostorsRemainP || HKOIECMDOKL == StringNames.ImpostorsRemainS)
                {
                    if (Jester.Joker != null && playerId == Jester.Joker.PlayerId)
                        __result = "";
                }
            }
        }
    }
}
