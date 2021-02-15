using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static MegaMod.MegaMod;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            Doctor.ClearSettings();
            Detective.ClearSettings();
            Engineer.ClearSettings();
            Jester.ClearSettings();
            Doctor.SetConfigSettings();
            Detective.SetConfigSettings();
            Engineer.SetConfigSettings();
            Jester.SetConfigSettings();
            killedPlayers.Clear();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.medicSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetDoctor, Hazel.SendOption.None, -1);
                var MedicRandom = rng.Next(0, crewmates.Count);
                Doctor.Medic = crewmates[MedicRandom];
                crewmates.RemoveAt(MedicRandom);
                byte MedicId = Doctor.Medic.PlayerId;

                writer.Write(MedicId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.officerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetDetective, Hazel.SendOption.None, -1);

                var OfficerRandom = rng.Next(0, crewmates.Count);
                Detective.Officer = crewmates[OfficerRandom];
                crewmates.RemoveAt(OfficerRandom);
                byte OfficerId = Detective.Officer.PlayerId;

                writer.Write(OfficerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.engineerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetEngineer, Hazel.SendOption.None, -1);
                var EngineerRandom = rng.Next(0, crewmates.Count);
                Engineer.player = crewmates[EngineerRandom];
                crewmates.RemoveAt(EngineerRandom);
                byte EngineerId = Engineer.player.PlayerId;

                writer.Write(EngineerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.jokerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetJester, Hazel.SendOption.None, -1);
                var JokerRandom = rng.Next(0, crewmates.Count);
                ConsoleTools.Info(JokerRandom.ToString());
                Jester.Joker = crewmates[JokerRandom];
                crewmates.RemoveAt(JokerRandom);
                byte JokerId = Jester.Joker.PlayerId;

                writer.Write(JokerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor)
                    continue;
                if (Jester.Joker != null && player.PlayerId == Jester.Joker.PlayerId)
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
        public static bool Prefix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            var debugImpostors = false;
            if (debugImpostors)
            {
                var infected = new byte[] { 0, 0 };

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.PlayerName == "Impostor")
                    {
                        infected[0] = player.PlayerId;
                    }
                    if (player.Data.PlayerName == "Pretender")
                    {
                        infected[1] = player.PlayerId;
                    }
                }

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RPC.SetInfected, Hazel.SendOption.None, -1);
                writer.WritePacked((uint)2);
                writer.WriteBytesAndSize(infected);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                PlayerControl.LocalPlayer.SetInfected(infected);

                return false;
            }
            return true;
        }
    }
}
