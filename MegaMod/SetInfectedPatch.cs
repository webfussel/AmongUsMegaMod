using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            
            // TODO: Über Map iterieren und für jeden Spieler zurücksetzen
            
            List<Role> assignedRoles = assignedSpecialRoles.Values.ToList();
            foreach (Role r in assignedRoles)
            {
                r.ClearSettings();
                r.SetConfigSettings();
            }
            
            killedPlayers.Clear();
            
            WriteImmediately(CustomRPC.ResetVaribles);

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);

            Doctor.SetRole(crewmates);

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.officerSpawnChance.GetValue()))
            {

                var OfficerRandom = rng.Next(0, crewmates.Count);
                Detective.player = crewmates[OfficerRandom];
                crewmates.RemoveAt(OfficerRandom);
                byte OfficerId = Detective.player.PlayerId;

                MessageWriter writer = GetWriter(CustomRPC.SetDetective);
                writer.Write(OfficerId);
                CloseWriter(writer);
            }
            
            Engineer.SetRole(crewmates);

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.jokerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetJester, Hazel.SendOption.None, -1);
                var JokerRandom = rng.Next(0, crewmates.Count);
                ConsoleTools.Info(JokerRandom.ToString());
                Jester.player = crewmates[JokerRandom];
                crewmates.RemoveAt(JokerRandom);
                byte JokerId = Jester.player.PlayerId;

                writer.Write(JokerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor)
                    continue;
                if (Jester.player != null && player.PlayerId == Jester.player.PlayerId)
                    continue;
                else
                    localPlayers.Add(player);
            }
            var localPlayerBytes = new List<byte>();
            foreach (PlayerControl player in localPlayers)
            {
                localPlayerBytes.Add(player.PlayerId);
            }
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLocalPlayers, Hazel.SendOption.None, -1);
            writer.WriteBytesAndSize(localPlayerBytes.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}
