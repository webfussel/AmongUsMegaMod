using System;

namespace MegaMod
{
    public class DeadPlayer
    {
        public byte KillerId { get; }
        public byte PlayerId { get; }
        public DateTime KillTime { get; }
        public DeathReason DeathReason { get; }

        public DeadPlayer(byte killerId, byte playerId, DateTime killTime, DeathReason deathReason)
        {
            KillerId = killerId;
            PlayerId = playerId;
            KillTime = killTime;
            DeathReason = deathReason;
        }
    }
}