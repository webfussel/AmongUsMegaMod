using System;
using UnityEngine;

public class Detective
{
    public static Color color = new Color(0, 40f / 255f, 198f / 255f, 1);
    public static PlayerControl Officer { get; set; }
    public static float OfficerCD { get; set; }
    public static bool showOfficer { get; set; }
    public static DateTime? lastKilled { get; set; }

    public static void ClearSettings()
    {
        Officer = null;
        lastKilled = null;
    }

    public static void SetConfigSettings()
    {
        showOfficer = HarmonyMain.showOfficer.GetValue();
        OfficerCD = HarmonyMain.DetectiveKillCooldown.GetValue();
    }
}