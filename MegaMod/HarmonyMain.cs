using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using System.Net;
using Reactor;
using Essentials.CustomOptions;
using static MegaMod.MegaModManager;
using Reactor.Unstrip;
using UnityEngine;
using System.IO;
using Reactor.Extensions;

namespace MegaMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class HarmonyMain : BasePlugin
    {
        public const string Id = "gg.reactor.betterroles";

        public Harmony Harmony { get; } = new Harmony(Id);

        //This section uses the https://github.com/DorCoMaNdO/Reactor-Essentials framework, but I disabled the watermark.
        //The code said that you were allowed, as long as you provided credit elsewhere. 
        //I added a link in the Credits of the GitHub page, and I'm also mentioning it here.
        //If the owner of this library has any problems with this, just message me on discord and we'll find a solution

        //Hunter101#1337

        // TODO: Schauen, ob Optionen aus Klassen da sind, sobald wieder gbeaut werden kann

        public static CustomToggleOption showJoker = CustomOption.AddToggle("Show Joker", false);
        public static CustomToggleOption showImpostorToJoker = CustomOption.AddToggle("Show Impostor to Joker", false);
        public static CustomToggleOption jokerCanDieToOfficer = CustomOption.AddToggle("Joker Can Die To Officer", true);
        public static CustomNumberOption jokerSpawnChance = CustomOption.AddNumber("Joker Spawn Chance", 100, 0, 100, 5);
        
        public ConfigEntry<string> Ip { get; set; }
        public ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            Ip = Config.Bind("Custom", "Ipv4 or Hostname", "127.0.0.1");
            Port = Config.Bind("Custom", "Port", (ushort)22023);

            bundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\bundle");
            breakClip = bundle.LoadAsset<AudioClip>("SB").DontUnload();
            repairIco = bundle.LoadAsset<Sprite>("RE").DontUnload();
            shieldIco = bundle.LoadAsset<Sprite>("SA").DontUnload();
            smallShieldIco = bundle.LoadAsset<Sprite>("RESmall").DontUnload();

            var defaultRegions = ServerManager.DefaultRegions.ToList();
            var ip = Ip.Value;
            if (Uri.CheckHostName(Ip.Value).ToString() == "Dns")
            {
                foreach (IPAddress address in Dns.GetHostAddresses(Ip.Value))
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = address.ToString(); break;
                    }
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
