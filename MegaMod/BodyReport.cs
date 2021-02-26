using System;
using System.Collections.Generic;
using System.Linq;
using MegaMod.Roles;
using static MegaMod.MegaModManager; // TODO: wtf?

namespace MegaMod
{
    public class BodyReport
    {
        public PlayerControl Killer { get; set; }
        public DeadPlayer DeadPlayer { get; set; }
        public float KillAge { get; set; }
            
        // TODO: Add new colors from mod
        private readonly Dictionary<byte, string> _colors = new Dictionary<byte, string>
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
            {12, "lighter"},
            {13, "darker"},
            {14, "lighter"},
            {15, "darker"},
            {16, "darker"},
            {17, "lighter"},
            {18, "darker"},
            {19, "lighter"},
            {20, "lighter"},
            {21, "darker"},
            {22, "lighter"},
            {23, "lighter"},
            {24, "lighter"}
        };

        private readonly List<string> _lastWords = new List<string>
        {
            "YOLO!",
            "Oh no! It's YOU!?",
            "I wonder where the toilets are...",
            "I wonder if my pony could fly!",
            "I don't wanna die as a virgin!.... What do you mean \"Wish granted?\"",
            "That's a cool knife you've got there!",
            "Why is your tongue so big?",
            "NO REGERTS!"
        };

        public string ParseBodyReport()
        {
            String roleName = "Crewmate";
            if (TryGetSpecialRole<Role>(DeadPlayer.Victim.PlayerId, out var roleKvp))
            {
                roleName = roleKvp.name;
            } else if (DeadPlayer.Killer.Data.IsImpostor)
            {
                roleName = "Impostor";
            }
            
            List<string> hints = new List<string>()
            {
                $"The player was killed {Math.Round(KillAge / 1000)}s ago",
                $"The killer seems to have the Letter \"{Killer.name[Rng.Next(0, Killer.name.Length)]}\" in their name.",
                $"The Kill seems to be of a {_colors[Killer.Data.ColorId]} color",
                $"The Killer seems to have already killed {KilledPlayers.Count(x => x.Killer.PlayerId == Killer.PlayerId) - 1} other Crewmates.",
                $"It seems like the Victim had the role \"{roleName}\"",
                $"The last words of the Victim were: \"{_lastWords[Rng.Next(0, _lastWords.Count)]}\"",
                "Well, there goes my lunch...",
                "Urgs, a corpse!"
            };

            foreach (var hint in hints)
            {
                ConsoleTools.Info(hint);
            }

            return hints[Rng.Next(0, hints.Count)];
        }
    }
}