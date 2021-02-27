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
    public class MainConfig : BasePlugin
    {
        private const string Id = "gg.reactor.megamod";
        private Harmony Harmony { get; } = new Harmony(Id);

        // Spawn Chances
        public static readonly CustomNumberOption OptEngineerSpawnChance = CustomOption.AddNumber("Engineer Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptDetectiveSpawnChance = CustomOption.AddNumber("Detective Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptDoctorSpawnChance = CustomOption.AddNumber("Doctor Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptSeerSpawnChance = CustomOption.AddNumber("Seer Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptTrackerSpawnChance = CustomOption.AddNumber("Tracker Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptNocturnalSpawnChance = CustomOption.AddNumber("Nocturnal Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptManiacSpawnChance = CustomOption.AddNumber("Maniac Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptNinjaSpawnChance = CustomOption.AddNumber("Ninja Spawn Chance", 100, 0, 100, 10);

        // Detective
        public static readonly CustomNumberOption OptDetectiveKillCooldown = CustomOption.AddNumber("Detective Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static readonly CustomToggleOption OptShowDetectiveReports = CustomOption.AddToggle("Show Detective Reports", true);
        
        // Doctor
        public static readonly CustomStringOption OptDoctorShowShieldedPlayer = CustomOption.AddString("Show Shielded Player", new[] { "Self", "Doctor", "Self+Doctor", "Everyone" });
        public static readonly CustomToggleOption OptDoctorPlayerMurderIndicator = CustomOption.AddToggle("Murder Attempt Indicator for Shielded Player", true);
        
        // Seer
        public static readonly CustomToggleOption OptSeerCanPressEmergency = CustomOption.AddToggle("Seer can call emergency", false);

        // Nocturnal
        public static readonly CustomNumberOption OptNocturnalDarkVision = CustomOption.AddNumber("Nocturnal Dark Vision", 3, 1, 3, 0.5f);

        // Maniac
        public static readonly CustomToggleOption OptManiacShowImpostor = CustomOption.AddToggle("Show Impostor to Maniac", false);
        public static readonly CustomToggleOption OptManiacCanDieToDetective = CustomOption.AddToggle("Maniac Can Die To Detective", true);

        private ConfigEntry<string> Ip { get; set; }
        private ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            int originalPaletteCount = Palette.ShortColorNames.Count;
            int colorCount = originalPaletteCount + PalettePatch.ShortColorNames.Length;
            string[] allNames = new string[colorCount];
            Color32[] allColors = new Color32[colorCount];
            Color32[] allShadows = new Color32[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                if (i < originalPaletteCount)
                {
                    allNames[i] = Palette.ShortColorNames[i];
                    allColors[i] = Palette.PlayerColors[i];
                    allShadows[i] = Palette.ShadowColors[i];
                }
                else
                {
                    allNames[i] = PalettePatch.ShortColorNames[i - originalPaletteCount];
                    allColors[i] = PalettePatch.PlayerColors[i - originalPaletteCount];
                    allShadows[i] = PalettePatch.ShadowColors[i - originalPaletteCount];
                }
            }
            
            Palette.ShortColorNames = allNames;
            Palette.PlayerColors = allColors;
            Palette.ShadowColors = allShadows;
            
            Ip = Config.Bind("Custom", "Ipv4 or Hostname", "127.0.0.1");
            Port = Config.Bind("Custom", "Port", (ushort)22023);

            
            buttons = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\buttons");
            repairButton = buttons.LoadAsset<Sprite>("repair").DontUnload();
            markTrapButton = buttons.LoadAsset<Sprite>("mark").DontUnload();
            shieldButton = buttons.LoadAsset<Sprite>("protect").DontUnload();

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
