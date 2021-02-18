﻿using HarmonyLib;
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
            Detective.SetRole(crewmates);
            Engineer.SetRole(crewmates);
            Jester.SetRole(crewmates);

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            
            bool jesterExists = SpecialRoleIsAssigned<Jester>(out var jesterKv);
            Jester jesterInstance = jesterKv.Value;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor) continue;
                if (jesterExists && jesterInstance.player.PlayerId == player.PlayerId) continue;
                
                localPlayers.Add(player);
            }

            MessageWriter writer = GetWriter(CustomRPC.SetLocalPlayers);
            writer.WriteBytesAndSize(localPlayers.Select(player => player.PlayerId).ToArray());
            CloseWriter(writer);
        }
    }
}
