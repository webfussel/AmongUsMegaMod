using System;

namespace MegaMod
{
    public class DeadPlayer
    {
        public PlayerControl Killer { get; }
        public PlayerControl Victim { get; }
        public DateTime KillTime { get; }
        public DeathReason DeathReason { get; set;  }

        public DeadPlayer(PlayerControl killerId, PlayerControl playerId, DateTime killTime, DeathReason deathReason)
        {
            Killer = killerId;
            Victim = playerId;
            KillTime = killTime;
            DeathReason = deathReason;
        }
    }
}