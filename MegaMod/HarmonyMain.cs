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
using System.Collections.Generic;

namespace MegaMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class HarmonyMain : BasePlugin
    {
        public const string Id = "gg.reactor.megamod";
        public Harmony Harmony { get; } = new Harmony(Id);

        // Spawn Chances
        public static CustomNumberOption optEngineerSpawnChance = CustomOption.AddNumber("Engineer Spawn Chance", 100, 0, 100, 5);
        public static CustomNumberOption optDetectiveSpawnChance = CustomOption.AddNumber("Detective Spawn Chance", 100, 0, 100, 5);
        public static CustomNumberOption optDoctorSpawnChance = CustomOption.AddNumber("Doctor Spawn Chance", 100, 0, 100, 5);
        public static CustomNumberOption optJesterSpawnChance = CustomOption.AddNumber("Jester Spawn Chance", 100, 0, 100, 5);
        
        // Detective
        public static CustomNumberOption optDetectiveKillCooldown = CustomOption.AddNumber("Detective Kill Cooldown", 30f, 10f, 60f, 2.5f);
        
        // Doctor
        public static CustomStringOption optDoctorShowShieldedPlayer = CustomOption.AddString("Show Shielded Player", new[] { "Self", "Doctor", "Self+Doctor", "Everyone" });
        public static CustomToggleOption optDoctorPlayerMurderIndicator = CustomOption.AddToggle("Murder Attempt Indicator for Shielded Player", true);
        public static CustomToggleOption optDoctorReportSwitch = CustomOption.AddToggle("Show Doctor Reports", true);
        public static CustomNumberOption optDoctorReportNameDuration = CustomOption.AddNumber("Time Where Doctor Reports Will Have Name", 5, 0, 60, 2.5f);
        public static CustomNumberOption optDoctorReportColorDuration = CustomOption.AddNumber("Time Where Doctor Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomNumberOption optDoctorShieldCooldown = CustomOption.AddNumber("Detective Kill Cooldown", 30f, 10f, 60f, 2.5f);
        
        // Jester
        public static CustomToggleOption optJesterShowImpostor = CustomOption.AddToggle("Show Impostor to Jester", false);
        public static CustomToggleOption optJesterCanDieToDetective = CustomOption.AddToggle("Jester Can Die To Detective", true);
        
        public ConfigEntry<string> Ip { get; set; }
        public ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            Ip = Config.Bind("Custom", "Ipv4 or Hostname", "127.0.0.1");
            Port = Config.Bind("Custom", "Port", (ushort)22023);

            bundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\bundle");
            breakClip = bundle.LoadAsset<AudioClip>("SB").DontUnload();

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
