using System;
using System.Collections.Generic;
using static MegaMod.MegaModManager; // TODO: wtf?

namespace MegaMod
{
    public class BodyReport
    {
        public DeathReason DeathReason { get; set; }
        public PlayerControl Killer { get; set; }
        public PlayerControl Reporter { get; set; }
        public float KillAge { get; set; }

        public static string ParseBodyReport(BodyReport br)
        {
            System.Console.WriteLine(br.KillAge);
            Doctor doctor = GetSpecialRole<Doctor>(PlayerControl.LocalPlayer.PlayerId);
            
            if (br.KillAge > doctor.doctorKillerColorDuration * 1000)
            {
                return $"Body Report: The corpse is too old to gain information from. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            
            if (br.DeathReason == DEATH_REASON_SUICIDE)
            {
                return $"Body Report (Officer): The cause of death appears to be suicide! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            
            if (br.KillAge < doctor.doctorKillerNameDuration * 1000)
            {
                return $"Body Report: The killer appears to be {br.Killer.name}! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            
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
            
            return $"Body Report: The killer appears to be a {colors[br.Killer.Data.ColorId]} color. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
        }
    }
}