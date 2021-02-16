using System;
using System.Collections.Generic;
using Essentials.CustomOptions;
using Hazel;
using MegaMod;
using static MegaMod.MegaModManager;
using UnityEngine;

public class Doctor : Role
{
    public static CustomToggleOption optShowDoctor = CustomOption.AddToggle("Show Medic", false);
    public static CustomStringOption optShowShieldedPlayer = CustomOption.AddString("Show Shielded Player", new[] { "Self", "Medic", "Self+Medic", "Everyone" });
    public static CustomToggleOption optPlayerMurderIndicator = CustomOption.AddToggle("Murder Attempt Indicator for Shielded Player", true);
    public static CustomToggleOption optDoctorReportSwitch = CustomOption.AddToggle("Show Medic Reports", true);
    public static CustomNumberOption optDoctorReportNameDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Name", 5, 0, 60, 2.5f);
    public static CustomNumberOption optDoctorReportColorDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Color Type", 20, 0, 120, 2.5f);
    
    public static CustomNumberOption optSpawnChance = CustomOption.AddNumber("Medic Spawn Chance", 100, 0, 100, 5);
    public PlayerControl protectedPlayer { get; set; }
    public bool shieldUsed { get; set; }
    public int doctorKillerNameDuration { get; set; }
    public int doctorKillerColorDuration { get; set; }
    public bool showDoctor { get; set; }
    public bool showReport {get; set;}
    public int  showProtectedPlayer { get; set; }
    public bool shieldKillAttemptIndicator { get; set; }

    public Doctor(PlayerControl player)
    {
        this.player = player;
        name = "Doctor";
        color = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);
        startText = "Create a shield to protect a [8DFFFF]Crewmate";
    }

    public override void ClearSettings()
    {
        player = null;
        protectedPlayer = null;
        shieldUsed = false;
    }

    public override void SetConfigSettings()
    {
        showDoctor = optShowDoctor.GetValue();
        showProtectedPlayer = optShowShieldedPlayer.GetValue();
        showReport = optDoctorReportSwitch.GetValue();
        shieldKillAttemptIndicator = optPlayerMurderIndicator.GetValue();
        doctorKillerNameDuration = (int) optDoctorReportNameDuration.GetValue();
        doctorKillerColorDuration = (int) optDoctorReportColorDuration.GetValue();
    }
    
    
    public static void BreakShield(bool flag)
    {
        if (flag)
        {
            Doctor doctor = GetSpecialRole<Doctor>(PlayerControl.LocalPlayer.PlayerId);
            WriteImmediately(CustomRPC.ShieldBreak);
            
            doctor.protectedPlayer.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
            doctor.protectedPlayer.myRend.material.SetFloat("_Outline", 0f);
            doctor.protectedPlayer = null;
        }
    }

    /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
    public static void SetRole(List<PlayerControl> crew)
    {
        bool spawnChanceAchieved = rng.Next(1, 101) <= optSpawnChance.GetValue();
        if ((crew.Count > 0  && spawnChanceAchieved))
        {
            Doctor doctor = GetSpecialRole<Doctor>(PlayerControl.LocalPlayer.PlayerId);
            int random = rng.Next(0, crew.Count);
            doctor.player = crew[random];
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(CustomRPC.SetDoctor);
            writer.Write(doctor.player.PlayerId);
            CloseWriter(writer);
        }
    }
}