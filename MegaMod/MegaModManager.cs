using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using MegaMod.Roles;
using UnityEngine;
using Reactor.Unstrip;

namespace MegaMod
{
    [HarmonyPatch]
    public static class MegaModManager
    {
        private const string VersionString = "b-1.2.0";

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
            SetRole = 42,
            SetProtected = 44,
            ShieldBreak = 45,
            DetectiveKill = 47,
            FixLights = 49,
            ResetVariables = 51,
            SetLocalPlayers = 56,
            ManiacWin = 57
        }
        
        public static AssetBundle bundle;
        public static AssetBundle buttons;
        public static AudioClip breakClip; // Still need that for the shield
        public static Sprite defaultKillButton;
        public static Sprite shieldButton;
        public static Sprite repairButton;
        public static readonly Dictionary<byte, Role> AssignedSpecialRoles;
        public static readonly List<DeadPlayer> KilledPlayers = new List<DeadPlayer>();

        // Only the engineer gets added to the dictionary so far
        public static void AddSpecialRole(Role specialRole)
        {
            if (AssignedSpecialRoles.ContainsKey(specialRole.player.PlayerId))
                AssignedSpecialRoles.Remove(specialRole.player.PlayerId);
            AssignedSpecialRoles.Add(specialRole.player.PlayerId, specialRole);
        }

        public static T GetSpecialRole<T>(byte playerId) where T : Role => AssignedSpecialRoles.TryGetValue(playerId, out Role role) ? (T) role : null;

        public static Role GetSpecialRole(byte playerId) => AssignedSpecialRoles.TryGetValue(playerId, out Role role) ? role : null;

        public static T GetSpecialRole<T>() where T : Role
        {
            List<Role> specialRoles = AssignedSpecialRoles.Values.ToList();
            return specialRoles.OfType<T>().FirstOrDefault();
        }

        public static bool TryGetSpecialRole<T>(byte playerId, out T role) where T : Role
        {
            if(AssignedSpecialRoles.TryGetValue(playerId, out Role tempRole) && tempRole is T value)
            {
                role = value;
                return true;
            }
            role  = null;
            return false;
        }

        public static bool SpecialRoleIsAssigned<T>(out KeyValuePair<byte, T> keyValuePair) where T : Role
        {
            foreach ((var key, Role value) in AssignedSpecialRoles.Where(kvp => kvp.Value is T))
            {
                keyValuePair = new KeyValuePair<byte, T>(key, (T) value);
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
           return AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) action, SendOption.None, -1);
        }

        public static void CloseWriter(MessageWriter writer)
        {
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void ResetValues()
        {
            AssignedSpecialRoles.Clear();
            KilledPlayers.Clear();
        }

        public static Color VecToColor(Vector3 vec) => new Color(vec.x, vec.y, vec.z);
        public static PlayerControl localPlayer = null;
        public static readonly List<PlayerControl> Crew = new List<PlayerControl>();
        public static readonly System.Random Rng = new System.Random();

        static MegaModManager()
        {
            AssignedSpecialRoles = new Dictionary<byte, Role>();
        }

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
                __instance.text.Text += $"    MegaMod {VersionString}";
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
                __instance.text.Text += $"\nMegaMod {VersionString}";
            }
        }
        
        /* Maybe we need that someday
        [HarmonyPatch(typeof(ShipStatus), "GetSpawnLocation")]
        public static class StartGamePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                ConsoleTools.Info("Game Started!");
            }
        }*/
    }
}