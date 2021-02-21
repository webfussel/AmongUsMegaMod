using System;

namespace MegaMod
{
    public class DeadPlayer
    {
        public PlayerControl Killer { get; }
        public PlayerControl Victim { get; }
        public DateTime KillTime { get; }

        public DeadPlayer(PlayerControl killerId, PlayerControl playerId, DateTime killTime)
        {
            Killer = killerId;
            Victim = playerId;
            KillTime = killTime;
        }
    }
}