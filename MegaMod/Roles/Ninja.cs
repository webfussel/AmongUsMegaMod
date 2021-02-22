using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Ninja : Role
    {
        public static readonly byte RoleID = 106;
        
        public Ninja(PlayerControl player) : base(player)
        {
            name = "Ninja";
            color = new Color(0, 0, 0, 1);
            startText = "Double kill - Triple cooldown";
        }
        
        /**
         * Sets the Role if spawn chance is reached.
         * Can only set Role if crew still has space for Role.
         * Removes crew free space on successful assignment.
         */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptNinjaSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = Rng.Next(0, crew.Count);
            Ninja ninja = new Ninja(crew[random]);
            AddSpecialRole(ninja);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(ninja.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
        }

        protected override void SetConfigSettings()
        {
            // Do nothing for now
        }

        public override void CheckDead(HudManager instance)
        {
            // Do nothing for now
        }
    }
}