using System.Collections.Generic;
using System.Linq;
using Discord;
using HarmonyLib;
using Hazel;
using MegaMod.Roles;
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
                case (byte) RPC.SetRole:
                    byte roleId = reader.ReadByte();
                    byte playerId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == playerId)
                            switch (roleId)
                            {
                                case var value when value == Detective.RoleID:
                                    AddSpecialRole(new Detective(player));
                                    break;
                                
                                case var value when value == Doctor.RoleID:
                                    AddSpecialRole(new Doctor(player));
                                    break;
                                
                                case var value when value == Engineer.RoleID:
                                    AddSpecialRole(new Engineer(player));
                                    break;
                                
                                case var value when value == Maniac.RoleID:
                                    AddSpecialRole(new Maniac(player));
                                    break;
                                
                                case var value when value == Seer.RoleID:
                                    AddSpecialRole(new Seer(player));
                                    break;
                                
                                case var value when value == Ninja.RoleID:
                                    AddSpecialRole(new Ninja(player));
                                    break;
                            }
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
                case (byte) RPC.AttemptShield:
                    Doctor doctorShieldAttempt = GetSpecialRole<Doctor>();
                    if (doctorShieldAttempt.player != null)
                    {
                        doctorShieldAttempt.AttemptKillShielded();
                    }
                    break;
                case (byte) RPC.DetectiveKill:
                    var killerid = reader.ReadByte();
                    var targetid = reader.ReadByte();
                    PlayerControl killer = PlayerTools.GetPlayerById(killerid);
                    PlayerControl target = PlayerTools.GetPlayerById(targetid);
                    killer.MurderPlayer(target);
                    break;
                case (byte)RPC.ManiacWin:
                    Maniac maniac = GetSpecialRole<Maniac>();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player == maniac.player) continue;
                        player.RemoveInfected();
                        player.Die(DeathReason.Exile);
                        player.Data.IsDead = true;
                        player.Data.IsImpostor = false;
                    }

                    maniac.player.Revive();
                    maniac.player.Data.IsDead = false;
                    maniac.player.Data.IsImpostor = true;
                    break;
                
                // --------------------------- Other ---------------------------
                
                case (byte)RPC.FixLights:
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)RPC.SetLocalPlayers:
                    Crew.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = reader.ReadBytesAndSize();

                    foreach (byte id in localPlayerBytes)
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                            if (player.PlayerId == id)
                                Crew.Add(player);
                    break;
                case (byte) RPC.SetInfected:
                    break;
                case (byte) RPC.ResetVariables:
                    List<Role> assignedRoles = AssignedSpecialRoles.Values.ToList();
                    foreach (Role r in assignedRoles)
                    {
                        r.ClearSettings();
                    }
                    ResetValues();
                    break;
            }
        }
    }
}
