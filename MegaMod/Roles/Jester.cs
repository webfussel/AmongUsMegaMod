using System;
using System.Collections.Generic;
using Essentials.CustomOptions;
using Hazel;
using MegaMod;
using static MegaMod.MegaModManager; // TODO: wtf?
using UnityEngine;

public class Jester : Role
{

    public static CustomToggleOption optShowImpostorToJoker = CustomOption.AddToggle("Show Impostor to Joker", false);
    public static CustomToggleOption optJesterCanDieToOfficer = CustomOption.AddToggle("Jester Can Die To Officer", true);
    public static CustomNumberOption optSpawnChance = CustomOption.AddNumber("Jester Spawn Chance", 100, 0, 100, 5);
    
    public bool showImpostorToJester = false;
    public bool jesterCanDieToDetective = false;


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
        foreach (PlayerTask task in player.myTasks)
            if (task.TaskType != TaskTypes.FixComms && task.TaskType != TaskTypes.FixLights && task.TaskType != TaskTypes.ResetReactor && task.TaskType != TaskTypes.ResetSeismic && task.TaskType != TaskTypes.RestoreOxy)
                removeTask.Add(task);
        foreach (PlayerTask task in removeTask)
            player.RemoveTask(task);
    }

    public override void SetConfigSettings()
    {
        showImpostorToJester = optShowImpostorToJoker.GetValue();
        jesterCanDieToDetective = optJesterCanDieToOfficer.GetValue();
    }

    public override void CheckDead(HudManager instance)
    {
        throw new NotImplementedException();
    }
    
    public static void SetRole(List<PlayerControl> crew)
    {
        bool spawnChanceAchieved = rng.Next(1, 101) <= optSpawnChance.GetValue();
        if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
        Jester jester = GetSpecialRole<Jester>(PlayerControl.LocalPlayer.PlayerId);
        int random = rng.Next(0, crew.Count);
        jester.player = crew[random];
        crew.RemoveAt(random);

        MessageWriter writer = GetWriter(CustomRPC.SetJester);
        writer.Write(jester.player.PlayerId);
        CloseWriter(writer);
    }
}