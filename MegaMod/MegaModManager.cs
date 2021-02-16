using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppSystem.Xml.Schema;
using UnityEngine;
using Reactor.Unstrip;
using Reactor.Extensions;

/*
Hex colors for extra roles
Engineer: 972e00
Jester: 838383
Doctor: 24b720
Detective: 0028c6
*/

namespace MegaMod
{
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
        public DeathReason DeathReason { get; set; }
    }
    //body report class for when medic reports a body
    public class BodyReport
    {
        public DeathReason DeathReason { get; set; }
        public PlayerControl Killer { get; set; }
        public PlayerControl Reporter { get; set; }
        public float KillAge { get; set; }

        public static string ParseBodyReport(BodyReport br)
        {
            System.Console.WriteLine(br.KillAge);
            if (br.KillAge > MegaModManager.Doctor.medicKillerColorDuration * 1000)
            {
                return $"Body Report: The corpse is too old to gain information from. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else if (br.DeathReason == (DeathReason)3)
            {
                return $"Body Report (Officer): The cause of death appears to be suicide! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else if (br.KillAge < MegaModManager.Doctor.medicKillerNameDuration * 1000)
            {
                return $"Body Report: The killer appears to be {br.Killer.name}! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else
            {
                //TODO (make the type of color be written to chat
                var colors = new Dictionary<byte, string>()
                {
                    {0, "darker"},
                    {1, "darker"},
                    {2, "darker"},
                    {3, "lighter"},
                    {4, "lighter"},
                    {5, "lighter"},
                    {6, "darker"},
                    {7, "lighter"},
                    {8, "darker"},
                    {9, "darker"},
                    {10, "lighter"},
                    {11, "lighter"},
                };
                var typeOfColor = colors[br.Killer.Data.ColorId];
                return $"Body Report: The killer appears to be a {typeOfColor} color. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
        }
    }

    [HarmonyPatch]
    public static class MegaModManager
    {
        public static AssetBundle bundle;
        public static AudioClip breakClip;
        public static Sprite repairIco;
        public static Sprite shieldIco;
        public static Sprite smallShieldIco;

        public static Dictionary<byte, Role> assignedSpecialRoles;

        // Only the engineer gets added to the dictionary so far
        public static void AddSpecialRole(Role specialRole){
            if(assignedSpecialRoles == null)
                assignedSpecialRoles = new Dictionary<byte, Role>();
            assignedSpecialRoles.Add(specialRole.player.PlayerId, specialRole);
        }

        public static T GetSpecialRole<T>(byte playerId) where T : Role => (T) assignedSpecialRoles[playerId];

        public static bool SpecialRoleIsAssigned<T>(out Role instance) where T : Role
        {
            List<Role> assignedRoles = assignedSpecialRoles.Values.ToList();
            foreach(Role role in assignedRoles)
            {
                if (!(role is T)) continue;
                instance = role;
                return true;
            }                
            instance = null;
            return false;
        }

        public static void WriteImmediately(object action)
        {
            MessageWriter writer = GetWriter(action);
            CloseWriter(writer);
        }

        public static MessageWriter GetWriter(object action)
        {
           return AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) action, Hazel.SendOption.None, -1);
        }

        public static void CloseWriter(MessageWriter writer)
        {
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static Color VecToColor(Vector3 vec) => new Color(vec.x, vec.y, vec.z);

        public static Vector3 ColorToVec(Color color) => new Vector3(color.r, color.g, color.b);

        public static GameObject rend;
        //rudimentary array to convert a byte setting from config into true/false
        public static bool[] byteBool =
        {
            false,
            true
        };
        public static List<DeadPlayer> killedPlayers = new List<DeadPlayer>();
        public static PlayerControl CurrentTarget = null;
        public static PlayerControl localPlayer = null;
        public static List<PlayerControl> localPlayers = new List<PlayerControl>();
        //global rng
        public static System.Random rng = new System.Random();
        //the kill button in the bottom right
        public static KillButtonManager KillButton;
        //the id of the targeted player
        public static int KBTarget;
        //distance between the local player and closest player
        public static double DistLocalClosest;
        //shield indicator sprite (placeholder)
        public static GameObject shieldIndicator = null;
        //renderer for the shield indicator
        public static SpriteRenderer shieldRenderer = null;
        //medic settings and values
        public static string versionString = "v0.0.1";

        public static class ModdedPalette
        {
            public static Color protectedColor = new Color(0, 1, 1, 1);
        }
        

        //function called on start of game. write version text on menu
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionStartPatch
        {
            static void Postfix(VersionShower __instance)
            {
                __instance.text.Text += $"    MegaMod {versionString}";
            }
        }

        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class AmBannedPatch
        {
            public static void Postfix(out bool __result)
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.Text += $"\nMegaMod {versionString}";
            }
        }

        [HarmonyPatch(typeof(ShipStatus), "GetSpawnLocation")]
        public static class StartGamePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                ConsoleTools.Info("Game Started!");
            }
        }
    }
}