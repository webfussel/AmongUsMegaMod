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
            
            BodyReport br = new BodyReport();
            br.Killer = killed.Killer;
            br.DeadPlayer = killed;
            br.KillAge = (float) (DateTime.UtcNow - killed.KillTime).TotalMilliseconds;
            
            var reportMsg = br.ParseBodyReport();

            if (string.IsNullOrWhiteSpace(reportMsg)) return;
            
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, reportMsg);
            }
            if (reportMsg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyableSingleton<Telemetry>.Instance.SendWho();
            }
        }
    }
}
