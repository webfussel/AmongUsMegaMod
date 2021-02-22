using HarmonyLib;
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
            List<Role> assignedRoles = AssignedSpecialRoles.Values.ToList();
            foreach (Role r in assignedRoles)
            {
                r.ClearSettings();
            }
            ResetValues();
            
            WriteImmediately(RPC.ResetVariables);

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);

            Doctor.SetRole(crewmates);
            Detective.SetRole(crewmates);
            Engineer.SetRole(crewmates);
            Maniac.SetRole(crewmates);
            Seer.SetRole(crewmates);
            
            
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
            impostors.RemoveAll(x => !x.Data.IsImpostor);

            Ninja.SetRole(impostors);

            Crew.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            
            bool maniacExists = SpecialRoleIsAssigned<Maniac>(out var maniacKvp);
            Maniac maniacInstance = maniacKvp.Value;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor) continue;
                if (maniacExists && maniacInstance.player.PlayerId == player.PlayerId) continue;
                
                Crew.Add(player);
            }

            MessageWriter writer = GetWriter(RPC.SetLocalPlayers);
            writer.WriteBytesAndSize(Crew.Select(player => player.PlayerId).ToArray());
            CloseWriter(writer);
        }
    }
}
