using MegaMod;
using HarmonyLib;
using System;
using System.Linq;
using MegaMod.Roles;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo CAKODNGLPDF)
        {
            System.Console.WriteLine("Report Body!");
            DeadPlayer killed = KilledPlayers.FirstOrDefault(x => x.Victim.PlayerId == CAKODNGLPDF.PlayerId);
            if (killed == null) return;

            Detective detective = GetSpecialRole<Detective>(PlayerControl.LocalPlayer.PlayerId);
            if (__instance.PlayerId != detective.player.PlayerId || !detective.showReport) return;
            
            // Create Body Report
            BodyReport br = new BodyReport();
            br.Killer = killed.Killer;
            br.Reporter = __instance;
            br.DeadPlayer = killed;
            br.KillAge = (float) (DateTime.UtcNow - killed.KillTime).TotalMilliseconds;
            br.DeathReason = killed.DeathReason;
            // Generate message
            var reportMsg = br.ParseBodyReport();

            // If message is empty return
            if (string.IsNullOrWhiteSpace(reportMsg)) return;
            
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                // Send the message through chat only visible to the Detective
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
