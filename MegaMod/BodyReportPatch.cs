using MegaMod;
using HarmonyLib;
using System;
using System.Linq;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo CAKODNGLPDF)
        {
            System.Console.WriteLine("Report Body!");
            byte reporterId = __instance.PlayerId;
            DeadPlayer killer = killedPlayers.FirstOrDefault(x => x.PlayerId == CAKODNGLPDF.PlayerId);
            if (killer == null) return;

            Doctor doctor = GetSpecialRole<Doctor>(PlayerControl.LocalPlayer.PlayerId);
            if (reporterId != doctor.player.PlayerId || !doctor.showReport) return;
            // If doctor found body and has access to reports continue
            
            // Create Body Report
            BodyReport br = new BodyReport();
            br.Killer = PlayerTools.getPlayerById(killer.KillerId);
            br.Reporter = PlayerTools.getPlayerById(reporterId);
            br.KillAge = (float) (DateTime.UtcNow - killer.KillTime).TotalMilliseconds;
            br.DeathReason = killer.DeathReason;
            // Generate message
            var reportMsg = BodyReport.ParseBodyReport(br);

            // If message is empty return
            if (string.IsNullOrWhiteSpace(reportMsg)) return;
            
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
