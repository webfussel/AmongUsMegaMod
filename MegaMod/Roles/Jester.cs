using System;
using System.Collections.Generic;
using Essentials.CustomOptions;
using Hazel;
using MegaMod;
using static MegaMod.MegaModManager; // TODO: wtf?
using UnityEngine;

public class Jester : Role
{
    public bool showImpostorToJester { get; set; }
    public bool jesterCanDieToDetective { get; set; }


    public Jester(PlayerControl player)
    {
        this.player = player;
        name = "Jester";
        color = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);
        startText = "Get voted off of the ship to win";
    }
    public override void ClearSettings()
    {
        player = null;
    }

    public void ClearTasks()
    {
        var removeTask = new List<PlayerTask>();
        foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            if (task.TaskType != TaskTypes.FixComms && task.TaskType != TaskTypes.FixLights && task.TaskType != TaskTypes.ResetReactor && task.TaskType != TaskTypes.ResetSeismic && task.TaskType != TaskTypes.RestoreOxy)
                removeTask.Add(task);
        foreach (PlayerTask task in removeTask)
            player.RemoveTask(task);
    }

    public override void SetConfigSettings()
    {
        showImpostorToJester = HarmonyMain.optJesterShowImpostor.GetValue();
        jesterCanDieToDetective = HarmonyMain.optJesterCanDieToOfficer.GetValue();
    }

    public override void CheckDead(HudManager instance)
    {
    }
    
    public static void SetRole(List<PlayerControl> crew)
    {
        ConsoleTools.Info("I am a jester! x)");
        bool spawnChanceAchieved = rng.Next(1, 101) <= HarmonyMain.optJesterSpawnChance.GetValue();
        if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
        int random = rng.Next(0, crew.Count);
        Jester jester = new Jester(crew[random]);
        crew.RemoveAt(random);

        MessageWriter writer = GetWriter(RPC.SetJester);
        writer.Write(jester.player.PlayerId);
        CloseWriter(writer);
    }

    public override void SetIntro(IntroCutscene.CoBegin__d __instance)
    {
        base.SetIntro(__instance);
        var jesterTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        jesterTeam.Add(PlayerControl.LocalPlayer);
        __instance.yourTeam = jesterTeam;
    }
}