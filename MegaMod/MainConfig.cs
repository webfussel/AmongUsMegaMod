using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using System.Net;
using Reactor;
using static MegaMod.MegaModManager;
using UnityEngine;
using System.IO;
using Essentials.Options;
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
        public static readonly CustomNumberOption OptEngineerSpawnChance = CustomOption.AddNumber("(Crew) Engineer: Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptDetectiveSpawnChance = CustomOption.AddNumber("(Crew) Detective: Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptDoctorSpawnChance = CustomOption.AddNumber("(Crew) Doctor: Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptSeerSpawnChance = CustomOption.AddNumber("(Crew) Seer Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptTrackerSpawnChance = CustomOption.AddNumber("(Crew) Tracker Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptNocturnalSpawnChance = CustomOption.AddNumber("(Crew) Nocturnal Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptPathfinderSpawnChance = CustomOption.AddNumber("(Crew) Pathfinder Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptManiacSpawnChance = CustomOption.AddNumber("(Solo) Maniac Spawn Chance", 100, 0, 100, 10);
        public static readonly CustomNumberOption OptNinjaSpawnChance = CustomOption.AddNumber("(Imps) Ninja Spawn Chance", 100, 0, 100, 10);

        // Detective
        public static readonly CustomNumberOption OptDetectiveKillCooldown = CustomOption.AddNumber("Detective: Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static readonly CustomToggleOption OptShowDetectiveReports = CustomOption.AddToggle("Detective: Body Reports", true);
        public static readonly CustomToggleOption OptManiacCanDieToDetective = CustomOption.AddToggle("Detective: Can kill Maniac", true);
        
        // Doctor
        public static readonly CustomStringOption OptDoctorShowShieldedPlayer = CustomOption.AddString("Doctor: Show Shielded Player", new[] { "Self", "Doctor", "Self+Doctor", "Everyone" });
        public static readonly CustomToggleOption OptDoctorPlayerMurderIndicator = CustomOption.AddToggle("Doctor: Shielded Player Murder Attempt Indicator", true);
        
        // Seer
        public static readonly CustomToggleOption OptSeerCanPressEmergency = CustomOption.AddToggle("Seer: Can call emergency", false);

        // Pathfinder
        public static readonly CustomNumberOption OptPathfinderFootprintLifespan = CustomOption.AddNumber("Pathfinder: Footprint lifespan", 4f, 1f, 8f, 1f);
        public static readonly CustomNumberOption OptPathfinderFootprintInterval = CustomOption.AddNumber("Pathfinder: Footprint interval", 0.3f, 0.1f, 0.5f, 0.1f);
        public static readonly CustomToggleOption OptPathfinderAnonymousFootprints = CustomOption.AddToggle("Pathfinder: Anonymous footprints", false);

        // Maniac
        public static readonly CustomToggleOption OptManiacShowImpostor = CustomOption.AddToggle("Maniac: Can see all Roles", false);

        private ConfigEntry<string> Ip { get; set; }
        private ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            int originalPaletteCount = Palette.ShortColorNames.Count;
            int colorCount = originalPaletteCount + PalettePatch.ShortColorNames.Length;
            StringNames[] allNames = new StringNames[colorCount];
            StringNames[] allShortNames = new StringNames[colorCount];
            Color32[] allColors = new Color32[colorCount];
            Color32[] allShadows = new Color32[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                if (i < originalPaletteCount)
                {
                    allNames[i] = MedScanMinigame.ColorNames[i];
                    allShortNames[i] = Palette.ShortColorNames[i];
                    allColors[i] = Palette.PlayerColors[i];
                    allShadows[i] = Palette.ShadowColors[i];
                }
                else
                {
                    allNames[i] = PalettePatch.ColorNames[i - originalPaletteCount];
                    allShortNames[i] = PalettePatch.ShortColorNames[i - originalPaletteCount];
                    allColors[i] = PalettePatch.PlayerColors[i - originalPaletteCount];
                    allShadows[i] = PalettePatch.ShadowColors[i - originalPaletteCount];
                }
            }
            
            MedScanMinigame.ColorNames = allNames;
            Palette.ShortColorNames = allShortNames;
            Palette.PlayerColors = allColors;
            Palette.ShadowColors = allShadows;
            
            Ip = Config.Bind("Custom", "Ipv4 or Hostname", "127.0.0.1");
            Port = Config.Bind("Custom", "Port", (ushort)22023);
            
            AssetBundle buttons = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\buttons");
            repairButton = buttons.LoadAsset<Sprite>("repair").DontUnload();
            markTrapButton = buttons.LoadAsset<Sprite>("mark").DontUnload();
            shieldButton = buttons.LoadAsset<Sprite>("protect").DontUnload();

            AssetBundle gui = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\gui");
            Texture2D footstepsTemp = gui.LoadAsset<Texture2D>("footsteps");
            footsteps = Sprite.Create(footstepsTemp, new Rect(0, 0, footstepsTemp.width, footstepsTemp.height), new Vector2(0.5f, 0.7f)).DontUnload();

            AssetBundle sounds = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\sounds");
            shieldAttempt = sounds.LoadAsset<AudioClip>("shield").DontUnload();
            ninjaOne = sounds.LoadAsset<AudioClip>("ninja_1").DontUnload();
            ninjaTwo = sounds.LoadAsset<AudioClip>("ninja_2").DontUnload();
            
            
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
