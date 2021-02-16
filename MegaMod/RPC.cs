using HarmonyLib;
using Hazel;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    enum RPC
    {
        PlayAnimation = 0,
        CompleteTask = 1,
        SyncSettings = 2,
        SetInfected = 3,
        Exiled = 4,
        CheckName = 5,
        SetName = 6,
        CheckColor = 7,
        SetColor = 8,
        SetHat = 9,
        SetSkin = 10,
        ReportDeadBody = 11,
        MurderPlayer = 12,
        SendChat = 13,
        StartMeeting = 14,
        SetScanner = 15,
        SendChatNote = 16,
        SetPet = 17,
        SetStartCounter = 18,
        EnterVent = 19,
        ExitVent = 20,
        SnapTo = 21,
        Close = 22,
        VotingComplete = 23,
        CastVote = 24,
        ClearVote = 25,
        AddVote = 26,
        CloseDoorsOfType = 27,
        RepairSystem = 28,
        SetTasks = 29,
        UpdateGameData = 30,
    }
    enum CustomRPC
    {
        SetDoctor = 43,
        SetProtected = 44,
        ShieldBreak = 45,
        SetDetective = 46,
        DetectiveKill = 47,
        SetEngineer = 48,
        FixLights = 49,
        SetJester = 50,
        ResetVaribles = 51,
        SetLocalPlayers = 56,
        JesterWin = 57,
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            byte packetId = HKHMBLJFLMC;
            MessageReader reader = ALMCIJKELCP;
            switch (packetId)
            {
                case (byte)CustomRPC.ShieldBreak:
                    if (Doctor.protectedPlayer != null)
                    {
                        Doctor.protectedPlayer.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
                        Doctor.protectedPlayer.myRend.material.SetFloat("_Outline", 0f);
                    }    
                    Doctor.protectedPlayer = null;
                    break;
                case (byte)CustomRPC.FixLights:
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)CustomRPC.SetLocalPlayers:
                    ConsoleTools.Info("Setting Local Players...");
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = ALMCIJKELCP.ReadBytesAndSize();

                    foreach (byte id in localPlayerBytes)
                    {
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == id)
                                localPlayers.Add(player);
                        }
                    }
                    break;
                case (byte)RPC.SetInfected:
                    {
                        ConsoleTools.Info("set infected.");
                        break;
                    }
                case (byte)CustomRPC.ResetVaribles:
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
                        break;
                    }
                case (byte)CustomRPC.SetDoctor:
                    {
                        
                        ConsoleTools.Info("Doctor Set Through RPC!");
                        byte doctorId = ALMCIJKELCP.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == doctorId)
                            {
                                Doctor.player = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.SetProtected:
                    {
                        byte ProtectedId = ALMCIJKELCP.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == ProtectedId)
                            {
                                Doctor.protectedPlayer = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.SetDetective:
                    {
                        ConsoleTools.Info("Detective Set Through RPC!");
                        byte detectiveId = ALMCIJKELCP.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == detectiveId)
                            {
                                Detective.player = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.DetectiveKill:
                    {
                        var killerid = ALMCIJKELCP.ReadByte();
                        var targetid = ALMCIJKELCP.ReadByte();
                        PlayerControl killer = PlayerTools.getPlayerById(killerid);
                        PlayerControl target = PlayerTools.getPlayerById(targetid);
                        killer.MurderPlayer(target);
                        break;
                    }
                case (byte)CustomRPC.SetEngineer:
                    {
                        ConsoleTools.Info("Engineer Set Through RPC!");
                        byte EngineerId = ALMCIJKELCP.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == EngineerId)
                            {
                                AddSpecialRole(new Engineer(player));
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.SetJester:
                    {
                        ConsoleTools.Info("Jester Set Through RPC!");
                        byte jesterId = ALMCIJKELCP.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == jesterId)
                            {
                                Jester.player = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.JesterWin:
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        player.RemoveInfected();
                        player.Die(DeathReason.Exile);
                        player.Data.IsDead = true;
                        player.Data.IsImpostor = false;
                    }
                    Jester.player.Revive();
                    Jester.player.Data.IsDead = false;
                    Jester.player.Data.IsImpostor = true;
                    break;
            }
        }
    }
}
