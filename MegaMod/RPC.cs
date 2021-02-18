using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            MessageReader reader = ALMCIJKELCP;
            
            switch (HKHMBLJFLMC /*Packet ID*/)
            {
                // ---------------------- Set special roles ----------------------
                
                case (byte) RPC.SetDetective:
                    ConsoleTools.Info("Detective Set Through RPC!");
                    byte detectiveId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == detectiveId)
                            AddSpecialRole(new Detective(player));
                    break;
                case (byte) RPC.SetDoctor:
                    ConsoleTools.Info("Doctor Set Through RPC!");
                    byte doctorId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == doctorId)
                            AddSpecialRole(new Doctor(player));
                    break;
                case (byte) RPC.SetEngineer:
                    ConsoleTools.Info("Engineer Set Through RPC!");
                    byte engineerId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == engineerId)
                            AddSpecialRole(new Engineer(player));
                    break;
                case (byte) RPC.SetJester:
                    ConsoleTools.Info("Jester Set Through RPC!");
                    byte jesterId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == jesterId)
                            AddSpecialRole(new Jester(player));
                    break;
                
                // -------------- Happenings related to special roles --------------
                
                case (byte)RPC.SetProtected:
                    byte protectedId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == protectedId)
                            GetSpecialRole<Doctor>().protectedPlayer = player;
                    break;
                case (byte)RPC.ShieldBreak:
                    Doctor doctor = GetSpecialRole<Doctor>();
                    PlayerControl protectedPlayer = doctor.protectedPlayer;
                    if (protectedPlayer != null)
                    {
                        protectedPlayer.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
                        protectedPlayer.myRend.material.SetFloat("_Outline", 0f);
                    }    
                    doctor.protectedPlayer = null;
                    break;
                case (byte) RPC.DetectiveKill:
                    var killerid = reader.ReadByte();
                    var targetid = reader.ReadByte();
                    PlayerControl killer = PlayerTools.GetPlayerById(killerid);
                    PlayerControl target = PlayerTools.GetPlayerById(targetid);
                    killer.MurderPlayer(target);
                    break;
                case (byte)RPC.JesterWin:
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        player.RemoveInfected();
                        player.Die(DeathReason.Exile);
                        player.Data.IsDead = true;
                        player.Data.IsImpostor = false;
                    }

                    Jester jester = GetSpecialRole<Jester>();
                    jester.player.Revive();
                    jester.player.Data.IsDead = false;
                    jester.player.Data.IsImpostor = true;
                    break;
                
                // --------------------------- Other ---------------------------
                
                case (byte)RPC.FixLights:
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)RPC.SetLocalPlayers:
                    ConsoleTools.Info("Setting Local Players...");
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = reader.ReadBytesAndSize();

                    foreach (byte id in localPlayerBytes)
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                            if (player.PlayerId == id)
                                localPlayers.Add(player);
                        break;
                case (byte) RPC.SetInfected:
                    ConsoleTools.Info("set infected.");
                    break;
                case (byte) RPC.ResetVariables:
                    List<Role> assignedRoles = assignedSpecialRoles.Values.ToList();
                    foreach (Role r in assignedRoles)
                    {
                        r.ClearSettings();
                        r.SetConfigSettings();
                    }
                    killedPlayers.Clear();
                    break;
            }
        }
    }
}
