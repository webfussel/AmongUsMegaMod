﻿using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using MegaMod.Roles;
using UnhollowerBaseLib;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            List<Role> assignedRoles = assignedSpecialRoles.Values.ToList();
            foreach (Role r in assignedRoles)
            {
                r.ClearSettings();
            }
            killedPlayers.Clear();
            assignedSpecialRoles.Clear();
            WriteImmediately(RPC.ResetVariables);

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);

            ConsoleTools.Info("Trying to set special roles");
            Doctor.SetRole(crewmates);
            Detective.SetRole(crewmates);
            Engineer.SetRole(crewmates);
            Jester.SetRole(crewmates);

            crew.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            
            bool jesterExists = SpecialRoleIsAssigned<Jester>(out var jesterKv);
            Jester jesterInstance = jesterKv.Value;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor) continue;
                if (jesterExists && jesterInstance.player.PlayerId == player.PlayerId) continue;
                
                crew.Add(player);
            }

            MessageWriter writer = GetWriter(RPC.SetLocalPlayers);
            writer.WriteBytesAndSize(crew.Select(player => player.PlayerId).ToArray());
            CloseWriter(writer);
        }
    }
}
