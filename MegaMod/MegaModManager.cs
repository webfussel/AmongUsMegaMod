using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Essentials.CustomOptions;
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
    [HarmonyPatch]
    public static class MegaModManager
    {
        public enum RPC
        {
            PlayAnimation = 0,
            CompleteTask = 1,
            SyncSettings = 2,
            SetInfected = 3,
            Exiled = 4,
            CheckName = 5,
            SetName = 6,
            CheckColor = 7,
            SetColor = 8,
            SetHat = 9,
            SetSkin = 10,
            ReportDeadBody = 11,
            MurderPlayer = 12,
            SendChat = 13,
            StartMeeting = 14,
            SetScanner = 15,
            SendChatNote = 16,
            SetPet = 17,
            SetStartCounter = 18,
            EnterVent = 19,
            ExitVent = 20,
            SnapTo = 21,
            Close = 22,
            VotingComplete = 23,
            CastVote = 24,
            ClearVote = 25,
            AddVote = 26,
            CloseDoorsOfType = 27,
            RepairSystem = 28,
            SetTasks = 29,
            UpdateGameData = 30,
            // --- Custom RPCs ---
            SetDoctor = 43,
            SetProtected = 44,
            ShieldBreak = 45,
            SetDetective = 46,
            DetectiveKill = 47,
            SetEngineer = 48,
            FixLights = 49,
            SetJester = 50,
            ResetVariables = 51,
            SetLocalPlayers = 56,
            JesterWin = 57,
        }
        
        public static AssetBundle bundle;
        public static AudioClip breakClip;
        public static Sprite repairIco;
        public static Sprite shieldIco;
        
        /* To cast into Deathreason, which are:
        Exile, (0)
        Kill, (1)
        Disconnect (2)
         */
        public static readonly DeathReason DEATH_REASON_SUICIDE = (DeathReason) 3;

        public static Dictionary<byte, Role> assignedSpecialRoles = new Dictionary<byte, Role>();

        // Only the engineer gets added to the dictionary so far
        public static void AddSpecialRole(Role specialRole)
        {
            assignedSpecialRoles.Add(specialRole.player.PlayerId, specialRole);
        }

        public static T GetSpecialRole<T>(byte playerId) where T : Role
        {
            return assignedSpecialRoles.TryGetValue(playerId, out Role role) ? (T) role : null;
        }

        public static Role GetSpecialRole(byte playerId)
        {
            return assignedSpecialRoles.TryGetValue(playerId, out Role role) ? role : null;
        }

        public static T GetSpecialRole<T>() where T : Role
        {
            List<Role> specialRoles = assignedSpecialRoles.Values.ToList();
            foreach(Role role in specialRoles)
                if (role is T)
                    return (T) role;
            return null;
        }

        public static bool TryGetSpecialRole<T>(byte playerId, out T role) where T : Role
        {
            if(assignedSpecialRoles.TryGetValue(playerId, out Role tempRole))
            {
                role = (T) tempRole;
                return true;
            }
            role  = null;
            return false;
        }

        public static bool SpecialRoleIsAssigned<T>(out KeyValuePair<byte, T> keyValuePair) where T : Role
        {
            foreach (var kvp in assignedSpecialRoles.Where(kvp => kvp.Value is T))
            {
                keyValuePair = new KeyValuePair<byte, T>(kvp.Key, (T) kvp.Value);
                return true;
            }
            keyValuePair = default;
            return false;
        }

        public static void WriteImmediately(RPC action)
        {
            MessageWriter writer = GetWriter(action);
            CloseWriter(writer);
        }

        public static MessageWriter GetWriter(RPC action)
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
        public static List<PlayerControl> crew = new List<PlayerControl>();
        //global rng
        public static System.Random rng = new System.Random();
        //the id of the targeted player
        public static int KBTarget;
        //distance between the local player and closest player
        public static double DistLocalClosest;
        //shield indicator sprite (placeholder)
        public static GameObject shieldIndicator = null;
        //renderer for the shield indicator
        public static SpriteRenderer shieldRenderer = null;
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