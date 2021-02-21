using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using System.Net;
using Reactor;
using static MegaMod.MegaModManager;
using Reactor.Unstrip;
using UnityEngine;
using System.IO;
using Essentials.CustomOptions;
using Reactor.Extensions;

namespace MegaMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class HarmonyMain : BasePlugin
    {
        private const string Id = "gg.reactor.megamod";
        private Harmony Harmony { get; } = new Harmony(Id);

        // Spawn Chances
        public static readonly CustomNumberOption OptEngineerSpawnChance = CustomOption.AddNumber("Engineer Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptDetectiveSpawnChance = CustomOption.AddNumber("Detective Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptDoctorSpawnChance = CustomOption.AddNumber("Doctor Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptManiacSpawnChance = CustomOption.AddNumber("Maniac Spawn Chance", 100, 0, 100, 10);
        
        // Detective
        public static readonly CustomNumberOption OptDetectiveKillCooldown = CustomOption.AddNumber("Detective Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static readonly CustomToggleOption ShowDetectiveReports = CustomOption.AddToggle("Show Detective Reports", true);
        
        // Doctor
        public static readonly CustomStringOption OptDoctorShowShieldedPlayer = CustomOption.AddString("Show Shielded Player", new[] { "Self", "Doctor", "Self+Doctor", "Everyone" });
        public static readonly CustomToggleOption OptDoctorPlayerMurderIndicator = CustomOption.AddToggle("Murder Attempt Indicator for Shielded Player", true);
        
        // Jester
        public static readonly CustomToggleOption OptManiacShowImpostor = CustomOption.AddToggle("Show Impostor to Maniac", false);
        public static readonly CustomToggleOption OptManiacCanDieToDetective = CustomOption.AddToggle("Maniac Can Die To Detective", true);

        private ConfigEntry<string> Ip { get; set; }
        private ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            Ip = Config.Bind("Custom", "Ipv4 or Hostname", "127.0.0.1");
            Port = Config.Bind("Custom", "Port", (ushort)22023);

            bundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\bundle");
            breakClip = bundle.LoadAsset<AudioClip>("SB").DontUnload();
            
            buttons = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\buttons");

            var defaultRegions = ServerManager.DefaultRegions.ToList();
            var ip = Ip.Value;
            if (Uri.CheckHostName(Ip.Value).ToString() == "Dns")
            {
                foreach (IPAddress address in Dns.GetHostAddresses(Ip.Value))
                {
                    if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) continue;
                    ip = address.ToString(); break;
                }
            }

            defaultRegions.Insert(0, new RegionInfo(
                "Custom", ip, new[]
                {
                    new ServerInfo($"Custom-Server", ip, Port.Value)
                })
            );

            ServerManager.DefaultRegions = defaultRegions.ToArray();
            Harmony.PatchAll();
        }
    }
}
