using System;
using UnityEngine;

public class Jester : Role
{
    public bool showJoker = false;
    public bool showImpostorToJoker = false;
    public bool jokerCanDieToOfficer = false;


    public Jester(PlayerControl player)
    {
        this.player = player;
        name = "Jester";
        color = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);
        startText = "Get voted off of the ship to win";
    }
    public static void ClearSettings()
    {
        player = null;
    }

    public static void ClearTasks()
    {
        var removeTask = new List<PlayerTask>();
        foreach (PlayerTask task in Jester.player.myTasks)
            if (task.TaskType != TaskTypes.FixComms && task.TaskType != TaskTypes.FixLights && task.TaskType != TaskTypes.ResetReactor && task.TaskType != TaskTypes.ResetSeismic && task.TaskType != TaskTypes.RestoreOxy)
                removeTask.Add(task);
        foreach (PlayerTask task in removeTask)
            Jester.player.RemoveTask(task);
    }

    public static void SetConfigSettings()
    {
        showJoker = HarmonyMain.showJoker.GetValue();
        showImpostorToJoker = HarmonyMain.showImpostorToJoker.GetValue();
        jokerCanDieToOfficer = HarmonyMain.jokerCanDieToOfficer.GetValue();
    }
}