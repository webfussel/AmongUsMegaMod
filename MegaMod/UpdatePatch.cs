using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static MegaMod.MegaMod;

namespace MegaMod
{
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
    class GameOptionsData_ToHudString
    {
        static void Postfix(ref string __result)
        {
            DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.5f;
        }
    }


    //This is a class that sends a ping to my public api so people can see a player counter. Go to http://computable.us:5001/api/playercount to view the people currently playing.
    //No sensitive information is logged, viewed, or used in any way.
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
    class GameUpdate
    {
        static readonly HttpClient client = new HttpClient();
        static DateTime? lastGuid = null;
        static Guid clientGuid = Guid.NewGuid();
        static void Postfix()
        {
            if (!lastGuid.HasValue)
            {
                lastGuid = DateTime.UtcNow.AddSeconds(-20);
            }
            if (lastGuid.Value.AddSeconds(20).Ticks < DateTime.UtcNow.Ticks)
            {
                client.PostAsync("http://computable.us:5001/api/ping?guid=" + clientGuid, null);
                lastGuid = DateTime.UtcNow;
            }
        }
    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static bool defaultSet = false;
        static bool lastQ = false;
        static int currentColor = 0;
        static Color newColor;
        static Color nextColor;
        static Color[] colors = { Color.red, new Color(255f / 255f, 94f / 255f, 19f / 255f), Color.yellow, Color.green, Color.blue, new Color(120f / 255f, 7f / 255f, 188f / 255f) };
        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.PlayerName == "Hunter")
                    {
                        if (!defaultSet)
                        {
                            System.Console.Write(currentColor);
                            defaultSet = true;
                            player.myRend.material.SetColor("_BackColor", colors[currentColor]);
                            player.myRend.material.SetColor("_BodyColor", colors[currentColor]);
                            newColor = colors[currentColor];
                            if (currentColor + 1 >= colors.Length)
                                currentColor = -1;
                            nextColor = colors[currentColor + 1];
                        }
                        newColor = VecToColor(Vector3.MoveTowards(ColorToVec(newColor), ColorToVec(nextColor), 0.02f));
                        player.myRend.material.SetColor("_BackColor", newColor);
                        player.myRend.material.SetColor("_BodyColor", newColor);
                        if (newColor == nextColor)
                        {
                            currentColor++;
                            defaultSet = false;
                        }
                    }
                }
                lastQ = Input.GetKeyUp(KeyCode.Q);
                KillButton = __instance.KillButton;
                PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(PlayerControl.LocalPlayer);
                DistLocalClosest = PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
                if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ && __instance.UseButton.isActiveAndEnabled)
                {
                    PerformKillPatch.Prefix();
                }
                if (Doctor.Protected != null && Doctor.Protected.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    if (rend == null)
                    {
                        rend = new GameObject("Shield Icon");
                        rend.AddComponent<SpriteRenderer>().sprite = smallShieldIco;
                    }
                    int scale;
                    if (Screen.width > Screen.height)
                        scale = Screen.width / 800;
                    else
                        scale = Screen.height / 600;
                    rend.transform.localPosition = Camera.main.ScreenToWorldPoint(new Vector3(0 + (25 * scale), 0 + (25 * scale), -50f));
                    rend.SetActive(true);
                }
                if (Engineer.player != null && Engineer.player.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, 1f);
                    KillButton.renderer.sprite = repairIco;
                    KillButton.renderer.color = Palette.EnabledColor;
                    KillButton.renderer.material.SetFloat("_Desat", 0f);
                }
                if (Jester.Joker != null)
                    Jester.ClearTasks();
                if (rend != null)
                    rend.SetActive(false);
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms)
                        sabotageActive = true;
                Engineer.sabotageActive = sabotageActive;
                if (Doctor.Protected != null && Doctor.Protected.Data.IsDead)
                    BreakShield(true);
                if (Doctor.Protected != null && Doctor.Medic != null && Doctor.Medic.Data.IsDead)
                    BreakShield(true);
                if (Doctor.Medic == null && Doctor.Protected != null)
                    BreakShield(true);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    player.nameText.Color = Color.white;
                if (PlayerControl.LocalPlayer.Data.IsImpostor || Jester.Joker == PlayerControl.LocalPlayer && Jester.showImpostorToJoker)
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.Data.IsImpostor)
                            player.nameText.Color = Color.red;
                if (Doctor.Medic != null)
                {
                    if (Doctor.Medic == PlayerControl.LocalPlayer || Doctor.showMedic)
                    {
                        Doctor.Medic.nameText.Color = ModdedPalette.medicColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && Doctor.Medic.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.medicColor;
                    }
                }
                if (Detective.Officer != null)
                {
                    if (Detective.Officer == PlayerControl.LocalPlayer || Detective.showOfficer)
                    {
                        Detective.Officer.nameText.Color = ModdedPalette.officerColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && Detective.Officer.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.officerColor;
                    }
                }
                if (Engineer.player != null)
                {
                    if (Engineer.player == PlayerControl.LocalPlayer || Engineer.showEngineer)
                    {
                        Engineer.player.nameText.Color = Engineer.color;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && Engineer.player.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.engineerColor;
                    }
                }
                if (Jester.Joker != null)
                {
                    if (Jester.Joker == PlayerControl.LocalPlayer || Jester.showJoker)
                    {
                        Jester.Joker.nameText.Color = ModdedPalette.jokerColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && Jester.Joker.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.jokerColor;
                    }
                }
                    
                if (Doctor.Protected != null)
                {
                    int showShielded = Doctor.showProtected;
                    // If everyone can see shielded
                    if(showShielded == 3)
                    {
                        Doctor.Protected.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                        Doctor.Protected.myRend.material.SetFloat("_Outline", 1f);
                        Doctor.Protected.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
                    }
                    // If I am protected and should see the shield
                    else if (PlayerControl.LocalPlayer == Doctor.Protected && (showShielded == 0 || showShielded == 2))
                    {
                        Doctor.Protected.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                        Doctor.Protected.myRend.material.SetFloat("_Outline", 1f);
                        Doctor.Protected.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
                    }
                    // If I am Medic and should see the shield
                    else if(PlayerControl.LocalPlayer == Doctor.Medic && (showShielded == 1 || showShielded == 2))
                    {
                        Doctor.Protected.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                        Doctor.Protected.myRend.material.SetFloat("_Outline", 1f);
                        Doctor.Protected.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
                    }
                }
                        
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (Engineer.player == null || Engineer.player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        KillButton.gameObject.SetActive(false);
                        KillButton.renderer.enabled = false;
                        KillButton.isActive = false;
                        KillButton.SetTarget(null);
                        KillButton.enabled = false;
                        return;
                    }
                }
                if (Doctor.Medic != null && __instance.UseButton != null && Doctor.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.renderer.sprite = shieldIco;
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, 1f);
                    if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance] && Doctor.shieldUsed == false)
                    {
                        KillButton.SetTarget(PlayerTools.closestPlayer);
                        CurrentTarget = PlayerTools.closestPlayer;
                    }
                    else
                    {
                        KillButton.SetTarget(null);
                        CurrentTarget = null;
                    }
                }
                if (Detective.Officer != null && __instance.UseButton != null && Detective.Officer.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(PlayerTools.GetOfficerKD(), PlayerControl.GameOptions.KillCooldown + 15.0f);
                    if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance])
                    {
                        KillButton.SetTarget(PlayerTools.closestPlayer);
                        CurrentTarget = PlayerTools.closestPlayer;
                    }
                    else
                    {
                        KillButton.SetTarget(null);
                        CurrentTarget = null;
                    }
                }
            }
        }
    }
}
