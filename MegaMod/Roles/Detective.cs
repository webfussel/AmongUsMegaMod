using Essentials.CustomOptions;
using HarmonyLib;
using Hazel;
using MegaMod;
using static MegaMod.MegaModManager;
using UnityEngine;
using System;

public class Detective : Role
{
    public static CustomNumberOption optDetectiveKillCooldown = CustomOption.AddNumber("Detective Kill Cooldown", 30f, 10f, 60f, 2.5f);
    public static CustomToggleOption optShowDetective = CustomOption.AddToggle("Show Detective", false);
    public static CustomNumberOption optDetectiveSpawnChance = CustomOption.AddNumber("Detective Spawn Chance", 100, 0, 100, 5);

    public bool showOfficer { get; set; }
    public DateTime? lastKilled { get; set; }

    public Detective(PlayerControl player) {
        this.player = player;
        name = "Detective";
        color = new Color(0, 40f / 255f, 198f / 255f, 1);
        startText = "Shoot the [FF0000FF]Impostor";
    }


    public static void ClearSettings()
    {
        player = null;
        lastKilled = null;
    }

    public static void SetConfigSettings()
    {
        showOfficer = HarmonyMain.showOfficer.GetValue();
        OfficerCD = HarmonyMain.DetectiveKillCooldown.GetValue();
    }

    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            Detective detective = GetSpecialRole<Detective>(PlayerControl.LocalPlayer.PlayerId);
            detective.setIntro(__instance);
            
            // TODO: Wieso zur Hölle???
            lastKilled = DateTime.UtcNow.AddSeconds((player.playerId * -1) + 10 + __instance.timer_0);
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class MeetingEnd
    {
        static void Postfix(ExileController __instance)
        {
            Detective detective = GetSpecialRole<Detective>(PlayerControl.LocalPlayer.PlayerId);
            detectivelastKilled = DateTime.UtcNow.AddMilliseconds(__instance.Duration);
        }
    }
}