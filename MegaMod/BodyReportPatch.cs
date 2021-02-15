﻿using MegaMod;
using HarmonyLib;
using System;
using System.Linq;
using static MegaMod.MegaMod;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo CAKODNGLPDF)
        {
            System.Console.WriteLine("Report Body!");
            byte reporterId = __instance.PlayerId;
            DeadPlayer killer = killedPlayers.Where(x => x.PlayerId == CAKODNGLPDF.PlayerId).FirstOrDefault();
            if (killer != null)
            {
                // If there is a Medic alive and Medic reported and reports are enabled
                if (Doctor.Medic != null && reporterId == Doctor.Medic.PlayerId && Doctor.showReport)
                {
                    // If the user is the medic
                    if (Doctor.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        // Create Body Report
                        BodyReport br = new BodyReport();
                        br.Killer = PlayerTools.getPlayerById(killer.KillerId);
                        br.Reporter = br.Killer = PlayerTools.getPlayerById(killer.KillerId);
                        br.KillAge = (float)(DateTime.UtcNow - killer.KillTime).TotalMilliseconds;
                        br.DeathReason = killer.DeathReason;
                        // Generate message
                        var reportMsg = BodyReport.ParseBodyReport(br);

                        // If message is not empty
                        if (!string.IsNullOrWhiteSpace(reportMsg))
                        {   
                            
                            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                            {
                                // Send the message through chat only visible to the medic
                                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, reportMsg);
                            }
                            if (reportMsg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Really did not understand this
                                DestroyableSingleton<Telemetry>.Instance.SendWho();
                            }
                        }
                    }
                }
            }       
        }
    }
}
