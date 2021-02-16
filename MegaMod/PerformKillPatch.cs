﻿using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer == Engineer.player)
            {
                DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
                {
                    m.ShowInfectedMap();
                    m.ColorControl.baseColor = Engineer.sabotageActive ? Color.gray : ModdedPalette.engineerColor;
                });
                return false;
            }
            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;
            if (CurrentTarget != null)
            {
                PlayerControl target = CurrentTarget;
                //code that handles the ability button presses
                if (Detective.player != null && PlayerControl.LocalPlayer.PlayerId == Detective.player.PlayerId)
                {
                    if (PlayerTools.GetOfficerKD() == 0)
                    {
                        //check if they're shielded by medic
                        if (Doctor.protectedPlayer != null && target.PlayerId == Doctor.protectedPlayer.PlayerId)
                        {
                            //officer suicide packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer);
                            Detective.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        //check if they're joker and the setting is configured
                        else if (Jester.jokerCanDieToOfficer && (Jester.player != null && target.PlayerId == Jester.player.PlayerId))
                        {
                            //officer joker murder packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(target);
                            Detective.lastKilled = DateTime.UtcNow;
                        }
                        //check if they're an impostor
                        else if (target.Data.IsImpostor)
                        {
                            //officer impostor murder packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(target);
                            Detective.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        //else, they're innocent and not shielded
                        else
                        {
                            //officer suicide packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer);
                            Detective.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        return false;
                    }
                    return false;
                }
                else if (Doctor.player != null && PlayerControl.LocalPlayer == Doctor.player)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetProtected, Hazel.SendOption.None, -1);
                    Doctor.protectedPlayer = target;
                    Doctor.shieldUsed = true;
                    byte ProtectedId = Doctor.protectedPlayer.PlayerId;
                    writer.Write(ProtectedId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    return false;
                }
            }

            if (Doctor.protectedPlayer != null && PlayerTools.closestPlayer.PlayerId == Doctor.protectedPlayer.PlayerId)
            {
                //cancel the kill
                return false;
            }
            //otherwise, continue the murder as normal
            return true;
        }
        

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                if (Detective.player != null)
                {
                    //check if the player is an officer
                    if (__instance == Detective.player)
                    {
                        //if so, set them to impostor for one frame so they aren't banned for anti-cheat
                        __instance.Data.IsImpostor = true;
                    }
                }
                return true;
            }

            //handle the murder after it's ran
            public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                var deadBody = new DeadPlayer();
                deadBody.PlayerId = CAKODNGLPDF.PlayerId;
                deadBody.KillerId = __instance.PlayerId;
                deadBody.KillTime = DateTime.UtcNow;
                deadBody.DeathReason = DeathReason.Kill;
                if (Detective.player != null)
                {
                    //check if killer is officer
                    if (__instance == Detective.player)
                    {
                        //finally, set them back to normal
                        __instance.Data.IsImpostor = false;
                    }
                    if (__instance.PlayerId == CAKODNGLPDF.PlayerId)
                    {
                        deadBody.DeathReason = (DeathReason)3;
                    }
                }
                killedPlayers.Add(deadBody);
            }
        }
    }
}