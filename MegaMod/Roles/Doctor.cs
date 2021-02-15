using System;
using UnityEngine;

public class Doctor 
{
    public static Color color = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);

    public static PlayerControl Medic { get; set; }
    public static PlayerControl Protected { get; set; }
    public static bool shieldUsed { get; set; }
    public static int medicKillerNameDuration { get; set; }
    public static int medicKillerColorDuration { get; set; }
    public static bool showMedic { get; set; }
    public static bool showReport {get; set;}
    public static int  showProtected { get; set; }
    public static bool shieldKillAttemptIndicator { get; set; }

    public static void ClearSettings()
    {
        Medic = null;
        Protected = null;
        shieldUsed = false;
    }

    public static void SetConfigSettings()
    {
        showMedic = HarmonyMain.showMedic.GetValue();
        showProtected = HarmonyMain.showShieldedPlayer.GetValue();
        showReport = HarmonyMain.medicReportSwitch.GetValue();
        shieldKillAttemptIndicator = HarmonyMain.playerMurderIndicator.GetValue();
        medicKillerNameDuration = (int)HarmonyMain.medicReportNameDuration.GetValue();
        medicKillerColorDuration = (int)HarmonyMain.medicReportColorDuration.GetValue();
    }
}