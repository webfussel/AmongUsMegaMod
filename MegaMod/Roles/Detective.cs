using Essentials.CustomOptions;
using HarmonyLib;
using Hazel;
using MegaMod;
using static MegaMod.MegaModManager;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Detective : Role
{
    public static CustomNumberOption optDetectiveKillCooldown = CustomOption.AddNumber("Detective Kill Cooldown", 30f, 10f, 60f, 2.5f);
    public static CustomNumberOption optDetectiveSpawnChance = CustomOption.AddNumber("Detective Spawn Chance", 100, 0, 100, 5);

    public DateTime? lastKilled { get; set; }
    public float cooldown { get; set; }

    public Detective(PlayerControl player) {
        this.player = player;
        name = "Detective";
        color = new Color(0, 40f / 255f, 198f / 255f, 1);
        startText = "Shoot the [FF0000FF]Impostor";
        cooldown = optDetectiveKillCooldown.GetValue();
    }


    public override void ClearSettings()
    {
        player = null;
        lastKilled = null;
    }

    public override void SetConfigSettings()
    {
        cooldown = optDetectiveKillCooldown.GetValue();
    }

    public void CheckKillButton(HudManager instance)
    {
        if (instance.UseButton == null || !instance.UseButton.isActiveAndEnabled) return;
        
        KillButtonManager killButton = instance.KillButton;
        killButton.gameObject.SetActive(true);
        killButton.isActive = true;
        killButton.SetCoolDown(PlayerTools.GetOfficerKD(), PlayerControl.GameOptions.KillCooldown + 15.0f);
        if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance])
        {
            killButton.SetTarget(PlayerTools.closestPlayer);
            CurrentTarget = PlayerTools.closestPlayer;
        }
        else
        {
            killButton.SetTarget(null);
            CurrentTarget = null;
        }
    }

    private void KillPlayer(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveKill, Hazel.SendOption.None, -1);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(player.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        PlayerControl.LocalPlayer.MurderPlayer(player);
        lastKilled = DateTime.UtcNow;
    }

    public float GetCurrentCooldown()
    {
        if (lastKilled == null) return cooldown;
        
        TimeSpan diff = (TimeSpan) (DateTime.UtcNow - lastKilled);

        float cooldownMS = cooldown * 1000.0f;
        if (diff.TotalMilliseconds > cooldownMS) return 0;
        return (float) (cooldownMS - diff.TotalMilliseconds) / 1000.0f;
    }

    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            Detective detective = GetSpecialRole<Detective>(PlayerControl.LocalPlayer.PlayerId);
            detective.setIntro(__instance);
            
            // TODO: Wieso zur Hölle???
            detective.lastKilled = DateTime.UtcNow.AddSeconds((player.playerId * -1) + 10 + __instance.timer_0);
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class MeetingEnd
    {
        static void Postfix(ExileController __instance)
        {
            Detective detective = GetSpecialRole<Detective>(PlayerControl.LocalPlayer.PlayerId);
            detective.lastKilled = DateTime.UtcNow.AddMilliseconds(__instance.Duration);
        }
    }

    public bool KillOrCommitSuicide(PlayerControl target)
    {
        if (target == null || GetCurrentCooldown() != 0) return false;
        
        if (
            //check if they're jester and the setting is configured
            (SpecialRoleIsAssigned<Jester>(out KeyValuePair<byte, Jester> jesterKvp) && target.PlayerId == jesterKvp.Key && jesterKvp.Value.jesterCanDieToDetective)
            //or if they're an impostor
            || target.Data.IsImpostor
        )
            KillPlayer(target);
        //else, they're innocent or shielded by the doctor
        else
            KillPlayer(player);
        return false;
    }
}