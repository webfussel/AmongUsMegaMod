using System;
using UnityEngine;

public class Jester
{
    public static Color color = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);
    public static PlayerControl Joker;
    public static bool showJoker = false;
    public static bool showImpostorToJoker = false;
    public static bool jokerCanDieToOfficer = false;

    public static void ClearSettings()
    {
        Joker = null;
    }

    public static void ClearTasks()
    {
        var removeTask = new List<PlayerTask>();
        foreach (PlayerTask task in Jester.Joker.myTasks)
            if (task.TaskType != TaskTypes.FixComms && task.TaskType != TaskTypes.FixLights && task.TaskType != TaskTypes.ResetReactor && task.TaskType != TaskTypes.ResetSeismic && task.TaskType != TaskTypes.RestoreOxy)
                removeTask.Add(task);
        foreach (PlayerTask task in removeTask)
            Jester.Joker.RemoveTask(task);
    }

    public static void SetConfigSettings()
    {
        showJoker = HarmonyMain.showJoker.GetValue();
        showImpostorToJoker = HarmonyMain.showImpostorToJoker.GetValue();
        jokerCanDieToOfficer = HarmonyMain.jokerCanDieToOfficer.GetValue();
    }
}